using System;

namespace Dlzyff.BoardGameServer.View
{
    public class BoardGameServerView
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("************棋牌游戏服务器测试~~~");
            GameServer server = new GameServer();
            server.OnStartServer();

            #region 测试
            //do
            //{
            //    Console.WriteLine("请输入你要发送给客户端的消息体数据：");
            //    server.OnSendMessage(Console.ReadLine());
            //} while (true);

            //Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8088);
            //socket.Bind(point);
            //socket.Listen(0);
            //while (true)
            //{
            //    Socket client = socket.Accept();
            //    if (client.Connected)
            //    {
            //        Console.WriteLine("有客户端连接到服务端了~");
            //    }
            //    string sendMsgData = "这是从服务器发送过来的数据哟~~ ";
            //    byte[] sendByteData = Encoding.UTF8.GetBytes(sendMsgData);//将要发送的消息数据转成字节数组
            //    client.Send(sendByteData);//向客户端发送一条字节数组数据

            //    string receiveMsgData;//用于存储接收客户端发送过来的消息
            //    byte[] receiveByteData = new byte[1024 * 5];//构建用于接收客户端发送过来的消息缓冲区
            //    int receiveByteLength = client.Receive(receiveByteData);//接收客户端发送的消息数据 返回接收到的数据长度
            //    receiveMsgData = Encoding.UTF8.GetString(receiveByteData, 0, receiveByteLength);//根据客户端发送过来的消息数据长度进行消息编码解析
            //    Console.WriteLine("这是接收到客户端发送过来的一条数据哟-> {0}", receiveMsgData);
            //}
            //Todo：测试服务端向客户端打招呼
            // Console.ReadKey();  
            #endregion

            Console.ReadKey();

        }
    }
}
