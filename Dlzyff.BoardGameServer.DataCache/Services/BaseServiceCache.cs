using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGameServer.Cache;
using Dlzyff.BoardGameServer.DataCache.Room;
using Dlzyff.BoardGameServer.Log;
using Dlzyff.BoardGameServer.Model;
using System;
using System.Collections.Generic;

namespace Dlzyff.BoardGameServer.DataCache.Services
{
    public abstract class BaseServiceCache : IServiceCacheable
    {
        /// <summary>
        /// 存储所有卡牌
        /// </summary>
        protected List<string> allCards = new List<string>();

        /// <summary>
        /// 存储打乱顺序后的卡牌
        /// </summary>
        protected List<string> resCards = new List<string>();

        /// <summary>
        /// 随机卡牌索引对象
        /// </summary>
        protected Random ranCardIndex = new Random();

        /// <summary>
        /// 用来存储客户端连接对象
        /// </summary>
        protected List<ClientPeer> clientPeers = new List<ClientPeer>();

        /// <summary>
        /// 记录一个房间的游戏局数的数据字典(一次游戏结束后,需要将数据清空)
        /// </summary>
        protected Dictionary<RoomInfo, int> roomRecordsNumberDict = new Dictionary<RoomInfo, int>();

        /// <summary>
        /// 房间数据缓存对象
        /// </summary>
        protected RoomCache roomCache = Caches.RoomCache;

        #region 初始化和重置卡牌数据
        /// <summary>
        /// 初始化卡牌数据数据
        /// </summary>
        public abstract void InitCardsData();

        public virtual void ResetCards() { }
        #endregion

        #region 显示所有卡牌
        /// <summary>
        /// 显示所有卡牌
        /// </summary>
        public void DisplayAllCards()
        {
            if (resCards.Count == 0)
                return;
            int count = 0;
            //Console.WriteLine(this.resCards.Count);
            for (int cardIndex = 0; cardIndex < this.resCards.Count; cardIndex++)
            {
                if (count == 5)
                {
                    Console.WriteLine();
                    count = 0;
                }
                Console.Write(this.resCards[cardIndex] + " ");
                count++;
            }
        }
        #endregion

        #region 增加和移除客户端连接对象
        /// <summary>
        /// 添加客户端连接对象
        /// </summary>
        /// <param name="clientPeer"></param>
        public void AddClientPeer(ClientPeer clientPeer)
        {
            if (!this.clientPeers.Contains(clientPeer))
                this.clientPeers.Add(clientPeer);
            LogMessage.Instance.SetLogMessage("当前客户端用户的个数：" + this.clientPeers.Count.ToString());
        }

        /// <summary>
        /// 移除客户端连接对象
        /// </summary>
        /// <param name="clientPeer"></param>
        public void SubClientPeer(ClientPeer clientPeer)
        {
            if (this.clientPeers.Contains(clientPeer))
                this.clientPeers.Remove(clientPeer);//从客户端连接对象列表中移除客户端连接对象
            LogMessage.Instance.SetLogMessage("当前客户端用户的个数：" + this.clientPeers.Count.ToString());
        }

        #endregion

        #region 获取一张随机卡牌
        public abstract string GetRandomCard();
        #endregion

        #region 添加、获取房间信息
        /// <summary>
        /// 添加房间信息对象
        /// </summary>
        /// <param name="roomInfo">要添加的房间信息</param>
        public abstract void AddRoomInfo(RoomInfo roomInfo);

        /// <summary>
        /// 根据房间编号获取房间信息数据
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        protected RoomInfo GetRoomInfoByRoomId(int roomId)
        {
            return this.roomCache.GetRoomInfoByRoomId(roomId);
        }
        #endregion
    }
}
