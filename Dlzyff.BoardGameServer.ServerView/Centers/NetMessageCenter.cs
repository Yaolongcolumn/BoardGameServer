using Dlzyff.BoardGame.BottomServer.Applications;
using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGame.BottomServer.Tools;
using Dlzyff.BoardGame.Protocol.Codes;
using Dlzyff.BoardGameServer.Handles;
using Dlzyff.BoardGameServer.LogicHandle;

namespace Dlzyff.BoardGameServer.ServerView.Centers
{
    /// <summary>
    /// 网络消息转发中心
    /// </summary>
    public class NetMessageCenter : IApplicationBase
    {
        /// <summary>
        /// 房间逻辑处理对象
        /// </summary>
        private IHandler roomHandler = new RoomHandler();

        /// <summary>
        /// 
        /// </summary>
        private IHandler serviceHandler = new ServiceHandler();
        public void OnDisconnect(ClientPeer clientPeer)
        {
            this.roomHandler.OnDisconnect(clientPeer);
        }

        /// <summary>
        /// 处理接收客户端发送过来的消息
        /// </summary>
        /// <param name="clientPeer">发送消息的客户端对象</param>
        /// <param name="socketMessage">发送的消息</param>
        public void OnReceiveMessage(ClientPeer clientPeer, SocketMessage socketMessage)
        {
            switch (socketMessage.OperationCode)
            {
                case OperationCode.Account://处理账户模块
                    //Console.WriteLine(socketMessage.OperationCode.ToString() + " " + socketMessage.SubOperationCode.ToString() + " " + socketMessage.DataValue.ToString());
                    //         this.accountHandle.OnReceiveMessage(clientPeer, socketMessage.SubOperationCode, socketMessage.DataValue);
                    break;
                case OperationCode.User://处理用户模块
                    //      this.userHandle.OnReceiveMessage(clientPeer, socketMessage.SubOperationCode, socketMessage.DataValue);
                    break;
                case OperationCode.Room://处理房间模块
                    this.roomHandler.OnReceiveMessage(clientPeer, socketMessage.SubOperationCode, socketMessage.DataValue);
                    break;
                case OperationCode.Service://处理业务模块
                    this.serviceHandler.OnReceiveMessage(clientPeer, socketMessage.SubOperationCode, socketMessage.DataValue);
                    break;
            }
        }
    }
}
