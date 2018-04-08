using Dlzyff.BoardGame.BottomServer.Peers;
using System.Collections.Generic;

namespace Dlzyff.BoardGame.BottomServer.Pools
{
    /// <summary>
    /// 客户端对象连接池(重用客户端连接对象)
    /// </summary>
    public class ClientPeerPool
    {
        /// <summary>
        /// 客户端连接池队列
        /// </summary>
        private Queue<ClientPeer> clientPeers = null;
        /// <summary>
        /// 用于构造客户端连接池
        /// </summary>
        /// <param name="capacity">连接池容量</param>
        public ClientPeerPool(int capacity)
        {
            this.clientPeers = new Queue<ClientPeer>(capacity);
        }
        /// <summary>
        /// 向客户端连接池队列尾部添加一个客户端连接对象
        /// </summary>
        public void Enqueue(ClientPeer clientPeer)
        {
            this.clientPeers.Enqueue(clientPeer);
        }
        /// <summary>
        /// 从客户端连接池队列尾部移除一个客户端连接对象
        /// </summary>
        public ClientPeer Dequeue()
        {
            return this.clientPeers.Dequeue();
        }
    }
}
