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
        /// 存储卡牌花色
        /// </summary>
        protected string[] cardColors = null;

        /// <summary>
        /// 存储卡牌的值
        /// </summary>
        protected string[] cardValues = null;

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

        /// <summary>
        /// 重置卡牌牌数据
        /// </summary>
        public virtual void ResetCards()
        {
            //生成所有卡牌
            for (int cardColorIndex = 0; cardColorIndex < this.cardColors.Length; cardColorIndex++)
            {
                for (int cardValueIndex = 0; cardValueIndex < this.cardValues.Length; cardValueIndex++)
                {
                    string tmpCardValue = this.cardColors[cardColorIndex] + this.cardValues[cardValueIndex];
                    this.allCards.Add(tmpCardValue);
                }
            }
            //打乱扑克牌
            int cardCount = this.cardColors.Length * this.cardValues.Length;//取得卡牌的总个数
            for (int cardIndex = 0; cardIndex < cardCount; cardIndex++)//根据卡牌总个数进行循环遍历处理
            {
                int ranIndex = this.ranCardIndex.Next(0, this.allCards.Count);//从所有卡牌中随机取出一个卡牌的索引值
                this.resCards.Add(this.allCards[ranIndex]);//根据取出的索引值获取对应的数据 添加到存放打乱卡牌的集合中
                this.allCards.RemoveAt(ranIndex);//取出添加完毕 将数据删除
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
        public virtual void AddRoomInfo(RoomInfo roomInfo)
        {

        }

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
