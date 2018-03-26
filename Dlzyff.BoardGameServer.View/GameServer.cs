using Dlzyff.BoardGame.BottomServer.Applications;
using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGameServer.View.Centers;
using System;

namespace Dlzyff.BoardGameServer.View
{
    public class GameServer
    {
        public void OnStartServer()
        {
            ServerPeer serverPeer = new ServerPeer();
            IApplicationBase app =new NetMessageCenter();
            serverPeer.SetApplication(app);
            serverPeer.StartServer(6666, 10);
            Console.WriteLine("开启服务器成功~~");
        }
    }
}
