using Dlzyff.BoardGame.BottomServer.Peers;
namespace Dlzyff.BoardGameServer.Handles
{
    public interface IHandler
    {
        /// <summary>
        /// 接收消息
        /// </summary>
        void OnReceiveMessage(ClientPeer clientPeer, int subOperationCode,object dataValue);
        /// <summary>
        /// 关闭连接
        /// </summary>
        void OnDisconnect(ClientPeer clientPeer);
    }
}
