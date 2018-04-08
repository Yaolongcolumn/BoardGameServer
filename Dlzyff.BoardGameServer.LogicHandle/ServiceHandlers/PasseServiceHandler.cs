using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGame.BottomServer.Tools;
using Dlzyff.BoardGame.Protocol.Codes;
using Dlzyff.BoardGame.Protocol.Codes.GameCode;
using Dlzyff.BoardGameServer.Cache;
using Dlzyff.BoardGameServer.DataCache.Room;
using Dlzyff.BoardGameServer.DataCache.Services;
using Dlzyff.BoardGameServer.DataCache.Users;
using Dlzyff.BoardGameServer.Handles;
using Dlzyff.BoardGameServer.Model;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Dlzyff.BoardGameServer.LogicHandle.ServiceHandlers
{
    /// <summary>
    /// 帕斯业务处理
    /// </summary>
    public class PasseServiceHandler : IHandler
    {

        /// <summary>
        /// 用户数据缓存对象
        /// </summary>
        private UserCache userCache = Caches.UserCache;

        /// <summary>
        /// 房间数据缓存对象
        /// </summary>
        private RoomCache roomCache = Caches.RoomCache;

        /// <summary>
        /// 帕斯业务数据缓存对象
        /// </summary>
        private PasseServiceCache passeServiceCache = Caches.PasseServiceCache;
        public void OnDisconnect(ClientPeer clientPeer)
        {
            if (clientPeer != null)
                clientPeer.OnDisconnect();
        }

        /// <summary>
        /// 接收客户端发送过来的消息
        /// </summary>
        /// <param name="clientPeer">发送消息的客户端对象</param>
        /// <param name="subOperationCode">发送给指定模块的子操作码</param>
        /// <param name="dataValue">消息附带的数据</param>
        public void OnReceiveMessage(ClientPeer clientPeer, int subOperationCode, object dataValue)
        {
            PasseGameCode passeGameCode = (PasseGameCode)Enum.Parse(typeof(PasseGameCode), subOperationCode.ToString());
            switch (passeGameCode)
            {
                case PasseGameCode.Follow_Request://处理跟的业务请求
                    {
                        //判断客户端传递过来的房间编号值是否能够转换成功
                        string[] roomDataSplits = dataValue.ToString().Split(',');//解析客户端传递过来的下注时的数据
                        string strRoomId = roomDataSplits[0];//解析出来的房间编号
                        string strMoney = roomDataSplits[1];//解析出来的玩家的下的注数
                        if (int.TryParse(strRoomId, out int roomId))//如果转换成功了
                            if (int.TryParse(strMoney, out int money))
                                this.ProcessFollowRequest(clientPeer, roomId, money);//处理跟牌
                    }
                    break;
                case PasseGameCode.NoFollow_Request://处理不跟的业务请求
                    {
                        //不跟
                        //判断客户端传递过来的房间编号值是否能够转换成功
                        if (int.TryParse(dataValue.ToString(), out int roomId))//如果转换成功了
                            this.ProcessNoFollowRequest(clientPeer, roomId);//处理不跟牌
                    }
                    break;
                case PasseGameCode.Play_Request://处理踢的业务请求
                    {
                        //解析客户端发送过来的踢牌的数据
                        //解析格式：房间编号,筹码(钱)
                        string[] playDataSplit = dataValue.ToString().Split(',');
                        string roomIdStr = playDataSplit[0];//取出解析完毕的房间号
                        string moneyStr = playDataSplit[1];//取出解析完毕要下注的筹码值
                        //判断客户端传递过来的房间编号值是否能够转换成功
                        if (int.TryParse(roomIdStr, out int roomId))//如果转换成功了
                        {
                            if (int.TryParse(moneyStr, out int money))
                                this.ProcessPlayRequest(clientPeer, roomId, money);
                        }
                    }
                    break;
                case PasseGameCode.Discard_Request://处理弃牌的业务请求
                    {
                        //判断客户端传递过来的房间编号值是否能够转换成功
                        if (int.TryParse(dataValue.ToString(), out int roomId))//如果转换成功了
                            this.ProcessDiscardRequest(clientPeer, roomId);
                    }
                    break;
                case PasseGameCode.DispalyFollow_Request://处理显示跟的请求
                    SocketMessage message = new SocketMessage()
                    {
                        OperationCode = OperationCode.Service,
                        SubOperationCode = (int)ServiceCode.Passe_BroadcastResponse,
                        DataValue = "null"
                    };
                    clientPeer.OnSendMessage(message);//给下注的客户端对象发送消息(通知下注玩家,准备开始下注)
                    break;
                case PasseGameCode.BottomPour_Request://处理下注的业务请求
                    {
                        //判断客户端传递过来的下注的钱数值是否能够转换成功
                        //if (int.TryParse(dataValue.ToString(), out int money))//如果转换成功了
                        //    this.ProcessBottomPourRequest(clientPeer, money);//处理下注
                        string[] roomDataSplits = dataValue.ToString().Split(',');//解析客户端传递过来的下注时的数据
                        string strRoomId = roomDataSplits[0];//解析出来的房间编号
                        string strMoney = roomDataSplits[1];//解析出来的玩家的下的注数
                        if (int.TryParse(strRoomId, out int roomId))//判断房间编号是否可以转换成整数类型
                        {
                            if (int.TryParse(strMoney, out int money))//判断玩家下的注数是否可以转换成整数类型
                                this.ProcessBottomPourRequest(clientPeer, roomId, money);//都转换成功了，处理客户端发送过来的下注业务请求
                        }
                    }
                    break;
            }
        }

        #region 跟牌
        /// <summary>
        /// 处理跟牌的业务请求
        /// </summary>
        /// <param name="clientPeer"></param>
        private void ProcessFollowRequest(ClientPeer clientPeer, int roomId, int money)
        {
            //当客户端向服务端发起跟的业务请求的时候
            //服务端需要扣除客户端的赌注积分(默认情况下扣除1赌注积分)
            //这里从对应的用户数据缓存对象中减少玩家的钱数即可
            bool isSuccess = this.userCache.SubMoney(clientPeer, 100);//使用用户数据缓存对象减少玩家的钱数
            if (isSuccess)//如果减少钱数成功了
            {
                this.passeServiceCache.ChangeRoomFollowcardUserDictionaryByRoomId(roomId, clientPeer);
                UserInfo userInfo = this.roomCache.GetUserInfoByClientPeer(clientPeer);
                //构造要发送给下注客户端的一条网络消息
                //业务模块/帕斯响应码/下注响应码|下的注数,下注的玩家座位索引
                SocketMessage message = new SocketMessage()
                {
                    OperationCode = OperationCode.Service,
                    SubOperationCode = (int)ServiceCode.Passe_FollowResponse,
                    DataValue = money + "," + userInfo.ClientIndex.ToString()//发送的消息:玩家下注的钱数,下注的客户端座位号
                };
                Thread.Sleep(1000);
                //广播消息给每一个房间内的玩家
                this.roomCache.BroadcastMessageByRoomId(roomId, message);//通知给房间内的每一个客户端对象,告知他们有玩家下注了.
            }
            else//否则减少钱数失败了
            {
                //Todo:给客户端发送消息,告诉你钱不够了,不能进行下注了....
            }
        }
        #endregion

        #region 不跟牌
        /// <summary>
        /// 处理不跟牌的业务请求
        /// </summary>
        /// <param name="clientPeer">不跟牌的指定客户端对象</param>
        /// <param name="roomId">在哪个房间</param>
        private void ProcessNoFollowRequest(ClientPeer clientPeer, int roomId)
        {
            this.ProcessDiscardRequest(clientPeer, roomId);
        }
        #endregion

        #region 踢牌
        /// <summary>
        /// 处理踢牌的业务请求
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        private void ProcessPlayRequest(ClientPeer clientPeer, int roomId, int money)
        {
            //就减少要踢牌的客户端钱数
            this.userCache.SubMoney(clientPeer, money);
        }
        #endregion

        #region 不踢牌
        /// <summary>
        /// 处理不踢牌的业务请求
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        private void ProcessNoPlayRequest(ClientPeer clientPeer, int roomId)
        {
            //不踢牌直接当做弃牌处理
            this.ProcessDiscardRequest(clientPeer, roomId);
        }
        #endregion

        #region 弃牌
        /// <summary>
        ///  处理弃牌的业务请求
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        private void ProcessDiscardRequest(ClientPeer clientPeer, int roomId)
        {
            this.passeServiceCache.SetDiscardUserByRoomId(roomId, clientPeer);//根据房间编号设置弃牌玩家对象
        }
        #endregion

        #region 下注
        /// <summary>
        /// 处理下注的业务请求
        /// </summary>
        /// <param name="clientPeer">要进行下注的玩家</param>
        /// <param name="roomId">在哪个房间进行的下注</param>
        /// <param name="money">下了多少注</param>
        private void ProcessBottomPourRequest(ClientPeer clientPeer, int roomId, int money)
        {
            Console.WriteLine("接收到客户端发送过来的下注请求~");
            bool isSuccess = this.userCache.SubMoney(clientPeer, money);//使用用户数据缓存对象减少玩家的钱数
            if (isSuccess)//减少钱数成功了
            {
                RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);//根据指定房间编号获取房间信息数据
                //记录最后一个下注的玩家
                if (!this.passeServiceCache.roomLastBottompourUserDict.ContainsKey(roomInfo))
                    this.passeServiceCache.roomLastBottompourUserDict.Add(roomInfo, clientPeer);
                else
                    this.passeServiceCache.roomLastBottompourUserDict[roomInfo] = clientPeer;
                //Todo:给客户端发送消息
                //通知客户端减少的钱数
                UserInfo userInfo = this.roomCache.GetUserInfoByClientPeer(clientPeer);//根据客户端连接对象获取用户信息
                //构造要发送给下注客户端的一条网络消息
                //业务模块/帕斯响应码/下注响应码|下的注数,下注的玩家座位索引
                SocketMessage message = new SocketMessage()
                {
                    OperationCode = OperationCode.Service,
                    SubOperationCode = (int)ServiceCode.Passe_BottomPourResponse,
                    DataValue = money + "," + userInfo.ClientIndex.ToString()//发送的消息:下注的钱数,下注的客户端座位号
                };
                Thread.Sleep(1000);
                //广播消息给每一个房间内的玩家
                this.roomCache.BroadcastMessageByRoomId(roomId, message);//通知给房间内的每一个客户端对象,告知他们有玩家下注了.
                List<ClientPeer> clientPeers = this.roomCache.GetClientPeersByRoomId(roomId);//根据指定房间编号获取房间内的客户端连接对象列表
                int tmpIndex = (clientPeers.FindIndex(client => client == clientPeer) + 1);//找出当前玩家的下一个玩家 通知他该跟注了
                if (tmpIndex > clientPeers.Count - 1)
                    tmpIndex = clientPeers.Count - 1;
                ClientPeer nextClientPeer = clientPeers[tmpIndex];
                //判断当前玩家是否是最后一个玩家 如果是的话 需要给玩家列表中的第一个玩家发跟注释响应消息
                if (nextClientPeer == clientPeers[clientPeers.Count - 1] && nextClientPeer == clientPeer)
                    nextClientPeer = clientPeers[0];
                else//不做任何处理 直接正常发送即可(这里表示当前玩家不是玩家列表中最后一个玩家)
                {

                }
                Thread.Sleep(1000);
                //将消息更改至跟注释的一条网络消息
                //业务模块/帕斯响应码/跟牌响应码
                message.ChangeMessage(OperationCode.Service, (int)ServiceCode.Passe_Response, (int)PasseGameCode.Follow_Response);
                nextClientPeer.OnSendMessage(message);//给下一个玩家发送跟注消息,提示他需要进行跟注
            }
            else//减少钱数失败了
            {
                //Todo:给客户端发送消息,告诉你钱不够了,不能进行下注了....
            }
        }
        #endregion
    }
}
