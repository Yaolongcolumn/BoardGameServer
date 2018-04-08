using Dlzyff.BoardGame.BottomServer.Tools;
using Dlzyff.BoardGame.Protocol.Codes;
using Dlzyff.BoardGameServer.Log;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Dlzyff.BoardGame.BottomServer.Peers
{
    /// <summary>
    /// 接收消息完成的自定义委托类型
    /// </summary>
    /// <param name="clientPeer">具体的客户端对象</param>
    /// <param name="value">接收消息后取到的数据值</param>
    public delegate void ReceiveMessageCompleteDelegate(ClientPeer clientPeer, SocketMessage socketMessage);

    /// <summary>
    /// 发送消息出错时进行关闭连接的自定义委托类型
    /// </summary>
    /// <param name="clientPeer"></param>
    /// <param name="reason"></param>
    public delegate void SendMessageErrorToDisconnectDelegate(ClientPeer clientPeer, string reason);

    /// <summary>
    /// 客户端的核心类(封装客户端的Socket连接对象)
    /// </summary>
    public class ClientPeer
    {
        #region 客户端连接对象的特征

        private Socket _clientSocket;
        public Socket ClientSocket
        {
            get { return this._clientSocket; }
            set { this._clientSocket = value; }
        }

        #endregion


        public ClientPeer()
        {
            this.ReceiveeEventArgs = new SocketAsyncEventArgs()
            {
                UserToken = this//指定客户端接收消息的事件参数对象的用户为当前客户端对象
            };
            this.ReceiveeEventArgs.SetBuffer(new byte[1024], 0, 1024);
            this.SendEventArgs = new SocketAsyncEventArgs()
            {
                UserToken = this
            };
            this.SendEventArgs.Completed += this.SendMessageComplete;
        }

        #region 接收消息

        /// <summary>
        /// 客户端对象接收消息完成时用于返回给上层的事件
        /// </summary>
        public event ReceiveMessageCompleteDelegate ReceiveMessageComplete;

        /// <summary>
        /// 存储消息体数据的缓冲区
        /// </summary>
        private List<byte> dataValueBytesCache = new List<byte>();

        /// <summary>
        /// 客户端接收异步消息的事件参数对象
        /// </summary>
        public SocketAsyncEventArgs ReceiveeEventArgs;

        /// <summary>
        /// 是否处于处理接收到的消息
        /// </summary>
        private bool IsProcessReceiveMessage { get; set; }

        #region 网络传输中的粘包拆包问题原理
        //网络传输中的粘包拆包问题
        /*
         解决策略：打包消息头和消息尾
         例如发送的消息体数据->12345

        byte[] sendMsgData=Encoding.Default.GetBytes("12345")

        如何重新构造打包消息头+消息尾部?
        消息头指的就是要发送的消息字节数据的长度: sendMsgData.Length
        消息尾指的就是要发送的消息具体内容: sendMsgData
        获取数据长度-> sendMsgData.Length
        通过数据长度取得一个新的字节数组-> byte[] newBytes=BitConvert.ToBytes(sendMsgData.Length)
        打包之后的消息就是 sendMsgData+newBytes;

        如何解析读取打包好的消息体？
        首先将前边的4个字节转换成int类型(因为在打包的时候前边的4个字节是打包的消息数据长度,也就是对应的int类型)
        根据转换好的int类型,从第五个字节往后读取这个int类型的值(就是数据的长度)的数据即可
         */
        #endregion

        /// <summary>
        /// 开始接收消息体数据
        /// </summary>
        /// <param name="dataValueBytes">接收到的消息体数据</param>
        public void OnStartReceiveMessage(byte[] dataValueBytes)
        {
            this.dataValueBytesCache.AddRange(dataValueBytes);
            if (!this.IsProcessReceiveMessage)
                this.OnProcessReceiveMessage();
        }

        /// <summary>
        /// 处理接收到的消息体数据
        /// </summary>
        private void OnProcessReceiveMessage()
        {
            try
            {
                this.IsProcessReceiveMessage = true;
                byte[] dataValueBytes = EncodeHelper.DecodeMessage(ref this.dataValueBytesCache);
                if (dataValueBytes == null)
                    this.IsProcessReceiveMessage = false;
                else
                {
                    //根据接收到的消息体数据转成一个具体的对应类型,然后才能对接收到的消息体数据做处理
                    SocketMessage tmpSocketMessage = EncodeHelper.DecodeMessage(dataValueBytes);
                    LogMessage.Instance.SetLogMessage(
                        string.Format("接收到客户端对象 [ {0} ] 发送过来的消息 [ {1} ]~", this.ClientSocket.RemoteEndPoint.ToString(), tmpSocketMessage.DataValue.ToString()));
                    //处理完数据需要将处理好的数据回调给上层(通过委托的方式实现)
                    this.ReceiveMessageComplete?.Invoke(this, tmpSocketMessage);
                    //回调之后需要使用尾递归
                    this.OnProcessReceiveMessage();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
            }
        }

        #endregion

        #region 发送消息

        /// <summary>
        /// 客户端对象发送消息时发生错误关闭连接时用于返回给上层的事件
        /// </summary>
        public event SendMessageErrorToDisconnectDelegate SendMessageErrorToDisconnect;
        /// <summary>
        /// 客户端发送异步消息的事件参数对象
        /// </summary>
        private SocketAsyncEventArgs SendEventArgs;
        /// <summary>
        /// 标识是否处于发送消息的状态下
        /// </summary>
        private bool IsProcessSendMessage { get; set; }
        /// <summary>
        /// 存储要发送的消息体数据字节队列
        /// </summary>
        private Queue<byte[]> sendMessageDataBytes = new Queue<byte[]>();
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="operationCode">操作码</param>
        /// <param name="subOPerationCode">子操作码</param>
        /// <param name="dataValue">数据值</param>
        public void OnSendMessage(OperationCode operationCode, int subOPerationCode, object dataValue)
        {
            SocketMessage socketMessage = new SocketMessage(operationCode, subOPerationCode, dataValue);
            this.OnSendMessage(socketMessage);
        }
        /// <summary>
        /// 通过网络消息体发送消息
        /// </summary>
        /// <param name="message"></param>
        public void OnSendMessage(SocketMessage message)
        {
            byte[] dataValueBytes = EncodeHelper.EncodeMessage(message);
            byte[] sendMessageDataBytes = EncodeHelper.EncodeMessage(dataValueBytes);
            this.SendMessage(sendMessageDataBytes);
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="messageDataBytes"></param>
        public void SendMessage(byte[] messageDataBytes)
        {
            this.sendMessageDataBytes.Enqueue(messageDataBytes);
            if (!this.IsProcessSendMessage)
                this.SendMessage();
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        private void SendMessage()
        {
            try
            {
                this.IsProcessSendMessage = true;
                if (this.sendMessageDataBytes.Count == 0)
                {
                    this.IsProcessSendMessage = false;
                    return;
                }
                byte[] dataValueBytes = this.sendMessageDataBytes.Dequeue();
                // this.clientSocket.Send(dataValueBytes, dataValueBytes.Length, SocketFlags.None);
                this.SendEventArgs.SetBuffer(dataValueBytes, 0, dataValueBytes.Length);
                bool result = this.ClientSocket.SendAsync(this.SendEventArgs);
                if (result == false)
                    this.OnProcessMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 处理发送消息
        /// </summary>
        private void OnProcessMessage()
        {
            if (this.SendEventArgs.SocketError != SocketError.Success)
            {
                //Todo:
                //表示发送出错了
                if (this.SendMessageErrorToDisconnect != null)
                    this.SendMessageErrorToDisconnect(this, this.SendEventArgs.SocketError.ToString());
            }
            else
            {
                this.SendMessage();
            }
        }
        /// <summary>
        /// 异步消息发送完成时回调的事件函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendMessageComplete(object sender, SocketAsyncEventArgs e)
        {
            this.OnProcessMessage();
        }

        #endregion

        #region 断开连接

        /// <summary>
        /// 断开连接    
        /// </summary>
        public void OnDisconnect()
        {
            //需要清空接收到的消息数据
            this.dataValueBytesCache.Clear();
            this.IsProcessReceiveMessage = false;
            //需要清空要发送的消息数据
            sendMessageDataBytes.Clear();
            IsProcessSendMessage = false;
            if (this.ClientSocket != null)
            {
                //关闭客户端连接对象
                this.ClientSocket.Shutdown(SocketShutdown.Both);
                this.ClientSocket.Close();
                this.ClientSocket = null;
            }
        }

        #endregion
    }
}
