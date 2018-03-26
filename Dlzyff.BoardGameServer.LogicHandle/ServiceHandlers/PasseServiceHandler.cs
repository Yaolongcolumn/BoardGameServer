using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGame.Protocol.Codes.GameCode;
using Dlzyff.BoardGameServer.Cache;
using Dlzyff.BoardGameServer.DataCache.Services;
using Dlzyff.BoardGameServer.DataCache.Users;
using Dlzyff.BoardGameServer.Handles;
using System;

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
        /// 帕斯业务数据缓存对象
        /// </summary>
        private PasseServiceCache passeServiceCache = Caches.PasseServiceCache;
        public void OnDisconnect(ClientPeer clientPeer)
        {

        }

        public void OnReceiveMessage(ClientPeer clientPeer, int subOperationCode, object dataValue)
        {
            PasseGameCode passeGameCode = (PasseGameCode)Enum.Parse(typeof(PasseGameCode), subOperationCode.ToString());
            switch (passeGameCode)
            {
                case PasseGameCode.Follow_Request://处理跟的业务请求
                    {
                        //判断客户端传递过来的房间编号值是否能够转换成功
                        if (int.TryParse(dataValue.ToString(), out int roomId))//如果转换成功了
                            this.ProcessFollowRequest(clientPeer, roomId);//处理跟牌
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
                        string roomIdStr = playDataSplit[0];
                        string moneyStr = playDataSplit[1];
                        //判断客户端传递过来的房间编号值是否能够转换成功
                        if (int.TryParse(roomIdStr, out int roomId))//如果转换成功了
                        {
                            if (int.TryParse(moneyStr,out int money))
                                this.ProcessPlayRequest(clientPeer, roomId,money);
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
                case PasseGameCode.BottomPour_Request://处理下注的业务请求
                    {
                        //判断客户端传递过来的下注的钱数值是否能够转换成功
                        if (int.TryParse(dataValue.ToString(), out int money))//如果转换成功了
                            this.ProcessBottomPourRequest(clientPeer, money);//处理下注
                    }
                    break;
            }
        }

        #region 跟牌
        /// <summary>
        /// 处理跟牌的业务请求
        /// </summary>
        /// <param name="clientPeer"></param>
        private void ProcessFollowRequest(ClientPeer clientPeer, int roomId)
        {
            //当客户端向服务端发起跟的业务请求的时候
            //服务端需要扣除客户端的赌注积分(默认情况下扣除1赌注积分)
            //这里从对应的用户数据缓存对象中减少玩家的钱数即可
            //this.passeServiceCache.SubPourByClient(clientPeer);
            this.userCache.SubMoney(clientPeer, 100);
            //服务端需要给客户端发送一张明牌的数据
            this.passeServiceCache.GetClearCardToClient(clientPeer, roomId);
            //计算分数
            //   this.passeServiceCache.CompleteUserScoreByRoomId(roomId);
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
        /// <param name="clientPeer">下注的客户端对象</param>
        /// <param name="money">赌注筹码</param>
        private void ProcessBottomPourRequest(ClientPeer clientPeer, int money)
        {
            this.userCache.SubMoney(clientPeer, money);
        }
        #endregion
    }
}
