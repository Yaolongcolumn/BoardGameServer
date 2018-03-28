using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGameServer.Handles;
using System;

namespace Dlzyff.BoardGameServer.LogicHandle.ServiceHandlers
{
    /// <summary>
    /// 五轰六炸业务处理
    /// </summary>
    public class FivebombsWithSixbombsServiceHandler : IHandler
    {
        public void OnDisconnect(ClientPeer clientPeer)
        {
            if (clientPeer != null)
            {
                clientPeer.OnDisconnect();
            }
        }

        public void OnReceiveMessage(ClientPeer clientPeer, int subOperationCode, object dataValue)
        {
            throw new NotImplementedException();
        }
    }
}
