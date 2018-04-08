using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGame.BottomServer.Tools;

namespace Dlzyff.BoardGame.BottomServer.Applications
{
    public interface IApplicationBase
    {
        /// <summary>
        /// 接收消息
        /// </summary>
        void OnReceiveMessage(ClientPeer clientPeer, SocketMessage socketMessage);
        /// <summary>
        /// 关闭连接
        /// </summary>
        void OnDisconnect(ClientPeer clientPeer);
    }
}
