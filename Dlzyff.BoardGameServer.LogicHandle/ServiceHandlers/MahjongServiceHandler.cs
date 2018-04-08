using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGameServer.Handles;
using System;

namespace Dlzyff.BoardGameServer.LogicHandle.ServiceHandlers
{
    /// <summary>
    /// 麻将业务处理
    /// </summary>
    public class MahjongServiceHandler : IHandler
    {
        public void OnDisconnect(ClientPeer clientPeer)
        {
          
        }

        public void OnReceiveMessage(ClientPeer clientPeer, int subOperationCode, object dataValue)
        {
            
        }
    }
}
