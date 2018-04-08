using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGame.Protocol.Codes.GameCode;
using Dlzyff.BoardGameServer.Cache;
using Dlzyff.BoardGameServer.DataCache.Services;
using Dlzyff.BoardGameServer.Handles;
using System;

namespace Dlzyff.BoardGameServer.LogicHandle.ServiceHandlers
{
    /// <summary>
    /// 五轰六炸业务处理
    /// </summary>
    public class FivebombsWithSixbombsServiceHandler : IHandler
    {
        private FivebombsWithSixbombsServiceCache fivebombsWithSixbombsServiceCache = Caches.FivebombsWithSixbombsServiceCache;

        public void OnDisconnect(ClientPeer clientPeer)
        {
            if (clientPeer != null)
            {
                clientPeer.OnDisconnect();
            }
        }

        public void OnReceiveMessage(ClientPeer clientPeer, int subOperationCode, object dataValue)
        {
            FivebombsWithSixbombsGameCode fivebombsWithSixbombsGameCode = (FivebombsWithSixbombsGameCode)Enum.Parse(typeof(FivebombsWithSixbombsGameCode), subOperationCode.ToString());
            switch (fivebombsWithSixbombsGameCode)
            {
                case FivebombsWithSixbombsGameCode.TouchCard_Request://处理摸牌的业务请求
                    {
                        if (int.TryParse(dataValue.ToString(), out int roomId))
                            this.ProcessTouchCardRequest(clientPeer, roomId);
                    }
                    break;
                case FivebombsWithSixbombsGameCode.PlayCard_Request://处理出牌的业务请求
                    {
                        if (int.TryParse(dataValue.ToString(), out int roomId))
                            this.ProcessPlayCardRequest(clientPeer, roomId);
                    }
                    break;
            }
        }

        /// <summary>
        /// 处理摸牌的业务请求
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        private void ProcessTouchCardRequest(ClientPeer clientPeer, int roomId)
        {

        }

        /// <summary>
        /// 处理出牌的业务请求
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        private void ProcessPlayCardRequest(ClientPeer clientPeer, int roomId)
        {

        }
    }
}
