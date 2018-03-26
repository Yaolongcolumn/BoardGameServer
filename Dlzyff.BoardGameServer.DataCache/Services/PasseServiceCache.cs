using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGame.BottomServer.Tools;
using Dlzyff.BoardGame.Protocol.Codes;
using Dlzyff.BoardGame.Protocol.Codes.GameCode;
using Dlzyff.BoardGameServer.Cache;
using Dlzyff.BoardGameServer.DataCache.Room;
using Dlzyff.BoardGameServer.Log;
using Dlzyff.BoardGameServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dlzyff.BoardGameServer.DataCache.Services
{
    /// <summary>
    /// 帕斯业务缓存类
    /// </summary>
    public class PasseServiceCache : IServiceCacheable
    {
        /// <summary>
        /// 存储卡牌花色
        /// </summary>
        private string[] cardColors = null;

        /// <summary>
        /// 存储卡牌的值
        /// </summary>
        private string[] cardValues = null;

        /// <summary>
        /// 存储所有卡牌
        /// </summary>
        private List<string> allCards = new List<string>();

        /// <summary>
        /// 存储打乱顺序后的卡牌
        /// </summary>
        private List<string> resCards = new List<string>();

        /// <summary>
        /// 随机卡牌索引对象
        /// </summary>
        private Random ranCardIndex = new Random();

        /// <summary>
        /// 客户端对象 对应的赌注数据字典
        /// </summary>
        private Dictionary<ClientPeer, int> clientPourDict = new Dictionary<ClientPeer, int>();

        /// <summary>
        /// 客户端对象 对应的分数数据字典
        /// </summary>
        private Dictionary<ClientPeer, int> clientScoreDict = new Dictionary<ClientPeer, int>();

        /// <summary>
        /// 用来存储客户端连接对象
        /// </summary>
        private List<ClientPeer> clientPeers = new List<ClientPeer>();

        /// <summary>
        /// 用来存储一局游戏房间内的客户端用户信息
        /// </summary>
        private Dictionary<RoomInfo, Dictionary<UserInfo, int>> roomUserScoreDict = new Dictionary<RoomInfo, Dictionary<UserInfo, int>>();

        /// <summary>
        /// 用来存储游戏房间内弃牌的玩家的数据字典(一次游戏结束后,需要将数据清空)
        /// </summary>
        private Dictionary<RoomInfo, List<ClientPeer>> roomDiscardDict = new Dictionary<RoomInfo, List<ClientPeer>>();

        /// <summary>
        /// 记录一个房间的游戏局数的数据字典(一次游戏结束后,需要将数据清空)
        /// </summary>
        private Dictionary<RoomInfo, int> roomRecordsNumberDict = new Dictionary<RoomInfo, int>();

        /// <summary>
        /// 房间数据缓存对象
        /// </summary>
        private RoomCache roomCache = Caches.RoomCache;

        #region 初始化和重置卡牌数据
        /// <summary>
        /// 初始卡牌数据
        /// </summary>
        public void InitCardsData()
        {
            this.cardColors = new string[] { "Heart", "Spade", "Square", "Club" };
            this.cardValues = new string[] { "Nine", "Ten", "Jack", "Queen", "King", "One" }; 
        }

        /// <summary>
        /// 重置卡牌
        /// </summary>
        public void ResetCards()
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
            LogMessage.Instance.SetLogMessage("创建房间后,将一副牌打乱后的结果：");
            StringBuilder sb = new StringBuilder();
            string str = string.Empty;
            foreach (string cardValue in this.resCards)
            {
                sb.Append(cardValue + ",\n");
            }
            if (sb.Length > 0)
                str = sb.ToString().Remove(sb.Length - 1, 1);
            LogMessage.Instance.SetLogMessage(str);
        }

        #endregion

        #region 随机获取卡牌数据
        /// <summary>
        /// 获取随机卡牌
        /// </summary>
        /// <returns></returns>
        public string GetRandomCard()
        {
            int index = this.ranCardIndex.Next(0, this.resCards.Count);
            string cardStr = this.resCards[index];
            this.resCards.RemoveAt(index);
            return cardStr;
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

        #region 针对房间内玩家的操作
        /// <summary>
        /// 添加房间信息对象
        /// </summary>
        /// <param name="roomInfo">要添加的房间信息</param>
        public void AddRoomInfo(RoomInfo roomInfo)
        {
            Dictionary<UserInfo, int> userScoreDict = new Dictionary<UserInfo, int>();//构建一个用于存储用户信息 对应 用户分数的数据字典
            foreach (UserInfo user in roomInfo.UserInfos)//循环遍历房间内的每一个玩家
            {
                if (!userScoreDict.ContainsKey(user))
                    userScoreDict.Add(user, 0);//添加到存储用户信息 和 分数 的字典中
            }
            if (!this.roomUserScoreDict.ContainsKey(roomInfo))
                this.roomUserScoreDict.Add(roomInfo, userScoreDict);//将房间和用户信息存储起来;
            else
                this.roomUserScoreDict[roomInfo] = userScoreDict;
        }

        /// <summary>
        /// 通过房间编号更改存储用户信息和用户分数的数据字典
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userinfoArray"></param>
        public void ChangeRoomUserScoresDictionaryByRoomId(int roomId, params UserInfo[] userinfoArray)
        {
            RoomInfo tmpRoomInfo = this.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间信息数据
            if (tmpRoomInfo != null)//如果获取到了房间
            {
                for (int userIndex = 0; userIndex < userinfoArray.Length; userIndex++)//使用循环遍历新进房间来的用户
                    this.roomUserScoreDict[tmpRoomInfo].Add(userinfoArray[userIndex], 0);//将他们添加到数据字典中
            }
        }

        /// <summary>
        /// 根据房间编号给房间内的客户端用户发底牌数据(只需在一局游戏中第一轮发)
        /// </summary>
        /// <param name="roomId"></param>
        public void GetTouchCardByRoomId(int roomId)
        {
            LogMessage.Instance.SetLogMessage("没发底牌之前剩余的牌数：" + this.resCards.Count.ToString());
            RoomInfo tmpRoomInfo = GetRoomInfoByRoomId(roomId);
            if (tmpRoomInfo != null)//如果不为空的话 表示根据房间编号取到了对应的房间数据
            {
                this.RecordsNumberByRoomId(roomId);
                ClientPeer tmpClientPeer = null;
                //LogMessage.Instance.SetLogMessage("要发底牌的房间中的用户个数：" + tmpRoomInfo.UserInfos.Count.ToString());
                foreach (UserInfo user in tmpRoomInfo.UserInfos)//循环遍历这个房间中的客户端用户列表
                {
                    //判断用户是否为空
                    if (user != null)//如果不为空,再进行具体的发牌细节处理
                    {
                        tmpClientPeer = this.clientPeers.Find(clientPeer => clientPeer.ClientSocket == user.ClientUserSocket);//校验是不是同一个客户端连接对象
                        if (tmpClientPeer != null)//如果不为空 则表示是同一个客户端连接对象
                        {
                            string tmpCardValue = this.GetRandomCard();//随机取出一张牌(这张牌发给当前客户端对象)
                            //根据房间编号和用户信息记录用户分数
                            this.SetUserScoreByRoomId(roomId, user, tmpCardValue);
                            //这里需要发送两次消息(第一次是当前客户端自己的底牌数据,第二次是要给这个客户端发送一些假数据,表示是其他客户端对象的底牌数据)
                            LogMessage.Instance.SetLogMessage("玩家[ " + tmpClientPeer.ClientSocket.RemoteEndPoint.ToString() + " ]发到的底牌-> " + tmpCardValue + "," + user.ClientIndex.ToString());
                            //第一次发消息:给当前客户端对象先发一张自己的底牌
                            tmpClientPeer.OnSendMessage(
                                new SocketMessage()
                                {
                                    OperationCode = OperationCode.Service,
                                    SubOperationCode = (int)ServiceCode.Passe_Response,
                                    DataValue = tmpCardValue + "," + user.ClientIndex.ToString()
                                });

                            //循环遍历所有客户端对象 找到不是当前发底牌的客户端对象
                            for (int clientPeerIndex = 0; clientPeerIndex < this.clientPeers.Count; clientPeerIndex++)
                            {
                                ClientPeer clientPeer = this.clientPeers[clientPeerIndex];
                                if (clientPeer != tmpClientPeer)//如果不是当前客户端对象
                                {
                                    //第二次发消息:给当前抽到底牌的客户端对象发送假数据
                                    tmpClientPeer.OnSendMessage(new SocketMessage()
                                    {
                                        OperationCode = OperationCode.Service,
                                        SubOperationCode = (int)ServiceCode.Passe_Response,
                                        DataValue = "底牌" + "," + user.ClientIndex.ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            LogMessage.Instance.SetLogMessage("发了底牌之前剩余的牌数：" + this.resCards.Count.ToString());
        }

        /// <summary>
        /// 根据房间编号获取房间信息数据
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        private RoomInfo GetRoomInfoByRoomId(int roomId)
        {
            RoomInfo tmpRoomInfo = null;//用于存储获取到的房间信息数据
            //循环遍历整个数据字典
            foreach (KeyValuePair<RoomInfo, Dictionary<UserInfo, int>> roomIem in this.roomUserScoreDict)
            {
                RoomInfo roomInfo = roomIem.Key;
                if (roomInfo.Id == roomId)//判断当前循环遍历到的房间的编号与要取的房间信息的编号是否一致(如果一致,将房间信息记录下来,跳出循环)
                {
                    tmpRoomInfo = roomInfo;
                    break;
                }
            }
            return tmpRoomInfo;
        }

        /// <summary>
        /// 根据房间编号给房间内的客户端用户发明牌数据(只需在一局游戏中第二轮发)
        /// </summary>
        /// <param name="roomId"></param>
        public void GetClearCardByRoomId(int roomId)
        {
            LogMessage.Instance.SetLogMessage("没发明牌之前剩余的牌数：" + this.resCards.Count.ToString());
            RoomInfo tmpRoomInfo = this.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间信息数据
            if (tmpRoomInfo != null)//如果不为空 则表示通过房间编号成功获取到了房间信息数据
            {
                this.RecordsNumberByRoomId(roomId);//记录游戏次数
                StringBuilder cardDataSb = new StringBuilder();
                string cardData = string.Empty;
                for (int userIndex = 0; userIndex < tmpRoomInfo.UserInfos.Count; userIndex++)//循环遍历房间内的用户列表
                {
                    UserInfo user = tmpRoomInfo.UserInfos[userIndex];//取得当前遍历到的用户
                    if (user != null)//如果不为空了 成功取到了用户
                    {
                        ClientPeer clientPeer = this.clientPeers.Find(peer => peer.ClientSocket == user.ClientUserSocket);//取得跟当前遍历到的用户 相符 的客户端连接对象
                        string tmpCard = this.GetRandomCard();//随机取出一张卡牌
                        //记录分数
                        this.SetUserScoreByRoomId(roomId, user, tmpCard);
                        if (!string.IsNullOrEmpty(tmpCard))//如果取得卡牌不为空的情况下
                        {
                            LogMessage.Instance.SetLogMessage("玩家[ " + clientPeer.ClientSocket.RemoteEndPoint.ToString() + " ]发到的明牌-> " + tmpCard + "," + user.ClientIndex.ToString());
                            //给这个要发牌的客户端发送卡牌数据
                            clientPeer.OnSendMessage
                                (
                                        new SocketMessage()
                                        {
                                            OperationCode = OperationCode.Service,
                                            SubOperationCode = (int)ServiceCode.Passe_Response,
                                            DataValue = tmpCard + "," + user.ClientIndex.ToString()
                                        }
                                );
                            cardDataSb.Append(tmpCard + "," + user.ClientIndex.ToString() + "|");
                        }
                        else//如果取到的卡牌为空(这里可能会通知房间内的所有客户端用户对象,告诉它们本局基本牌数已经发完,可能需要开始计算分数了)
                            LogMessage.Instance.SetLogMessage("本局中已经没有卡牌了~");
                    }
                }
                if (cardDataSb.Length > 0)
                    cardData = cardDataSb.ToString().Remove(cardDataSb.Length - 1, 1);
                //LogMessage.Instance.SetLogMessage(cardData);

                this.roomCache.BroadcastMessageByRoomId(roomId, new SocketMessage()
                {
                    OperationCode = OperationCode.Service,
                    SubOperationCode = (int)ServiceCode.Passe_BroadcastResponse,
                    DataValue = cardData
                });

            }
            else
                return;
            LogMessage.Instance.SetLogMessage("发了明牌之前剩余的牌数：" + this.resCards.Count.ToString());

            //测试当局游戏计分情况
            #region 这里测试在发完一张底牌 和 一张明牌后的每个玩家得到的分数情况
            //foreach (KeyValuePair<RoomInfo, Dictionary<UserInfo, int>> item in this.roomUserScores)
            //{
            //    foreach (KeyValuePair<UserInfo, int> valueItem in item.Value)
            //        LogMessage.Instance.SetLogMessage(valueItem.Key.UserName + " 获得了" + valueItem.Value.ToString());
            //}
            #endregion
        }

        /// <summary>
        /// 根据房间编号设置房间内指定用户的分数
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userInfo"></param>
        /// <param name="cardData"></param>
        public void SetUserScoreByRoomId(int roomId, UserInfo userInfo, string cardData)
        {
            if (roomId < 0 || userInfo == null || string.IsNullOrEmpty(cardData))//做参数数据完整校验工作(防止程序抛出异常)
                return;
            RoomInfo tmpRoomInfo = this.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间信息数据
            int userScore = 0;//临时存储用户分数数据
            bool isGetUserInfoSuccess = this.roomUserScoreDict[tmpRoomInfo].TryGetValue(userInfo, out userScore);
            //如果获取成功,则开始计算分数
            if (isGetUserInfoSuccess)
            {
                //循环遍历卡牌值得数组
                foreach (string cardValue in this.cardValues)
                {
                    if (cardData.Contains(cardValue))//如果传递过来的卡牌数据中包含卡牌值的话,就计算分数即可
                    {
                        switch (cardValue)//判断卡牌的值
                        {
                            case "Nine":// +9分
                                userScore += 9; break;
                            case "Ten":// +10分
                                userScore += 10; break;
                            case "Jack":// +11分
                                userScore += 11; break;
                            case "Queen":// +12分
                                userScore += 12; break;
                            case "King":// +13分
                                userScore += 13; break;
                            case "One":// +15分
                                userScore += 15; break;
                        }
                        break;//这里使用break关键字的用意在于,只进行循环遍历一次,目的是节省性能.
                    }
                    else
                        continue;
                }
                if (userScore != 0)//表示计算了一次分数,需要根据用户信息来重新给用户赋分数的值
                    this.roomUserScoreDict[tmpRoomInfo][userInfo] = userScore;//为用户的分数值重新赋值
            }
        }

        /// <summary>
        /// 根据房间编号显示指定房间内的所有用户当前分数
        /// </summary>
        /// <param name="roomId"></param>
        public void DisplayAllUserScoresByRoomId(int roomId)
        {
            RoomInfo tmpRoomInfo = this.GetRoomInfoByRoomId(roomId);
            if (tmpRoomInfo != null)
            {
                foreach (KeyValuePair<UserInfo, int> userItem in this.roomUserScoreDict[tmpRoomInfo])
                    LogMessage.Instance.SetLogMessage("玩家[ " + userItem.Key.UserName + " ] 当前一共获得了[ " + userItem.Value.ToString() + " ]分数~");
            }
        }

        /// <summary>
        /// 根据房间编号比较房间内的客户端用户分数
        /// </summary>
        /// <param name="roomId"></param>
        public void CompleteUserScoreByRoomId(int roomId)
        {
            RoomInfo tmpRoomInfo = this.GetRoomInfoByRoomId(roomId);
            if (tmpRoomInfo != null)
            {
                UserInfo userInfo = null;//存储分数最大的玩家信息数据
                int maxUserScore = int.MinValue;//存储最大分数
                List<ClientPeer> clients = new List<ClientPeer>();//存储所有房间内的客户端用户对象
                foreach (KeyValuePair<UserInfo, int> userItem in this.roomUserScoreDict[tmpRoomInfo])
                {
                    int tmpUserScore = userItem.Value;
                    if (tmpUserScore > maxUserScore)
                    {
                        userInfo = userItem.Key;
                        maxUserScore = tmpUserScore;
                    }
                    for (int clientIndex = 0; clientIndex < this.clientPeers.Count; clientIndex++)
                    {
                        if (userItem.Key.ClientUserSocket == this.clientPeers[clientIndex].ClientSocket)
                        {
                            if (!clients.Contains(this.clientPeers[clientIndex]))
                                clients.Add(this.clientPeers[clientIndex]);
                        }
                    }
                }
                if (userInfo != null)
                {
                    LogMessage.Instance.SetLogMessage(userInfo.UserName + " 取得了游戏胜利~");
                    if (clients != null && clients.Count > 0)
                    {
                        for (int clientIndex = 0; clientIndex < clients.Count; clientIndex++)
                        {
                            ClientPeer clientPeer = clients[clientIndex];
                            if (userInfo.ClientUserSocket == clientPeer.ClientSocket)//这是要通知给胜利的客户端用户玩家
                            {
                                //给获胜的玩家发送胜利消息
                                clientPeer.OnSendMessage
                                  (
                                        new SocketMessage()
                                        {
                                            OperationCode = OperationCode.Service,
                                            SubOperationCode = (int)GameResultCode.Game_Success_Response,
                                            DataValue = "你的分数最大,你胜利了~"
                                        }
                                );
                            }
                            else//这是要通知给失败的客户端用户玩家
                            {
                                //给失败的玩家发送胜利消息
                                clientPeer.OnSendMessage
                                  (
                                        new SocketMessage()
                                        {
                                            OperationCode = OperationCode.Service,
                                            SubOperationCode = (int)GameResultCode.Game_Success_Response,
                                            DataValue = "你失败了~"
                                        }
                                );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 通过房间编号设置弃牌玩家
        /// </summary>
        /// <param name="roomId">玩家弃牌的房间编号</param>
        /// <param name="clientPeerArray">弃牌的玩家数组</param>
        public void SetDiscardUserByRoomId(int roomId, params ClientPeer[] clientPeerArray)
        {
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间信息数据
            if (!this.roomDiscardDict.ContainsKey(roomInfo))
                this.roomDiscardDict.Add(roomInfo, clientPeerArray.ToList());
            else
                this.roomDiscardDict[roomInfo] = clientPeerArray.ToList();
        }

        /// <summary>
        /// 根据房间编号记录游戏局数
        /// </summary>
        /// <param name="roomId">要记录的房间编号数据</param>
        public void RecordsNumberByRoomId(int roomId)
        {
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间信息数据
            if (!this.roomRecordsNumberDict.ContainsKey(roomInfo))//如果记录房间的游戏局数中不存在房间信息 
                this.roomRecordsNumberDict.Add(roomInfo, 1);//直接存储
            else//如果存在
                this.roomRecordsNumberDict[roomInfo]++;//直接累加游戏局数后进行存储
        }

        #endregion

        /// <summary>
        /// 给指定的客户端发明牌
        /// </summary>
        /// <param name="peer"></param>
        public void GetClearCardToClient(ClientPeer peer, int roomId)
        {
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);
            for (int userIndex = 0; userIndex < roomInfo.UserInfos.Count; userIndex++)
            {
                UserInfo tmpUserInfo = roomInfo.UserInfos[userIndex];
                if (tmpUserInfo.ClientUserSocket == peer.ClientSocket)
                {
                    string tmpCard = this.GetRandomCard();
                    LogMessage.Instance.SetLogMessage(peer.ClientSocket.RemoteEndPoint.ToString() + "得到的明牌：" + tmpCard);
                    peer.OnSendMessage(new SocketMessage()
                    {
                        OperationCode = OperationCode.Service,
                        SubOperationCode = (int)ServiceCode.Passe_Response,
                        DataValue = tmpCard + "," + tmpUserInfo.ClientIndex
                    });
                    break;
                }
            }
        }

        #region 针对所有玩家的一致操作

        /// <summary>
        /// 给客户端发明牌
        /// </summary>
        /// <param name="clients"></param>
        /// <param name="clientIndex"></param>
        public void GetClearCard(List<ClientPeer> clients, int clientIndex)
        {
            ClientPeer peer = clients[clientIndex];
            //   GetClearCardToClient(peer);
        }



        /// <summary>
        /// 给客户端发底牌
        /// </summary>
        /// <param name="clients"></param>
        /// <param name="clientIndex"></param>
        public void GetTouchCard(List<ClientPeer> clients, int clientIndex)
        {
            ClientPeer currentClientPeer = clients[clientIndex];
            if (!this.clientScoreDict.ContainsKey(currentClientPeer))
                this.clientScoreDict.Add(currentClientPeer, 0);
            string card = this.GetRandomCard();
            //Console.WriteLine(currentClientPeer.ClientSocket.RemoteEndPoint.ToString() + "摸到底牌：" + card);
            LogMessage.Instance.SetLogMessage(currentClientPeer.ClientSocket.RemoteEndPoint.ToString() + "摸到底牌：" + card);
            foreach (string cardValue in this.cardValues)
            {
                if (card.Contains(cardValue))
                    this.GetScore(clients[clientIndex], cardValue);
            }
            currentClientPeer.OnSendMessage(
                new SocketMessage()
                {
                    OperationCode = OperationCode.Service,
                    SubOperationCode = (int)ServiceCode.Passe_Response,
                    DataValue = card
                });
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i] != currentClientPeer)
                {
                    ClientPeer tmpClientPeer = clients[i];
                    if (!this.clientScoreDict.ContainsKey(tmpClientPeer))
                        this.clientScoreDict.Add(tmpClientPeer, 0);
                    tmpClientPeer.OnSendMessage(new SocketMessage()
                    {
                        OperationCode = OperationCode.Service,
                        SubOperationCode = (int)ServiceCode.Passe_Response,
                        DataValue = "底牌"
                    });
                    string tmpCard = this.GetRandomCard();
                    foreach (string cardValue in this.cardValues)
                    {
                        if (tmpCard.Contains(cardValue))
                            this.GetScore(tmpClientPeer, cardValue);
                    }
                }
            }
        }

        /// <summary>
        /// 根据卡牌的值获取分数
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="cardValue"></param>
        public void GetScore(ClientPeer peer, string cardValue)
        {
            if (this.clientScoreDict.ContainsKey(peer))
            {
                switch (cardValue)
                {
                    case "Nine":
                        this.clientScoreDict[peer] += 9;
                        break;
                    case "Ten":
                        this.clientScoreDict[peer] += 10;
                        break;
                    case "Jack":
                        this.clientScoreDict[peer] += 11;
                        break;
                    case "Queen":
                        this.clientScoreDict[peer] += 12;
                        break;
                    case "King":
                        this.clientScoreDict[peer] += 13;
                        break;
                    case "One":
                        this.clientScoreDict[peer] += 15;
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientPeer"></param>
        public void RemoveUserScoreofUserScoreDict(ClientPeer clientPeer)
        {
            this.clientScoreDict.Remove(clientPeer);
        }

        /// <summary>
        /// 比较客户端对象之间的分数大小
        /// </summary>
        public void CompleteScore()
        {
            //计分处理(比较每个玩家的大小值)
            //比较完毕通知给客户端结果
            ClientPeer tmpClientPeer = null;//记录分数高的客户端
            int maxScore = int.MinValue;//记录最大分数
            foreach (KeyValuePair<ClientPeer, int> clientScoreItem in this.clientScoreDict)//循环当前进入房间内的客户端对象 判断校验谁的分数最大
            {
                //Console.WriteLine("客户端对象-> {0} 获得的总分为：{1} .", clientScoreItem.Key.ClientSocket.RemoteEndPoint.ToString(), clientScoreItem.Value.ToString());
                if (clientScoreItem.Value > maxScore)
                {
                    maxScore = clientScoreItem.Value;
                    tmpClientPeer = clientScoreItem.Key;
                }
            }
            //   GetGameResult(tmpClientPeer);
        }

        /// <summary>
        /// 获取游戏结果(并需要通知胜利的玩家和失败的玩家,推送消息给他们)
        /// </summary>
        /// <param name="clientPeer">获得胜利的客户端对象</param>
        private void GetGameResult(ClientPeer clientPeer)
        {
            if (clientPeer != null)
            {
                Console.WriteLine("客户端对象 -> {0} 获取了游戏的胜利~", clientPeer.ClientSocket.RemoteEndPoint.ToString());
                //给获胜的玩家发送胜利消息
                clientPeer.OnSendMessage(new SocketMessage()
                {
                    OperationCode = OperationCode.Service,
                    SubOperationCode = (int)GameResultCode.Game_Success_Response,
                    DataValue = "你的分数最大,你胜利了~"
                });
                foreach (KeyValuePair<ClientPeer, int> clientScoreItem in this.clientScoreDict)//循环遍历没有获胜的玩家 通知他们游戏失败的信息
                {
                    if (clientScoreItem.Key != clientPeer)//代表不是获胜的玩家
                    {
                        //给没有胜利的玩家推送消息
                        clientScoreItem.Key.OnSendMessage(
                            new SocketMessage()
                            {
                                OperationCode = OperationCode.Service,
                                SubOperationCode = (int)GameResultCode.Game_Success_Response,
                                DataValue = "你失败了~"
                            });
                    }
                }
            }
            else
                return;
            //Todo:推送消息完成,等待下局游戏开始(这里首先判断一局游戏结束时,是否有玩家退出,如果有玩家退出,需要进入游戏等待状态,直到满足最低游戏人数再开始下一局的游戏)
        }

        #endregion
    }
}
