using Dlzyff.BoardGame.BottomServer.Applications;
using Dlzyff.BoardGame.BottomServer.Pools;
using Dlzyff.BoardGame.BottomServer.Tools;
using Dlzyff.BoardGame.Protocol.Codes;
using Dlzyff.BoardGameServer.Log;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace Dlzyff.BoardGame.BottomServer.Peers
{
    /// <summary>
    /// 服务端底层核心类(封装服务端的Socket连接对象)
    /// </summary>
    public class ServerPeer
    {
        /// <summary>
        /// 服务端连接对象
        /// </summary>
        private Socket serverSocket;

        /// <summary>
        /// 服务端连接信号量(限制客户端对象的连接个数)
        /// </summary>
        private Semaphore acceptSemaphore;

        /// <summary>
        /// 客户端对象连接池
        /// </summary>
        private ClientPeerPool clientPeerPool;

        /// <summary>
        /// 应用层
        /// </summary>
        private IApplicationBase application;

        /// <summary>
        /// 设置应用层
        /// </summary>
        /// <param name="application"></param>
        public void SetApplication(IApplicationBase application)
        {
            this.application = application;
        }

        #region 启动服务端

        /// <summary>
        /// 开启服务端
        /// </summary>
        /// <param name="portNumber">服务端端口号</param>
        /// <param name="maxConnectCount">服务端支持的最大连接数</param>
        public void StartServer(int portNumber, int maxConnectCount)
        {
            try
            {
                this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.acceptSemaphore = new Semaphore(maxConnectCount, maxConnectCount);

                #region 构建客户端对象连接池
                this.clientPeerPool = new ClientPeerPool(maxConnectCount);//构建客户端连接池对象
                ClientPeer tmpClientPeer = null;

                for (int clientPeerIndex = 0; clientPeerIndex < maxConnectCount; clientPeerIndex++)//遍历客户端最大连接数
                {
                    tmpClientPeer = new ClientPeer();//构建客户端对象
                    tmpClientPeer.ReceiveeEventArgs.Completed += this.OnReceiveMessageEventCompleted;//注册接收消息完成时的事件函数
                    tmpClientPeer.ReceiveMessageComplete += this.ReceiveMessageComplete;//注册客户端对象接收消息完成时的事件函数
                    tmpClientPeer.SendMessageErrorToDisconnect += this.Disconnect;//注册客户端对象发送消息时出错的事件函数
                    this.clientPeerPool.Enqueue(tmpClientPeer);//将构建好的客户端对象存放到客户端对象连接池中
                }
                #endregion

                this.serverSocket.Bind(new IPEndPoint(IPAddress.Any, portNumber));
                this.serverSocket.Listen(10);
                this.StartAccept(null);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
            }
        }

        public void ResetServer(int portNumber)
        {
            try
            {
                this.serverSocket.Bind(new IPEndPoint(IPAddress.Any, portNumber));
                this.serverSocket.Listen(10);
                this.StartAccept(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion

        #region 接收连接请求

        /// <summary>
        /// 开始等待客户端对象异步连接请求的建立
        /// </summary>
        /// <param name="eventArgs"></param>
        private void StartAccept(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                eventArgs = new SocketAsyncEventArgs();
                eventArgs.Completed += this.ClientAcceptEventCompleted;//注册建立连接完成时的事件函数
            }
            //if (this.serverSocket == null)
            //    return;
            bool result = this.serverSocket.AcceptAsync(eventArgs);//开启异步连接
            if (result == false)
                this.ProcessAccept(eventArgs);
        }

        /// <summary>
        /// 处理客户端对象建立异步连接的事件回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void ClientAcceptEventCompleted(object sender, SocketAsyncEventArgs eventArgs)
        {
            this.ProcessAccept(eventArgs);
        }

        /// <summary>
        /// 处理客户端对象建立异步请求
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ProcessAccept(SocketAsyncEventArgs eventArgs)
        {
            this.acceptSemaphore.WaitOne();//增加信号量,表示有新的客户端连接到服务端了
            ClientPeer tmpClientPeer = this.clientPeerPool.Dequeue();//从客户端对象连接池中取出一个连接对象供连接到服务端的客户端对象使用
            tmpClientPeer.ClientSocket = eventArgs.AcceptSocket;//取得连接对象
            tmpClientPeer.OnSendMessage(OperationCode.User, 0, "你好,玩家~");
            //Console.WriteLine("有客户端对象连接到服务端了~" + tmpClientPeer.ClientSocket.RemoteEndPoint.ToString());
            if (tmpClientPeer.ClientSocket.RemoteEndPoint != null)
                LogMessage.Instance.SetLogMessage(string.Format("玩家 [ {0} ] 连接到服务端了~", tmpClientPeer.ClientSocket.RemoteEndPoint.ToString()));
            this.OnStartReceiveMessage(tmpClientPeer);//开始接收连接到服务端的客户端对象发送给服务端的消息体数据
            eventArgs.AcceptSocket = null;
            this.StartAccept(eventArgs);//继续等待其他客户端对象向服务端发起建立连接的请求
        }
        #endregion

        #region 接收数据
        /// <summary>
        /// 根据指定的客户端对象开始接收消息体数据
        /// </summary>
        /// <param name="clientPeer">要接收消息的指定客户端对象</param>
        private void OnStartReceiveMessage(ClientPeer clientPeer)
        {
            try
            {
                bool result = clientPeer.ClientSocket.ReceiveAsync(clientPeer.ReceiveeEventArgs);//通过异步的方式接收客户端对象发送过来的消息体数据
                if (result == false)//如果异步的返回结果为false 表示消息体没有接收完成 
                    this.OnProcessReceiveMessage(clientPeer.ReceiveeEventArgs);
            }
            catch
            {
                //Console.WriteLine(exception.Message);
                //throw exception;
            }
        }

        /// <summary>
        /// 处理异步接收消息完成时的回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnReceiveMessageEventCompleted(object sender, SocketAsyncEventArgs eventArgs)
        {
            this.OnProcessReceiveMessage(eventArgs);
        }

        /// <summary>
        /// 处理接收到的消息体数据
        /// </summary>
        /// <param name="eventArgs"></param>
        private void OnProcessReceiveMessage(SocketAsyncEventArgs eventArgs)
        {
            ClientPeer tmpClientPeer = eventArgs.UserToken as ClientPeer;//获取客户端应用程序对象
            SocketAsyncEventArgs tmpReceiveEventArgs = tmpClientPeer.ReceiveeEventArgs;//获取用于接收消息的事件参数对象
            //校验获取到的客户端对象是否为空&&是否成功接收到了消息&&接收到的消息体字节数据长度是否大于0
            //如果校验成功
            if (tmpClientPeer != null && tmpReceiveEventArgs.SocketError == SocketError.Success && tmpReceiveEventArgs.BytesTransferred > 0)
            {
                //根据接收到消息体字节数据长度构建一个用于存储接收到消息的字节数组用于存储接收到的消息体数据
                byte[] receiveMessageData = new byte[tmpReceiveEventArgs.BytesTransferred];
                Buffer.BlockCopy(tmpReceiveEventArgs.Buffer, 0, receiveMessageData, 0, tmpReceiveEventArgs.BytesTransferred);//通过拷贝的方式向存储数据的字节数组中复制数据
                tmpClientPeer.OnStartReceiveMessage(receiveMessageData);//把存储数据的字节数组交由客户端对象自己进行数据处理
                //使用尾递归的方式,循环接收客户端对象发送过来的消息体数据
                this.OnStartReceiveMessage(tmpClientPeer);
            }
            else if (tmpReceiveEventArgs.BytesTransferred == 0)//处理断开连接(如果接收到的字节数为0的情况下相当于没有接收到任何客户端发送过来的消息体数据长度)
            {
                if (tmpReceiveEventArgs.SocketError == SocketError.Success)//客户端主动断开连接
                {
                    //处理客户端主动断开连接的行为
                    this.Disconnect(tmpClientPeer, "客户端对象主动断开连接~");
                }
                else//客户端由于网络异常被动断开连接
                    //处理客户端由于网络异常被动断开连接的行为
                    this.Disconnect(tmpClientPeer, tmpReceiveEventArgs.SocketError.ToString());
            }
        }

        /// <summary>
        /// 客户端对象接收消息完成时的事件回调函数
        /// </summary>
        /// <param name="clientPeer">具体的客户端对象</param>
        /// <param name="socketMessage">接收消息后解析出来的数据值</param>
        private void ReceiveMessageComplete(ClientPeer clientPeer, SocketMessage socketMessage)
        {
            try
            {
                if (clientPeer != null && socketMessage != null)
                    //接收完消息,返回给对应的应用层
                    this.application.OnReceiveMessage(clientPeer, socketMessage);
                else
                {
                    if (clientPeer == null)
                        Console.WriteLine("客户端连接对象为空");
                    else if (socketMessage == null)
                        Console.WriteLine("网络消息为空");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
            }
        }

        #endregion

        #region 断开连接
        /// <summary>
        /// 处理客户端断开连接
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="reason"></param>
        private void Disconnect(ClientPeer clientPeer, string reason)
        {
            try
            {
                if (clientPeer == null && clientPeer.ClientSocket == null)
                    return;
            }
            catch
            {
            }
            finally
            {
                if (clientPeer != null && clientPeer.ClientSocket != null)
                {
                    // Console.WriteLine("有客户端对象断开连接了：" + clientPeer.ClientSocket.RemoteEndPoint.ToString());
                    LogMessage.Instance.SetLogMessage(string.Format("玩家 [ {0} ] 与服务端断开连接了~", clientPeer.ClientSocket.RemoteEndPoint.ToString()));
                    this.application.OnDisconnect(clientPeer);//通知应用层有客户端连接对象断开连接了
                    clientPeer.OnDisconnect();//关闭客户端连接对象
                    this.clientPeerPool.Enqueue(clientPeer);//复用已经关闭的客户端连接对象
                    this.acceptSemaphore.Release();//将信号量-1(增加可连接数)
                }
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            this.application = null;
            if (this.serverSocket != null && this.serverSocket.Connected == true)
            {
                this.serverSocket.Shutdown(SocketShutdown.Both);
                this.serverSocket.Close();
                this.serverSocket.Disconnect(true);
                this.acceptSemaphore.Dispose();
            }
        }
        #endregion
    }
}
