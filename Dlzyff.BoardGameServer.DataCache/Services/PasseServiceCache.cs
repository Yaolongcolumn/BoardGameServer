using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGame.BottomServer.Tools;
using Dlzyff.BoardGame.Protocol.Codes;
using Dlzyff.BoardGame.Protocol.Codes.GameCode;
using Dlzyff.BoardGameServer.Log;
using Dlzyff.BoardGameServer.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dlzyff.BoardGameServer.DataCache.Services
{
    /// <summary>
    /// 帕斯业务用户分数码(用来标注当前设置或者比较的用户分数形式,是明牌还是底牌)
    /// </summary>
    public enum PasseServiceUserScoreCode
    {
        明牌,
        底牌,
        底明牌
    }
    /// <summary>
    /// 帕斯业务缓存类
    /// </summary>
    public class PasseServiceCache : PokerServiceCache
    {

        /// <summary>
        /// 用来存储一局游戏房间内的客户端用户底牌分数信息数据字典
        /// </summary>
        private Dictionary<RoomInfo, Dictionary<UserInfo, int>> roomUserTouchCardScoreDict = new Dictionary<RoomInfo, Dictionary<UserInfo, int>>();

        /// <summary>
        /// 用来存储一局游戏房间内的客户端用户明牌分数信息数据字典
        /// </summary>
        private Dictionary<RoomInfo, Dictionary<UserInfo, int>> roomUserClearCardScoreDict = new Dictionary<RoomInfo, Dictionary<UserInfo, int>>();

        /// <summary>
        /// 用来存储一局游戏房间内用户的分数信息数据字典
        /// </summary>
        private Dictionary<UserInfo, int> userScoreDict = new Dictionary<UserInfo, int>();//存储用户分数的数据字典

        /// <summary>
        /// 用来游戏房间内跟牌的玩家的数据字典
        /// </summary>
        private Dictionary<RoomInfo, Dictionary<ClientPeer, bool>> roomFollowcardUserDict = new Dictionary<RoomInfo, Dictionary<ClientPeer, bool>>();

        /// <summary>
        /// 用来存储游戏房间内弃牌的玩家的数据字典(一次游戏结束后,需要将数据清空)
        /// </summary>
        private Dictionary<RoomInfo, List<ClientPeer>> roomDiscardDict = new Dictionary<RoomInfo, List<ClientPeer>>();

        /// <summary>
        /// 用来存储游戏房间内发牌的次数数据字典(第三轮以后,给玩家发牌的时候,需要记录一下,其实就是和每轮跟牌的玩家做一个对应操作,每次有玩家跟牌的时候需要对比一下)
        /// </summary>
        private Dictionary<RoomInfo, int> roomPlayCardIndexDict = new Dictionary<RoomInfo, int>();

        /// <summary>
        /// 网络消息
        /// </summary>
        private SocketMessage message = new SocketMessage();

        #region 初始化和重置卡牌数据
        /// <summary>
        /// 初始卡牌数据
        /// </summary>
        public sealed override void InitCardsData()
        {
            base.InitCardsData();
            this.cardValues = new string[] { "Nine", "Ten", "Jack", "Queen", "King", "One" };
            this.roomUserClearCardScoreDict.Clear();//每次初始化卡牌数据的时候需要把存储分数的数据字典做一个清空数据的操作
            this.roomDiscardDict.Clear();//每次初始化卡牌数据的时候需要把存储弃牌玩家的数据字典做一个清空数据的操作
        }
        #endregion

        #region 随机获取卡牌数据
        /// <summary>
        /// 获取随机卡牌
        /// </summary>
        /// <returns></returns>
        public override string GetRandomCard()
        {
            if (this.resCards.Count == 0)
            {
                return "没有扑克牌可以抓取了~";
            }
            int index = this.ranCardIndex.Next(0, this.resCards.Count);
            string cardStr = this.resCards[index];
            this.resCards.RemoveAt(index);
            return cardStr;
        }
        #endregion

        #region 针对房间内玩家的操作

        #region 添加房间信息
        /// <summary>
        /// 向当前模块存储添加房间信息(其实也就是将房间信息保存在当前业务模块中进行缓存处理)
        /// </summary>
        /// <param name="roomInfo">要添加的房间信息</param>
        public override void AddRoomInfo(RoomInfo roomInfo)
        {
            #region 添加玩家的底牌数据信息
            Dictionary<UserInfo, int> userTouchCardScoreDict = new Dictionary<UserInfo, int>();//构建一个用于存储用户信息 对应 用户分数的数据字典
            foreach (UserInfo user in roomInfo.UserInfos)//循环遍历房间内的每一个玩家
            {
                if (!userTouchCardScoreDict.ContainsKey(user))
                    userTouchCardScoreDict.Add(user, 0);//添加到存储用户信息 和 分数 的字典中
            }
            if (!this.roomUserTouchCardScoreDict.ContainsKey(roomInfo))
                this.roomUserTouchCardScoreDict.Add(roomInfo, userTouchCardScoreDict);
            else
                this.roomUserTouchCardScoreDict[roomInfo] = userTouchCardScoreDict;
            #endregion

            #region 添加玩家的明牌数据信息
            Dictionary<UserInfo, int> userClearCardScoreDict = new Dictionary<UserInfo, int>();//构建一个用于存储用户信息 对应 用户分数的数据字典
            foreach (UserInfo user in roomInfo.UserInfos)//循环遍历房间内的每一个玩家
            {
                if (!userClearCardScoreDict.ContainsKey(user))
                    userClearCardScoreDict.Add(user, 0);//添加到存储用户信息 和 分数 的字典中
            }
            if (!this.roomUserClearCardScoreDict.ContainsKey(roomInfo))
                this.roomUserClearCardScoreDict.Add(roomInfo, userClearCardScoreDict);//将房间和用户信息存储起来;
            else
                this.roomUserClearCardScoreDict[roomInfo] = userClearCardScoreDict;
            #endregion

            #region 添加玩家跟牌的数据信息
            Dictionary<ClientPeer, bool> userFollowcardDict = new Dictionary<ClientPeer, bool>();
            if (!this.roomFollowcardUserDict.ContainsKey(roomInfo))
                this.roomFollowcardUserDict.Add(roomInfo, userFollowcardDict);
            else
                this.roomFollowcardUserDict[roomInfo] = userFollowcardDict;
            #endregion
        }
        #endregion

        #region 根据房间编号更改存储玩家分数的数据字典
        /// <summary>
        /// 通过房间编号更改存储用户信息和用户底牌牌分数的数据字典
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userinfoArray"></param>
        public void ChangeRoomUserTouchCardScoresDictionaryByRoomId(int roomId, params UserInfo[] userinfoArray)
        {
            RoomInfo tmpRoomInfo = this.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间信息数据
            if (tmpRoomInfo != null)//如果获取到了房间
            {
                for (int userIndex = 0; userIndex < userinfoArray.Length; userIndex++)//使用循环遍历新进房间来的用户
                    this.roomUserTouchCardScoreDict[tmpRoomInfo].Add(userinfoArray[userIndex], 0);//将他们添加到数据字典中
            }
        }

        /// <summary>
        /// 通过房间编号更改存储用户信息和用户明牌分数的数据字典
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userinfoArray"></param>
        public void ChangeRoomUserClearCardScoresDictionaryByRoomId(int roomId, params UserInfo[] userinfoArray)
        {
            RoomInfo tmpRoomInfo = this.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间信息数据
            if (tmpRoomInfo != null)//如果获取到了房间
            {
                for (int userIndex = 0; userIndex < userinfoArray.Length; userIndex++)//使用循环遍历新进房间来的用户
                    this.roomUserClearCardScoreDict[tmpRoomInfo].Add(userinfoArray[userIndex], 0);//将他们添加到数据字典中
            }
        }
        #endregion

        #region 通过房间编号更改存储用户信息和用户是否跟牌的数据字典
        /// <summary>
        /// 通过房间编号更改存储用户信息和用户是否跟牌的数据字典
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="clients"></param>
        public void ChangeRoomFollowcardUserDictionaryByRoomId(int roomId, params ClientPeer[] clients)
        {
            RoomInfo tmpRoomInfo = this.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间信息数据
            if (tmpRoomInfo != null)//如果获取到了房间
            {
                for (int clientIndex = 0; clientIndex < clients.Length; clientIndex++)//使用循环遍历新进房间来的用户
                    this.roomFollowcardUserDict[tmpRoomInfo].Add(clients[clientIndex], true);//将他们添加到数据字典中

                int followUserCount = 0;//记录跟牌的玩家个数
                foreach (KeyValuePair<RoomInfo, Dictionary<ClientPeer, bool>> roomItem in this.roomFollowcardUserDict)
                {
                    foreach (KeyValuePair<ClientPeer, bool> clientItem in roomItem.Value)
                    {
                        if (clientItem.Value == true)//如果等于true 就表示跟牌
                            followUserCount++;//记录跟牌的玩家个数
                        else//如果不等于true 就表示不跟牌
                            continue;//直接使用continue 关键字 终止本次循环 开始下次循环
                    }
                }

                #region 用来判断一轮游戏是否结束(也就是是否可以开始下一轮游戏了)
                if (followUserCount == this.roomPlayCardIndexDict[tmpRoomInfo])//如果跟牌玩家的个数和玩家出牌的索引对应(则进行下一轮发牌的操作)
                {
                    LogMessage.Instance.SetLogMessage("跟牌的玩家个数和玩家出牌的索引值一致,开始进行下一轮发牌的工作~");
                    this.GetClearCardByRoomId(roomId);//再发一轮明牌
                }
                #endregion
            }
        }

        /// <summary>
        /// 根据房间编号设置不跟牌的玩家
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="clients"></param>
        public void SetRoomNoFollowUserByRoomId(int roomId, params ClientPeer[] clients)
        {
            RoomInfo tmpRoomInfo = this.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间信息数据
            if (tmpRoomInfo != null)//如果获取到了房间
            {
                for (int clientIndex = 0; clientIndex < clients.Length; clientIndex++)//使用循环遍历新进房间来的用户
                    this.roomFollowcardUserDict[tmpRoomInfo][clients[clientIndex]] = false;
            }
        }
        #endregion

        #region 分发底牌[给所有玩家分发]
        /// <summary>
        /// 根据房间编号给房间内的客户端用户发底牌数据(只需在一局游戏中第一轮发)
        /// </summary>
        /// <param name="roomId"></param>
        public void GetTouchCardByRoomId(int roomId)
        {
            LogMessage.Instance.SetLogMessage("没发底牌之前剩余的牌数：" + this.resCards.Count.ToString());
            RoomInfo tmpRoomInfo = GetRoomInfoByRoomId(roomId);
            if (tmpRoomInfo != null)//如果不为空的话 表示根据房间编号取到了对应的房间数据
            {
                this.RecordsNumberByRoomId(roomId);//记录游戏次数
                ClientPeer tmpClientPeer = null;//临时存储要发牌的客户端连接代码
                //LogMessage.Instance.SetLogMessage("要发底牌的房间中的用户个数：" + tmpRoomInfo.UserInfos.Count.ToString());
                foreach (UserInfo user in tmpRoomInfo.UserInfos)//循环遍历这个房间中的客户端用户列表
                {
                    //判断用户是否为空
                    if (user != null)//如果不为空,再进行具体的发牌细节处理
                    {
                        tmpClientPeer = this.clientPeers.Find(clientPeer => clientPeer.ClientSocket == user.ClientUserSocket);//校验是不是同一个客户端连接对象
                        if (tmpClientPeer != null)//如果不为空 则表示是同一个客户端连接对象
                        {
                            string tmpCardValue = this.GetRandomCard();//随机取出一张牌(这张牌发给当前客户端对象)
                            //根据房间编号和用户信息记录用户分数
                            this.SetUserScoreByRoomId(roomId, user, tmpCardValue, PasseServiceUserScoreCode.底牌);
                            //这里需要发送两次消息(第一次是当前客户端自己的底牌数据,第二次是要给这个客户端发送一些假数据,表示是其他客户端对象的底牌数据)
                            LogMessage.Instance.SetLogMessage("玩家[ " + tmpClientPeer.ClientSocket.RemoteEndPoint.ToString() + " ]发到的底牌-> " + tmpCardValue + "," + user.ClientIndex.ToString());

                            #region 给客户端对象发送消息
                            //第一次发消息:
                            //给当前客户端对象先发一张自己的底牌
                            this.message.ChangeMessage(OperationCode.Service, (int)ServiceCode.Passe_Response, tmpCardValue + "," + user.ClientIndex.ToString());
                            tmpClientPeer.OnSendMessage(this.message);//发送真的底牌数据给客户端对象
                            //循环遍历所有客户端对象 找到不是当前发底牌的客户端对象
                            for (int clientPeerIndex = 0; clientPeerIndex < this.clientPeers.Count; clientPeerIndex++)
                            {
                                ClientPeer clientPeer = this.clientPeers[clientPeerIndex];
                                if (clientPeer != tmpClientPeer)//如果不是当前客户端对象
                                {
                                    //第二次发消息:
                                    //给当前抽到底牌的客户端对象发送假数据
                                    this.message.ChangeMessage(OperationCode.Service, (int)ServiceCode.Passe_Response, "底牌" + "," + user.ClientIndex.ToString());
                                    tmpClientPeer.OnSendMessage(this.message);//发送假的底牌数据给客户端对象
                                }
                            }
                            #endregion

                        }
                    }
                }
            }
            LogMessage.Instance.SetLogMessage("发了底牌之前剩余的牌数：" + this.resCards.Count.ToString());
        }
        #endregion

        #region 分发明牌[给所有玩家分发]
        /// <summary>
        /// 根据房间编号给房间内的客户端用户发明牌数据(只需在一局游戏中第二轮发)
        /// </summary>
        /// <param name="roomId"></param>
        public void GetClearCardByRoomId(int roomId)
        {
            LogMessage.Instance.SetLogMessage("没发明牌之前剩余的牌数：" + this.resCards.Count.ToString());
            RoomInfo tmpRoomInfo = this.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间信息数据
            if (tmpRoomInfo != null)//如果不为空 则表示通过房间编号成功获取到了房间信息数据
            {
                this.RecordsNumberByRoomId(roomId);//记录游戏次数
                StringBuilder cardDataSb = new StringBuilder();//用于拼接要发送的卡牌数据
                string cardData = string.Empty;//存储要发送的卡牌数据
                for (int userIndex = 0; userIndex < tmpRoomInfo.UserInfos.Count; userIndex++)//循环遍历房间内的用户列表
                {
                    UserInfo user = tmpRoomInfo.UserInfos[userIndex];//取得当前遍历到的用户
                    if (user != null)//如果不为空了 成功取到了用户
                    {
                        ClientPeer clientPeer = this.clientPeers.Find(peer => peer.ClientSocket == user.ClientUserSocket);//取得跟当前遍历到的用户 相符 的客户端连接对象
                        string tmpCard = this.GetRandomCard();//随机取出一张卡牌
                        //记录分数
                        this.SetUserScoreByRoomId(roomId, user, tmpCard, PasseServiceUserScoreCode.明牌);
                        if (!string.IsNullOrEmpty(tmpCard))//如果取得卡牌不为空的情况下
                        {
                            LogMessage.Instance.SetLogMessage("玩家[ " + clientPeer.ClientSocket.RemoteEndPoint.ToString() + " ]发到的明牌-> " + tmpCard + "," + user.ClientIndex.ToString());
                            cardDataSb.Append(tmpCard + "," + user.ClientIndex.ToString() + "|");//拼接要发送给房间内的所有玩家的卡牌数据
                        }
                        else//如果取到的卡牌为空(这里可能会通知房间内的所有客户端用户对象,告诉它们本局基本牌数已经发完,可能需要开始计算分数了)
                            LogMessage.Instance.SetLogMessage("本局中已经没有卡牌了~");
                    }
                }
                if (cardDataSb.Length > 0)
                    cardData = cardDataSb.ToString().Remove(cardDataSb.Length - 1, 1);
                this.message.ChangeMessage(OperationCode.Service, (int)ServiceCode.Passe_BroadcastResponse, cardData);//更改网络消息
                this.roomCache.BroadcastMessageByRoomId(roomId, this.message);//将发完的一轮明牌广播给房间内的所有客户端对象
            }
            else
                return;
            LogMessage.Instance.SetLogMessage("发了明牌之前剩余的牌数：" + this.resCards.Count.ToString());
        }
        #endregion

        #region 计算分数
        /// <summary>
        /// 根据房间编号比较房间内的客户端用户分数
        /// </summary>
        /// <param name="roomId">要进行比较的房间</param>
        /// <param name="userScoreCode">用户分数码</param>
        public void CompleteUserScoreByRoomId(int roomId, PasseServiceUserScoreCode userScoreCode)
        {
            RoomInfo tmpRoomInfo = this.GetRoomInfoByRoomId(roomId);
            if (tmpRoomInfo != null)
            {
                UserInfo userInfo = null;//存储分数最大的玩家信息数据
                int maxUserScore = int.MinValue;//存储最大分数
                switch (userScoreCode)
                {
                    case PasseServiceUserScoreCode.明牌://仅处理比较明牌
                        {
                            #region 记录玩家的明牌分数
                            //这里遍历明牌的玩家分数数据字典
                            foreach (KeyValuePair<UserInfo, int> userItem in this.roomUserClearCardScoreDict[tmpRoomInfo])
                            {
                                int tmpUserScore = userItem.Value;
                                userInfo = userItem.Key;
                                //判断用户存不存在
                                if (!userScoreDict.ContainsKey(userInfo))//不存在
                                    userScoreDict.Add(userInfo, tmpUserScore);//添加
                                else//存在
                                    userScoreDict[userInfo] += tmpUserScore;//累加并重新赋值
                            }
                            #endregion
                        }
                        break;
                    case PasseServiceUserScoreCode.底明牌://处理比较底牌 + 明牌
                        {
                            #region 记录玩家的底牌分数
                            //这里遍历底牌的玩家分数数据字典
                            foreach (KeyValuePair<UserInfo, int> userItem in this.roomUserTouchCardScoreDict[tmpRoomInfo])
                            {
                                int tmpUserScore = userItem.Value;//取得当前遍历到的用户分数
                                userInfo = userItem.Key;//保存当前遍历到的用户
                                //判断用户存不存在
                                if (!userScoreDict.ContainsKey(userInfo))//不存在
                                    userScoreDict.Add(userInfo, tmpUserScore);//添加
                                else//存在
                                    userScoreDict[userInfo] += tmpUserScore;//累加并重新赋值
                            }
                            #endregion

                            #region 记录玩家的明牌分数
                            //这里遍历明牌的玩家分数数据字典
                            foreach (KeyValuePair<UserInfo, int> userItem in this.roomUserClearCardScoreDict[tmpRoomInfo])
                            {
                                int tmpUserScore = userItem.Value;
                                userInfo = userItem.Key;
                                //判断用户存不存在
                                if (!userScoreDict.ContainsKey(userInfo))//不存在
                                    userScoreDict.Add(userInfo, tmpUserScore);//添加
                                else//存在
                                    userScoreDict[userInfo] += tmpUserScore;//累加并重新赋值
                            }
                            #endregion
                        }
                        break;
                }

                #region 判断谁是最大分数玩家
                //这里遍历存储用户分数的数据字典
                foreach (KeyValuePair<UserInfo, int> userItem in userScoreDict)
                {
                    int tmpScore = userItem.Value;//取得当前遍历到的玩家分数
                    if (tmpScore > maxUserScore)//如果大于最大分数
                    {
                        maxUserScore = tmpScore;//设置最大分数
                        userInfo = userItem.Key;//设置最大分数的玩家
                    }
                }
                #endregion

                #region 显示所有玩家的分数值
                //这里循环遍历 底牌 或者 明牌的分数数据字典都可以,因为存储的玩家都是保持一致的
                foreach (KeyValuePair<UserInfo, int> userItem in this.roomUserClearCardScoreDict[tmpRoomInfo])
                {
                    if (this.userScoreDict.ContainsKey(userItem.Key))//如果存储用户分数的数据字典中存在这个玩家
                        LogMessage.Instance.SetLogMessage("玩家[ " + userItem.Key.UserName + " ] 当前一共获得了[ " + this.userScoreDict[userItem.Key] + " ]分数~");
                    else
                        continue;
                }
                #endregion

                #region 推送比较后的消息结果
                ClientPeer tmpClientPeer = this.roomCache.GetClientPeerByUserInfo(userInfo);//根据用户信息获取客户端连接对象
                if (tmpClientPeer != null)//如果获取到了
                {
                    LogMessage.Instance.SetLogMessage(userInfo.UserName + " 取得了游戏胜利~");
                    //给获胜的玩家发送胜利消息
                    this.message.ChangeMessage(OperationCode.GameResult, (int)GameResultCode.Game_Success_Response, "你的分数最大,你胜利了~" + "," + userInfo.ClientIndex.ToString());//消息格式:胜利|失败的消息,胜利的客户端索引
                    tmpClientPeer.OnSendMessage(this.message);//发送给这一局游戏胜利的玩家,通知他获取了这一局游戏的胜利
                    //给失败的玩家发送消息
                    this.message.ChangeMessage(OperationCode.GameResult, (int)GameResultCode.Game_Faild_Response, "你失败了~胜利的玩家是 [" + userInfo.ClientIndex.ToString() + "] ," + userInfo.ClientIndex.ToString());//消息格式:胜利|失败的消息,胜利的客户端索引
                    this.roomCache.BroadCastMessageByExClient(tmpClientPeer, tmpRoomInfo.Id, this.message);//广播给所有失败的客户端对象,并通知他们哪个客户端这一局游戏获取了最终的胜利
                }
                #endregion
            }
        }
        #endregion

        #region 设置弃牌玩家
        /// <summary>
        /// 通过房间编号设置弃牌玩家
        /// </summary>
        /// <param name="roomId">玩家弃牌的房间编号</param>
        /// <param name="clientPeerArray">弃牌的玩家数组</param>
        public void SetDiscardUserByRoomId(int roomId, params ClientPeer[] clientPeerArray)
        {
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间信息数据
            //判断存不存在这个房间数据
            if (!this.roomDiscardDict.ContainsKey(roomInfo))//如果不存在先进行添加,然后存储已经弃牌的玩家
                this.roomDiscardDict.Add(roomInfo, clientPeerArray.ToList());
            else//已经存在的话
                this.roomDiscardDict[roomInfo].AddRange(clientPeerArray.ToList());//重新存储添加弃牌玩家
            this.SetRoomNoFollowUserByRoomId(roomId, clientPeerArray);//设置不跟玩家
        }
        #endregion

        #region 记录游戏局数
        /// <summary>
        /// 根据房间编号记录游戏局数
        /// </summary>
        /// <param name="roomId">要记录的房间编号数据</param>
        public void RecordsNumberByRoomId(int roomId)
        {
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间信息数据
            if (roomInfo == null) return;
            if (!this.roomRecordsNumberDict.ContainsKey(roomInfo))//如果记录房间的游戏局数中不存在房间信息 
                this.roomRecordsNumberDict.Add(roomInfo, 1);//直接存储
            else//如果存在
                this.roomRecordsNumberDict[roomInfo]++;//直接累加游戏局数后进行存储
            if (this.roomRecordsNumberDict[roomInfo] == 5)//如果记录的游戏次数已经到达5次,则表示已经完成了一局游戏
            {
                //Todo：
                //服务端给客户端发送响应消息,推送给客户端游戏结果,比如哪个玩家获胜了,客户端需要对界面的显示数据进行更新
                //在帕斯游戏业务模块,如果记录的次数满五次以后,需要将房间恢复初始化时候的状态
                //以下的操作,只要在房间不解散的情况下,是一个无限循环的过程
                //也就是清除刚刚一局游戏结束时的一些临时缓存数据
                //继续等待客户端玩家对象发起准备请求
                //当所有人都准备之后,继续开始发牌操作
                //又开始新一局的游戏
                roomInfo.RoomState = RoomState.Ending;//设置房间状态为结束状态
            }
        }
        #endregion

        #region 根据指定的房间编号设置玩家的分数
        /// <summary>
        /// 根据房间编号设置房间内指定用户的分数
        /// </summary>
        /// <param name="roomId">设置分数的房间</param>
        /// <param name="userInfo">设置分数的玩家</param>
        /// <param name="cardData">按照那张卡牌来设置玩家分数</param>
        /// <param name="setUserScoreCode">设置玩家分数的代码</param>
        public void SetUserScoreByRoomId(int roomId, UserInfo userInfo, string cardData, PasseServiceUserScoreCode userScoreCode)
        {
            if (roomId < 0 || userInfo == null || string.IsNullOrEmpty(cardData))//做参数数据完整校验工作(防止程序抛出异常)
                return;
            RoomInfo tmpRoomInfo = this.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间信息数据
            int userScore = 0;//临时存储用户分数数据
            switch (userScoreCode)
            {
                case PasseServiceUserScoreCode.底牌://按照底牌的方式来设置玩家的分数
                    {
                        bool isGetUserInfoSuccess = this.roomUserTouchCardScoreDict[tmpRoomInfo].TryGetValue(userInfo, out userScore);
                        //如果获取成功,则开始计算分数
                        if (isGetUserInfoSuccess)
                        {
                            //循环遍历卡牌值得数组
                            foreach (string cardValue in this.cardValues)
                            {
                                if (cardData.Contains(cardValue))//如果传递过来的卡牌数据中包含卡牌值的话,就计算分数即可
                                {
                                    userScore += this.GetScoreByCardValue(cardValue);
                                    break;//这里使用break关键字的用意在于,只进行循环遍历一次,目的是节省性能.
                                }
                                else
                                    continue;
                            }
                            if (userScore != 0)//表示计算了一次分数,需要根据用户信息来重新给用户赋分数的值
                                this.roomUserTouchCardScoreDict[tmpRoomInfo][userInfo] = userScore;//为用户的分数值重新赋值
                        }
                    }
                    break;
                case PasseServiceUserScoreCode.明牌://按照明牌的方式来设置玩家的分数
                    {
                        bool isGetUserInfoSuccess = this.roomUserClearCardScoreDict[tmpRoomInfo].TryGetValue(userInfo, out userScore);
                        //如果获取成功,则开始计算分数
                        if (isGetUserInfoSuccess)
                        {
                            //循环遍历卡牌值得数组
                            foreach (string cardValue in this.cardValues)
                            {
                                if (cardData.Contains(cardValue))//如果传递过来的卡牌数据中包含卡牌值的话,就计算分数即可
                                {
                                    userScore += GetScoreByCardValue(cardValue);
                                    break;//这里使用break关键字的用意在于,只进行循环遍历一次,目的是节省性能.
                                }
                                else
                                    continue;
                            }
                            if (userScore != 0)//表示计算了一次分数,需要根据用户信息来重新给用户赋分数的值
                                this.roomUserClearCardScoreDict[tmpRoomInfo][userInfo] = userScore;//为用户的分数值重新赋值
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 根据卡牌值获取分数
        /// </summary>
        /// <param name="cardValue"></param>
        /// <returns></returns>
        private int GetScoreByCardValue(string cardValue)
        {
            int userScore = 0;
            switch (cardValue)//判断卡牌的值
            {
                case "Nine":// +9分
                    userScore = 9; break;
                case "Ten":// +10分
                    userScore = 10; break;
                case "Jack":// +11分
                    userScore = 11; break;
                case "Queen":// +12分
                    userScore = 12; break;
                case "King":// +13分
                    userScore = 13; break;
                case "One":// +15分
                    userScore = 15; break;
                default:
                    userScore = 0; break;
            }
            return userScore;
        }
        #endregion

        #endregion

        #region 给指定的客户端对象分发明牌[这儿当玩家发起跟牌请求的时候会进行调用]
        /// <summary>
        /// 给指定的客户端发明牌
        /// </summary>
        /// <param name="peer">分发明牌的指定客户端对象</param>
        /// <param name="roomId">该玩家所在的房间的房间编号</param>
        public void GetClearCardToClient(ClientPeer peer, int roomId)
        {
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);//根据指定房间编号获取房间数据

            #region 这儿用来校验当前发起跟牌请求的时候玩家是否已经处于不跟牌数状态
            foreach (KeyValuePair<RoomInfo, Dictionary<ClientPeer, bool>> roomItem in this.roomFollowcardUserDict)//循环遍历存储房间数据 和 房间内弃牌的玩家对象 数据字典
            {
                foreach (KeyValuePair<ClientPeer, bool> clientItem in roomItem.Value)//循环遍历已经不跟牌的玩家对象列表
                {
                    if (clientItem.Key == peer && clientItem.Value == true)//如果在遍历的过程中 找到了引用相同的玩家对象(这里如果相同,表示玩家已经弃牌,不需要继续处理给这个玩家发牌了)
                        return;//直接return
                }
            }
            #endregion

            #region 这儿用来校验当前发起跟牌请求的时候玩家是否已经处于弃牌状态
            foreach (KeyValuePair<RoomInfo, List<ClientPeer>> item in this.roomDiscardDict)//循环遍历存储房间数据 和 房间内弃牌的玩家对象 数据字典
            {
                for (int clientIndex = 0; clientIndex < item.Value.Count; clientIndex++)//循环遍历已经弃牌的玩家对象列表
                {
                    if (item.Value[clientIndex] == peer)//如果在遍历的过程中 找到了引用相同的玩家对象(这里如果相同,表示玩家已经弃牌,不需要继续处理给这个玩家发牌了)
                        return;//直接return
                }
            }
            #endregion

            for (int userIndex = 0; userIndex < roomInfo.UserInfos.Count; userIndex++)//循环遍历房间内的玩家列表 直到找到要发明牌的客户端玩家对象为止
            {
                UserInfo tmpUserInfo = roomInfo.UserInfos[userIndex];
                if (tmpUserInfo.ClientUserSocket == peer.ClientSocket)//如果找到了
                {
                    string tmpCard = this.GetRandomCard();//获取一张随机牌
                    LogMessage.Instance.SetLogMessage(peer.ClientSocket.RemoteEndPoint.ToString() + "得到的明牌：" + tmpCard);
                    //构建网络消息给他发过去
                    peer.OnSendMessage
                        (
                            new SocketMessage()
                            {
                                OperationCode = OperationCode.Service,
                                SubOperationCode = (int)ServiceCode.Passe_Response,
                                DataValue = tmpCard + "," + tmpUserInfo.ClientIndex
                            }
                        );
                    break;//这儿使用break的原因是因为只需要给一个客户端对象发明牌就可以了,发完之后,需要跳出循环,完成一次发明牌的操作。
                }
            }
            //判断存不存在这个房间信息
            if (!this.roomPlayCardIndexDict.ContainsKey(roomInfo))//不存在添加
                this.roomPlayCardIndexDict.Add(roomInfo, 1);//设置初始值
            else//存在
                this.roomPlayCardIndexDict[roomInfo]++;//重新累加以后并进行赋值
        }
        #endregion

        #region 从玩家分数数据字典中移除玩家的数据
        /// <summary>
        /// 移除用户分数数据字典中的用户数据
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        public void RemoveUserScoreofUserScoreDict(ClientPeer clientPeer, int roomId)
        {
            RoomInfo tmpRoomInfo = this.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间信息数据
            List<UserInfo> userInfos = tmpRoomInfo.UserInfos;
            foreach (UserInfo user in userInfos)
            {
                if (user.ClientUserSocket == clientPeer.ClientSocket)
                {
                    this.roomUserClearCardScoreDict[tmpRoomInfo].Remove(user);
                    break;
                }
            }
        }

        #endregion

    }
}
