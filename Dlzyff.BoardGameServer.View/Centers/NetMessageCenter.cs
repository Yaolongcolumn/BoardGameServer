using Dlzyff.BoardGame.BottomServer.Applications;
using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGame.BottomServer.Tools;
using Dlzyff.BoardGame.Protocol.Codes;
using Dlzyff.BoardGameServer.Handles;
using Dlzyff.BoardGameServer.LogicHandle;

namespace Dlzyff.BoardGameServer.View.Centers
{
    /// <summary>
    /// 网络消息中心
    /// </summary>
    public class NetMessageCenter : IApplicationBase
    {
        ///// <summary>
        ///// 账户逻辑处理对象
        ///// </summary>
        //private IHandler accountHandle = new AccountHandler();

        ///// <summary>
        ///// 用户逻辑处理对象
        ///// </summary>
        //private IHandler userHandle = new UserHandler();

        ///// <summary>
        ///// 匹配逻辑处理对象
        ///// </summary>
        //private IHandler matchHandle = new MatchHandler();

        /// <summary>
        /// 房间逻辑处理对象
        /// </summary>
        private IHandler roomHandler = new RoomHandler();



        public NetMessageCenter()
        {

        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="clientPeer"></param>
        public void OnDisconnect(ClientPeer clientPeer)
        {
            //this.accountHandle.OnDisconnect(clientPeer);
            //this.userHandle.OnDisconnect(clientPeer);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="socketMessage"></param>
        public void OnReceiveMessage(ClientPeer clientPeer, SocketMessage socketMessage)
        {
            switch (socketMessage.OperationCode)//根据OperationCode判断客户端对象向服务端发出的是什么请求
            {
                case OperationCode.Account:
                    //Console.WriteLine(socketMessage.OperationCode.ToString() + " " + socketMessage.SubOperationCode.ToString() + " " + socketMessage.DataValue.ToString());
                    //         this.accountHandle.OnReceiveMessage(clientPeer, socketMessage.SubOperationCode, socketMessage.DataValue);
                    break;
                case OperationCode.User:
                    //      this.userHandle.OnReceiveMessage(clientPeer, socketMessage.SubOperationCode, socketMessage.DataValue);
                    break;
                case OperationCode.Room:
                    this.roomHandler.OnReceiveMessage(clientPeer, socketMessage.SubOperationCode, socketMessage.DataValue);
                    break;
                case OperationCode.Service:
                    break;
                    //case OperationCode.Match:
                    //    this.matchHandle.OnReceiveMessage(clientPeer, socketMessage.SubOperationCode, socketMessage.DataValue);
                    //    break;
            }
        }

    }
}
