using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGame.BottomServer.Tools;
using Dlzyff.BoardGame.Protocol.Codes;
using Dlzyff.BoardGame.Protocol.Codes.GameCode;
using Dlzyff.BoardGame.Protocol.Dto;
using Dlzyff.BoardGameServer.Cache;
using Dlzyff.BoardGameServer.DataCache.Room;
using Dlzyff.BoardGameServer.DataCache.Services;
using Dlzyff.BoardGameServer.DataCache.Users;
using Dlzyff.BoardGameServer.Handles;
using Dlzyff.BoardGameServer.Log;
using Dlzyff.BoardGameServer.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dlzyff.BoardGameServer.LogicHandle
{
    /// <summary>
    /// 房间逻辑处理类
    /// </summary>
    public class RoomHandler : IHandler
    {

        /// <summary>
        /// 玩家数据缓存类对象
        /// </summary>
        private UserCache userCache = Caches.UserCache;

        /// <summary>
        /// 房间数据缓存类对象
        /// </summary>
        private RoomCache roomCache = Caches.RoomCache;

        /// <summary>
        /// 帕斯游戏业务数据缓存类对象
        /// </summary>
        private PasseServiceCache passeServiceCache = Caches.PasseServiceCache;

        public void OnDisconnect(ClientPeer clientPeer)
        {
            if (clientPeer != null)//如果要断开连接的客户端对象是可用状态
                clientPeer.OnDisconnect();//直接断开连接即可
        }

        public void OnReceiveMessage(ClientPeer clientPeer, int subOperationCode, object dataValue)
        {
            if (clientPeer == null || subOperationCode < 0 || string.IsNullOrEmpty(dataValue.ToString()))
                return;
            RoomCode roomCode = (RoomCode)Enum.Parse(typeof(RoomCode), subOperationCode.ToString());
            switch (roomCode)
            {
                case RoomCode.GetRoomList_Request:          //处理获取房间列表的业务请求
                    {
                        //获取房间列表
                        this.ProcessGetRoomListRequest(clientPeer);
                    }
                    break;
                case RoomCode.CreateRoom_Request:          //处理创建房间的业务请求
                    {
                        //创建房间
                        /*
                            这儿需要根据客户端传递过来的创建房间时的游戏类型数据,来进行一些相关的处理
                         */
                        this.ProcessCreateRoomRequest(clientPeer, dataValue);
                    }
                    break;
                case RoomCode.JoinRoom_Request:             //处理加入房间的业务请求
                    {
                        //这儿处理加入房间的时候,需要跟客户端确定传输时候的数据
                        //根据指定房间编号加入 还是 根据指定房间进入码加入 这是个问题 所以需要确定客户端向服务端发起加入房间请求时需要传输的数据
                        //可以这样设定 0 代表就是按照房间编号加入 1 代表 就是按照房间进入码加入
                        //构建的传输数据示例: 
                        //房间编号->0,1
                        //房间进入码->1,256474
                        //只要确定了数据,其它问题就迎刃而解了.
                        //加入房间
                        try
                        {
                            string[] joinRoomDataSplit = dataValue.ToString().Split(',');
                            string joinCode = joinRoomDataSplit[0];//加入房间码(用来识别是房间编号 还是 房间进入码)
                            string value = joinRoomDataSplit[1];
                            switch (joinCode)
                            {
                                case "0"://处理客户端根据指定房间编号的加入请求
                                    {
                                        //存储客户端对象要加入的指定的房间编号
                                        //校验客户端传递过来的数据是否成功转换成了整数类型的值
                                        if (int.TryParse(value.ToString(), out int roomId))//如果成功转换了,表示客户端要加入指定房间
                                            this.ProcessJoinRoomRequestByRoomId(clientPeer, roomId);
                                    }
                                    break;
                                case "1"://处理客户端根据房间进入码的加入请求
                                    {
                                        if (int.TryParse(value.ToString(), out int roomEnterCode))//如果成功转换了,表示客户端要根据房间进入码加入指定房间
                                            this.ProcessJoinRoomRequestByRoomEnterCode(clientPeer, roomEnterCode);//加入房间
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + " \n" + ex.StackTrace);
                        }
                    }
                    break;
                case RoomCode.Userready_Request:        //处理玩家准备的业务请求
                    {
                        //Todo:用户发起准备请求后,服务端需要根据请求的客户端对象 来进行数据保存
                        //只要服务端接到玩家发起的准备请求后 玩家肯定在房间内 只需要根据当前是哪个房间内的客户端对象即可
                        //如果客户端对象 发起准备请求后 设置一个标志位即可 
                        //加入房间
                        //存储客户端对象要加入的指定的房间编号
                        //校验客户端传递过来的数据是否成功转换成了整数类型的值
                        if (int.TryParse(dataValue.ToString(), out int roomId))//如果成功转换了,表示客户端要加入指定房间
                            this.ProcessUserreadyRequest(clientPeer, roomId);
                    }
                    break;
                case RoomCode.UserCancelready_Request:  //处理玩家取消准备的业务请求
                    {
                        //存储客户端对象要加入的指定的房间编号
                        //校验客户端传递过来的数据是否成功转换成了整数类型的值
                        if (int.TryParse(dataValue.ToString(), out int roomId))//如果成功转换了,表示客户端要加入指定房间
                            this.ProcessUserCancelreadyRequest(clientPeer, roomId);
                    }
                    break;
                case RoomCode.Startgame_Request:        //处理开始游戏的请求
                    {
                        //Todo:这儿需要判断当前房间内的所有玩家是否都已经准备了,如果都准备了,当客户端对象发起开始游戏的请求后,进行具体的开始游戏后的处理
                        //房间内的玩家全都准备好了 那么处理方式就是向所有房间内的客户端对象广播一条开始游戏的响应
                        //跟玩家满员自动开始游戏的原理实质上是差不多的
                        //存储客户端对象要加入的指定的房间编号
                        //校验客户端传递过来的数据是否成功转换成了整数类型的值
                        if (int.TryParse(dataValue.ToString(), out int roomId))//如果成功转换了,表示客户端要加入指定房间
                            this.ProcessStartGameRequest(clientPeer, roomId);
                    }
                    break;
                case RoomCode.RoomChat_Request:            //处理房间聊天的业务请求
                    {
                        //房间聊天
                        //this.ProcessRoomChatRequest(clientPeer, dataValue.ToString());//处理房间聊天
                    }
                    break;
                case RoomCode.LeaveRoom_Request:        //处理离开房间的业务请求
                    {
                        //离开房间
                        //判断客户端传递过来的房间编号值是否能够转换成功
                        if (int.TryParse(dataValue.ToString(), out int roomId))//如果转换成功了
                            this.ProcessLeaveRoomRequest(clientPeer, roomId);//则将这个客户端连接对象进行离开房间的处理
                    }
                    break;
                case RoomCode.DisbanadRoom_Request:     //处理解散房间的业务请求
                    {
                        //解释房间
                        //判断客户端传递过来的房间编号值是否能够转换成功
                        if (int.TryParse(dataValue.ToString(), out int roomId))//如果转换成功
                            this.ProcessDisbanadRoomRequest(clientPeer, roomId);//处理解散房间
                    }
                    break;
            }
        }

        #region 获取房间列表
        /// <summary>
        /// 处理客户端对象获取房间列表的请求
        /// </summary>
        /// <param name="clientPeer"></param>
        private void ProcessGetRoomListRequest(ClientPeer clientPeer)
        {
            if (clientPeer == null) return;
            List<RoomInfo> rooms = this.roomCache.Rooms;//取出所有房间
            string roomData = string.Empty;//用于存储所有房间的数据
            StringBuilder sb = new StringBuilder();//用于房间数据之间的拼接工作
            foreach (RoomInfo room in rooms)//遍历所有房间
                sb.Append(room.Id + "," + room.Name + "|");//进行房间数据之间的拼接
            if (sb.Length > 0)
                roomData = sb.ToString().Remove(sb.Length - 1, 1);
            //拼接完成后,将数据发送给请求获取房间列表的客户端对象
            clientPeer.OnSendMessage(new SocketMessage() { OperationCode = OperationCode.Room, SubOperationCode = (int)RoomCode.GetRoomList_Response, DataValue = roomData });
        }
        #endregion

        #region 创建房间
        /// <summary>
        /// 处理客户端对象创建房间的请求
        /// </summary>
        /// <param name="clientPeer">创建房间的客户端对象</param>
        /// <param name="createRoomData">客户端创建房间时传递过来的创建房间数据</param>
        private void ProcessCreateRoomRequest(ClientPeer clientPeer, object createRoomData = null)
        {
            RoomInfo newRoom = this.roomCache.CreateRoom(clientPeer);//通过房间数据缓存对象创建并返回一个新的房间信息对象
            newRoom.PersonNumber++;//累加房间人数

            #region 构建一个新的用户对象
            UserInfo userInfo = new UserInfo()
            {
                UserName = clientPeer.ClientSocket.RemoteEndPoint.ToString(),
                RoomId = newRoom.Id,
                Money = 5000,
                ClientUserSocket = clientPeer.ClientSocket,
                ClientIndex = this.roomCache.GetRoomClientIndexByRoomId(clientPeer, newRoom.Id)
            };
            this.userCache.AddUser(clientPeer, userInfo);//将构建好的用户对象保存起来
            userInfo.Id = this.userCache.GetUserIdByUserInfo(userInfo); //设置用户编号
            #endregion

            #region 将用户添加到房间内
            //将用户对象添加到新房间的客户端用户列表中去
            newRoom.UserInfos.Add(userInfo);
            #endregion

            #region 构建要传输的数据房间对象并把客户端需要的数据发送过去
            //构建房间信息传输数据对象(用于服务端把房间数据传输给客户端)
            RoomInfoDto roomInfoDto = new RoomInfoDto()
            {
                Id = newRoom.Id,
                EnterCode = newRoom.EnterCode,
                Name = newRoom.Name,
                PersonNumber = newRoom.PersonNumber
            };

            #region 循环取出当前房间内的用户数据
            //循环遍历房间内的客户端用户对象列表
            for (int userIndex = 0; userIndex < newRoom.UserInfos.Count; userIndex++)
            {
                UserInfo tmpUserInfo = newRoom.UserInfos[userIndex];//取得当前循环遍历到的客户端用户对象数据
                //构建一个用户信息传输数据添加至房间信息数据传输对象的用户列表中
                roomInfoDto.Users.Add
                    (
                        new UserInfoDto()
                        {
                            Id = tmpUserInfo.Id,
                            UserName = tmpUserInfo.UserName,
                            Money = tmpUserInfo.Money,
                            ClientIndex = this.roomCache.GetRoomClientIndexByRoomId(clientPeer, newRoom.Id)
                        }
                    );
            }
            #endregion

            #endregion

            #region 构建网络消息并发送给创建房间的客户端对象
            //构造服务端发送给客户端的网络消息对象
            SocketMessage message = new SocketMessage()
            {
                OperationCode = OperationCode.Room,
                SubOperationCode = (int)RoomCode.CreateRoom_Response,
                DataValue = roomInfoDto
            };
            clientPeer.OnSendMessage(message);//由于只有一个创建房间的客户端对象,所以直接给这一个客户端对象发送网络消息即可 
            #endregion

            LogMessage.Instance.SetLogMessage(
                string.Format("客户端对象唯一编号为 [ {0} ] 的用户创建了一个房间编号为 [ {1} ] 的新房间,这个房间的进入码为 [ {2} ]~", userInfo.Id.ToString(), newRoom.Id.ToString(), newRoom.EnterCode.ToString()));

            #region 创建房间后的 [洗牌] 和 [发牌] 阶段
            if (createRoomData != null)
            {
                //初始化卡牌数据
                //  this.InitCardsData();
                ////这里模拟创建房间后的一个洗牌过程
                // this.ResetCards();
                GameServiceTypeCode gameServiceTypeCode = (GameServiceTypeCode)Enum.Parse(typeof(GameServiceTypeCode), createRoomData.ToString());
                switch (gameServiceTypeCode)
                {
                    case GameServiceTypeCode.PasseService://帕斯业务初始发牌处理阶段
                        {
                            /*
                                    这里处理帕斯游戏的初始发牌过程
                                */
                            this.passeServiceCache.InitCardsData();//通过帕斯业务数据缓存对象初始化卡牌数据
                            this.passeServiceCache.ResetCards();//通过帕斯业务数据缓存对象重置卡牌数据
                            /*
                                    这里处理将客户端对象存储起来
                                */
                            this.passeServiceCache.AddClientPeer(clientPeer);//将客户端连接对象存储起来
                            /*
                                    这里处理将房间信息存储起来
                                */
                            this.passeServiceCache.AddRoomInfo(newRoom);//将新创建好的房间存储起来
                        }
                        break;
                    case GameServiceTypeCode.FivebombsWithSixbombsService://五轰六炸业务初始发牌处理阶段
                        {
                            /*
                                    这里处理五轰六炸游戏的初始发牌过程
                                */
                        }
                        break;
                    case GameServiceTypeCode.MahjongService://麻将业务初始发牌处理阶段
                        {
                            /*
                                    这里处理麻将游戏的初始发牌过程
                                */
                        }
                        break;
                }
            }
            #endregion

        }
        #endregion

        #region 加入房间

        /// <summary>
        /// 处理客户端对象加入房间的请求
        /// </summary>
        /// <param name="clientPeer"></param>
        private void ProcessJoinRoomRequest(ClientPeer clientPeer)
        {
            //获取要加入的房间
            RoomInfo tmpRoom = this.roomCache.JoinRoomByRoomId(clientPeer);

            #region 默认处理加入房间的测试代码
            //if (tmpRoom != null)//成功获取房间后
            //{
            //    this.passeServiceCache.AddClientPeer(clientPeer);//将客户端连接对象存储起来
            //    this.passeServiceCache.AddClientToInitPour(clientPeer);//初始化加入房间的玩家的赌注积分
            //    //构建一个客户端用户对象添加到房间内的客户端用户列表中去
            //    //构建一个用户对象
            //    UserInfo userInfo = new UserInfo()
            //    {
            //        Id = this.concurrentInteger.AddWithGet(),
            //        UserName = clientPeer.ClientSocket.RemoteEndPoint.ToString(),
            //        RoomId = tmpRoom.Id,
            //        Money = 5000,
            //        ClientUserSocket = clientPeer.ClientSocket
            //    };
            //    tmpRoom.UserInfos.Add(userInfo);//将用户存储添加到房间内
            //    this.passeServiceCache.ChangeRoomUserScoresDictionaryByRoomId(tmpRoom.Id, userInfo);//将用户存储起来

            //    #region 校验房间人数是否达到上限
            //    this.CheckPersonNumber(clientPeer, tmpRoom);//检查房间人数是否达到上限程度
            //    #endregion

            //} 
            #endregion

            this.ProcessJoinRoom(tmpRoom, clientPeer);//处理加入房间
        }

        /// <summary>
        /// 通过房间编号处理加入房间的业务请求
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        private void ProcessJoinRoomRequestByRoomId(ClientPeer clientPeer, int roomId)
        {
            //获取要加入的房间
            RoomInfo tmpRoom = this.roomCache.JoinRoomByRoomId(clientPeer, roomId);
            this.ProcessJoinRoom(tmpRoom, clientPeer);//加入房间
        }

        /// <summary>
        /// 通过房间进入码处理加入房间的业务请求
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomEnterCode"></param>
        private void ProcessJoinRoomRequestByRoomEnterCode(ClientPeer clientPeer, int roomEnterCode)
        {
            RoomInfo tmpRoom = this.roomCache.JoinRoomByRoomEnterCode(clientPeer, roomEnterCode);      //获取要加入的房间
            this.ProcessJoinRoom(tmpRoom, clientPeer);//加入房间
        }

        /// <summary>
        /// 处理加入房间
        /// </summary>
        /// <param name="roomInfo"></param>
        /// <param name="clientPeer"></param>
        private void ProcessJoinRoom(RoomInfo roomInfo, ClientPeer clientPeer)
        {
            if (roomInfo != null)//成功获取房间后
            {
                this.passeServiceCache.AddClientPeer(clientPeer);//将客户端连接对象存储起来
                //this.passeServiceCache.AddClientToInitPour(clientPeer);//初始化加入房间的玩家的赌注积分

                #region 这儿后期需要根据用户注册的用户数据从数据源中获取,而不是直接构建(比如从数据库服务器中获取)
                //构建一个客户端用户对象添加到房间内的客户端用户列表中去
                //构建一个用户对象
                UserInfo userInfo = new UserInfo()
                {
                    UserName = clientPeer.ClientSocket.RemoteEndPoint.ToString(),
                    RoomId = roomInfo.Id,
                    Money = 5000,
                    ClientUserSocket = clientPeer.ClientSocket,
                    ClientIndex = this.roomCache.GetRoomClientIndexByRoomId(clientPeer, roomInfo.Id)
                };

                this.userCache.AddUser(clientPeer, userInfo);//将创建好的用户数据保存起来
                userInfo.Id = this.userCache.GetUserIdByUserInfo(userInfo);//设置用户唯一编号

                roomInfo.UserInfos.Add(userInfo);//将用户存储添加到房间内
                #endregion

                this.passeServiceCache.ChangeRoomUserScoresDictionaryByRoomId(roomInfo.Id, userInfo);//将用户存储起来

                #region 校验房间人数是否达到上限
                this.CheckPersonNumber(clientPeer, roomInfo);//检查房间人数是否达到上限程度
                #endregion

            }
        }

        /// <summary>
        /// 处理校验房间人数
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="tmpRoom"></param>
        private void CheckPersonNumber(ClientPeer clientPeer, RoomInfo tmpRoom)
        {
            if (tmpRoom.UserInfos.Count < RoomCache.MAX_PERSON_NUMBER)//房间未满员 等待其它客户端的加入
            {
                tmpRoom.PersonNumber++;//累加房间人数
                LogMessage.Instance.SetLogMessage(
                             string.Format("客户端对象IP地址为 [ {0} ] 的用户加入了一个房间编号为 [ {1} ] 的房间,这个房间的进入码为 [ {2} ]~", clientPeer.ClientSocket.RemoteEndPoint.ToString(), tmpRoom.Id.ToString(), tmpRoom.EnterCode.ToString()));

                #region 构建要传输的数据房间对象并把客户端需要的数据发送过去
                //构建房间信息传输数据对象(用于服务端把房间数据传输给客户端)
                RoomInfoDto roomInfoDto = new RoomInfoDto()
                {
                    Id = tmpRoom.Id,
                    Name = tmpRoom.Name,
                    PersonNumber = tmpRoom.PersonNumber
                };
                //循环遍历房间内的客户端用户对象列表
                for (int userIndex = 0; userIndex < tmpRoom.UserInfos.Count; userIndex++)
                {
                    UserInfo userInfo = tmpRoom.UserInfos[userIndex];//取得当前循环遍历到的客户端用户对象数据
                    //构建一个用户信息传输数据添加至房间信息数据传输对象的用户列表中
                    roomInfoDto.Users.Add(
                        new UserInfoDto()
                        {
                            Id = userInfo.Id,
                            UserName = userInfo.UserName,
                            Money = userInfo.Money,
                            ClientIndex = this.roomCache.GetRoomClientIndexByRoomId(clientPeer, tmpRoom.Id)
                        });
                    LogMessage.Instance.SetLogMessage(userInfo.Id.ToString());
                }
                #endregion

                #region 构造网络消息对象 广播给房间内的每一个客户端对象
                //构造服务端发送给客户端的网络消息对象
                SocketMessage message = new SocketMessage()
                {
                    OperationCode = OperationCode.Room,
                    SubOperationCode = (int)RoomCode.JoinRoom_Response,
                    DataValue = roomInfoDto
                };
                //通过房间数据缓存对象根据指定房间编号进行房间内的消息广播,将网络消息发送给每一个房间内的客户端用户对象
                this.roomCache.BroadcastMessageByRoomId(tmpRoom.Id, message);
                #endregion

                #region 给房主广播一条空数据(没有实际作用,只是为了房主的数据同步工作,不会影响其他客户端对象)
                RoomInfo tmpRoomInfo = this.roomCache.RoomIdRooms[tmpRoom.Id];//获取房间信息对象
                List<ClientPeer> clients = this.roomCache.RoomClientsDict[tmpRoomInfo];//通过房间信息对象获取该房间内的客户端用户列表
                if (clients != null)//如果不为空的情况下     
                    //默认给房主发送一个null数据(之前不这样做,会导致房主的数据不进行同步处理,出现了Bug,之后这样做,竟然神奇地解决了)
                    clients[0].OnSendMessage(
                        new SocketMessage()
                        {
                            OperationCode = OperationCode.Service,
                            SubOperationCode = (int)ServiceCode.Passe_Response,
                            DataValue = "null"
                        });
                #endregion

                #region 房间满员的处理
                this.CheckPersonNumberIsFull(clientPeer, tmpRoom, clients);//检查房间人数是否满员
                #endregion

            }
        }

        /// <summary>
        /// 处理校验房间人数是否满员
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="tmpRoom"></param>
        /// <param name="clients"></param>
        private void CheckPersonNumberIsFull(ClientPeer clientPeer, RoomInfo tmpRoom, List<ClientPeer> clients)
        {
            /*
                针对房间内的特殊情况处理：
                客户端可能要加入一个设定选项,人满直接开 或者 是房主点击开始游戏按钮后 直接进行开始
                
                第一种情况就是人满直接开
                第二种情况就是等待房主按下开始游戏按钮 之后再进行开始游戏
             */

            #region 第一种情况处理 -> 根据人数满员的状态下来处理游戏开始
            if (tmpRoom.PersonNumber == 3)//如果房间人数等于3 代表房间已经满员
                this.ProcessStartGame(clientPeer, tmpRoom);
            #endregion

        }
        #endregion

        #region 准备 
        /// <summary>
        /// 处理玩家准备的业务请求
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        private void ProcessUserreadyRequest(ClientPeer clientPeer, int roomId)
        {
            this.roomCache.Ready(clientPeer, roomId);//使用房间数据缓存对象进行指定房间内的玩家准备
        }
        #endregion

        #region 取消准备
        /// <summary>
        /// 处理玩家取消准备的业务请求
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        private void ProcessUserCancelreadyRequest(ClientPeer clientPeer, int roomId)
        {
            this.roomCache.CancelReady(clientPeer, roomId);
        }
        #endregion

        #region 开始游戏

        /// <summary>
        /// 处理开始游戏的业务请求
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        private void ProcessStartGameRequest(ClientPeer clientPeer, int roomId)
        {
            if (this.roomCache.IsCanStartGame(roomId))//如果可以开始游戏
                this.ProcessStartGame(clientPeer, this.roomCache.GetRoomInfoByRoomId(roomId));//处理开始游戏
        }

        /// <summary>
        /// 处理开始游戏
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="tmpRoom"></param>
        private void ProcessStartGame(ClientPeer clientPeer, RoomInfo tmpRoom)
        {
            #region 响应游戏开始
            //构造网络消息
            //将数据发送给客户端对象(数据指的是牌的数据)
            SocketMessage broMsg = new SocketMessage()
            {
                OperationCode = OperationCode.Room,
                SubOperationCode = (int)RoomCode.StartGame_BroadcastResponse,
                DataValue = tmpRoom.UserInfos.Count.ToString()
            };
            this.roomCache.BroadcastMessageByRoomId(tmpRoom.Id, broMsg);//广播消息给所有房间内的客户端
            #endregion

            #region 模拟荷官给房间内的客户端对象发牌
            //this.Deal(clients);//处理发牌
            //人满之后 进行发牌处理
            //第一次发牌,需要给指定房间内的每一个客户端对象发一张底牌数据
            this.passeServiceCache.GetTouchCardByRoomId(tmpRoom.Id);//根据指定房间编号分发底牌数据
            //第二次发牌,需要给指定房间内的每一个客户端对象发一张明牌数据
            this.passeServiceCache.GetClearCardByRoomId(tmpRoom.Id);
            //打印显示当前房间内的分数情况
            this.passeServiceCache.DisplayAllUserScoresByRoomId(tmpRoom.Id);
            //这里模拟比较分数大小
            this.passeServiceCache.CompleteUserScoreByRoomId(tmpRoom.Id);
            #endregion
        }

        #endregion

        #region 离开房间
        /// <summary>
        /// 处理客户端对象离开房间的请求
        /// </summary>
        /// <param name="clientPeer">离开房间的客户端连接对象</param>
        /// <param name="roomId">要离开的房间编号</param>
        private void ProcessLeaveRoomRequest(ClientPeer clientPeer, int roomId)
        {
            #region 移除一些跟客户端对象引用相关的东东
            //需要断开要移除的客户端连接对象所有的引用指针(防止程序出现不正常的现象)
            this.passeServiceCache.RemoveUserScoreofUserScoreDict(clientPeer);//通过帕斯业务缓存对象从用户积分字典中移除指定地客户端连接对象
            this.passeServiceCache.SubClientPeer(clientPeer);//通过帕斯业务对象从客户端连接对象列表中移除客户端连接对象
            //Todo:中间可能还有其他模块部分需要移除客户端连接对象的引用指针
            //最后才需要从当前玩家要离开的房间中移除客户端对象
            RoomInfo tmpRoom = this.roomCache.LeaveRoom(clientPeer, roomId);
            //从房间中移除以后,需要广播消息给其他还在房间内的客户端对象,通知他们哪个玩家离开了房间
            //广播消息给其他客户端对象之后,需要将当前的房间数据重新广播给每一个在房间内的客户端对象 
            #endregion

            #region 构建要传输的数据房间对象并把客户端需要的数据发送过去
            //构建房间信息传输数据对象(用于服务端把房间数据传输给客户端)
            RoomInfoDto roomInfoDto = new RoomInfoDto()
            {
                Id = tmpRoom.Id,
                Name = tmpRoom.Name,
                PersonNumber = tmpRoom.PersonNumber
            };
            //循环遍历房间内的客户端用户对象列表
            for (int userIndex = 0; userIndex < tmpRoom.UserInfos.Count; userIndex++)
            {
                UserInfo userInfo = tmpRoom.UserInfos[userIndex];//取得当前循环遍历到的客户端用户对象数据
                                                                 //构建一个用户信息传输数据添加至房间信息数据传输对象的用户列表中
                roomInfoDto.Users.Add(
                    new UserInfoDto()
                    {
                        Id = userInfo.Id,
                        UserName = userInfo.UserName,
                        Money = userInfo.Money,
                        ClientIndex = tmpRoom.PersonNumber + 1
                    });
            }
            #endregion

            #region 构造网络消息对象 广播给房间内的每一个客户端对象 和 当前要离开房间的客户端对象
            //构造服务端发送给客户端的网络消息对象
            SocketMessage message = new SocketMessage()
            {
                OperationCode = OperationCode.Room,
                SubOperationCode = (int)RoomCode.LeaveRoom_BroadcastResponse,
                DataValue = roomInfoDto
            };
            this.ProcessBroadcastMessage(clientPeer, roomId, message);
            #endregion

            #region 给房主广播一条空数据(没有实际作用,只是为了房主的数据同步工作,不会影响其他客户端对象)
            RoomInfo tmpRoomInfo = this.roomCache.RoomIdRooms[tmpRoom.Id];//获取房间信息对象
            List<ClientPeer> clients = this.roomCache.RoomClientsDict[tmpRoomInfo];//通过房间信息对象获取该房间内的客户端用户列表
            if (clients != null)//如果不为空的情况下
                                //默认给房主发送一个null数据(之前不这样做,会导致房主的数据不进行同步处理,出现了Bug,之后这样做,竟然神奇地解决了)
                clients[0].OnSendMessage(
                    new SocketMessage()
                    {
                        OperationCode = OperationCode.Service,
                        SubOperationCode = (int)ServiceCode.Passe_Response,
                        DataValue = "null"
                    });
            #endregion
        }
        #endregion

        #region 解散房间
        /// <summary>
        /// 处理解散房间的业务请求
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        private void ProcessDisbanadRoomRequest(ClientPeer clientPeer, int roomId)
        {
            //解散房间的时候 判断 当前是不是房主解散了 房间
            //如果不是房主解散的房间 ,则解散房间的前置条件不成立 不能解散房间
            bool isHouseOwner = this.roomCache.IsHouseOwner(clientPeer, roomId);
            if (isHouseOwner)
            {
                //通知每一个房间内的客户端对象 房间被房主解散了
                #region 构造网络消息对象 广播给房间内的每一个客户端对象 和 当前要离开房间的客户端对象
                //构造服务端发送给客户端的网络消息对象
                SocketMessage message = new SocketMessage()
                {
                    OperationCode = OperationCode.Room,
                    SubOperationCode = (int)RoomCode.DisbanadRoom_BroadcastResponse,
                    DataValue = "解散房间"
                };
                this.ProcessBroadcastMessage(clientPeer, roomId, message);
                #endregion
                //通知完成后 需要将房间内的数据清空处理
                this.roomCache.DisbanadRoom(roomId);//先将房间内的数据移除处理
            }
        }


        #endregion

        #region 房间聊天
        /// <summary>
        /// 处理客户端对象房间聊天请求
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="chatContentMsg">聊天内容</param>
        private void ProcessRoomChatRequest(ClientPeer clientPeer, int roomId, string chatContentMsg)
        {
            RoomInfo tmpRoom = this.roomCache.GetRoomInfoByRoomId(roomId);
            if (tmpRoom != null)
            {
                SocketMessage message = new SocketMessage()
                {
                    OperationCode = OperationCode.Room,
                    SubOperationCode = (int)RoomCode.RoomChat_BroadcastResponse,
                    DataValue = chatContentMsg
                };
                this.roomCache.BroadCastMessageByExClient(clientPeer, tmpRoom.Id, message);
            }
        }
        #endregion

        #region 房间内消息广播
        /// <summary>
        /// 处理广播消息
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        /// <param name="message"></param>
        private void ProcessBroadcastMessage(ClientPeer clientPeer, int roomId, SocketMessage message)
        {
            //在离开房间之前需要通知这个离开房间的客户端对象
            clientPeer.OnSendMessage(message);
            //通过房间数据缓存对象根据指定房间编号进行房间内的消息广播,将网络消息发送给每一个房间内的客户端用户对象
            this.roomCache.BroadcastMessageByRoomId(roomId, message);
        }
        #endregion
    }
}
