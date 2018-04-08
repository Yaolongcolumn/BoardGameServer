using Dlzyff.BoardGame.BottomServer.Concurrents;
using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGame.BottomServer.Tools;
using Dlzyff.BoardGame.Protocol.Codes;
using Dlzyff.BoardGame.Protocol.Dto;
using Dlzyff.BoardGameServer.Log;
using Dlzyff.BoardGameServer.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dlzyff.BoardGameServer.DataCache.Services
{
    /// <summary>
    /// 五轰六炸业务缓存类
    /// </summary>
    public class FivebombsWithSixbombsServiceCache : PokerServiceCache
    {
        /// <summary>
        /// 房间数据对应房间内的玩家持有的卡牌数据 数据字典
        /// </summary>
        private Dictionary<RoomInfo, Dictionary<UserInfo, List<string>>> roomUserExistCardDict = new Dictionary<RoomInfo, Dictionary<UserInfo, List<string>>>();

        /// <summary>
        /// 存储玩家排序好的牌的数据字典
        /// </summary>
        private Dictionary<RoomInfo, Dictionary<ClientPeer, List<string>>> roomUserSortCardDict = new Dictionary<RoomInfo, Dictionary<ClientPeer, List<string>>>();

        /// <summary>
        /// 玩家阵营数据字典
        /// </summary>
        private Dictionary<int, List<UserInfo>> userCampDict = new Dictionary<int, List<UserInfo>>();

        /// <summary>
        /// 用来保存房间中当前出牌的玩家数据字典(顺便也就存储了当前玩家所打出的手牌)
        /// </summary>
        private Dictionary<RoomInfo, Dictionary<UserInfo, List<string>>> roomCurrentPlayCardDict = new Dictionary<RoomInfo, Dictionary<UserInfo, List<string>>>();

        /// <summary>
        /// 线程安全的整数类型
        /// </summary>
        private ConcurrentInteger concurrentInteger = new ConcurrentInteger(0);

        /// <summary>
        /// 用来保存房间中获胜玩家的获胜顺序的数据字典(其实就是用来存储出完手里所有手牌的玩家)
        /// (如果第一个玩家 和 第二个玩家同属一个阵营 则这个阵营触发获胜事件)
        /// (如果第一个玩家 和 第二个玩家不同属一个阵营 则两个阵营触发凉水事件)
        /// </summary>
        private Dictionary<RoomInfo, Dictionary<UserInfo, int>> roomWinUserDict = new Dictionary<RoomInfo, Dictionary<UserInfo, int>>();

        /// <summary>
        /// 存储用于房间内排序的卡牌
        /// </summary>
        private Dictionary<RoomInfo, Dictionary<UserInfo, Dictionary<int, List<int>>>> sortCardValueDict = new Dictionary<RoomInfo, Dictionary<UserInfo, Dictionary<int, List<int>>>>();

        /// <summary>
        /// 随机索引值
        /// </summary>
        private Random ranIndex = new Random(0);

        /// <summary>
        /// 扑克处理类
        /// </summary>
        private PokerProcess pokerProcess = null;

        /// <summary>
        /// 网络消息
        /// </summary>
        private SocketMessage message = new SocketMessage();

        #region 初始化和重置卡牌数据
        /// <summary>
        /// 初始化卡牌数据
        /// </summary>
        public sealed override void InitCardsData()
        {
            base.InitCardsData();
            this.XiaowangCount = 3;
            this.DawangCount = 3;
            this.TeCount = 3;
            this.pokerProcess = new PokerProcess();
            // 13 [卡牌值得个数] * 3 [卡牌的副数]
            List<string> tmpCardValues = new List<string>();//定义一个临时存储卡牌值的集合
            string[] cardValues = new string[] { "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King", "One" };//构建一副卡牌值的数组
            for (int numberIndex = 0; numberIndex < 3; numberIndex++)
            {
                for (int i = 0; i < cardValues.Length; i++)//外层循环遍历3次(这里3次是因为五轰六炸卡牌游戏为3副扑克牌)
                    tmpCardValues.Add(cardValues[i]);//每次遍历完成 添加到存储卡牌值的临时集合中
            }
            int cardValueCount = tmpCardValues.Count;
            this.cardValues = new string[cardValueCount];//重新构建存储卡牌值的集合数组
            for (int cardValueIndex = 0; cardValueIndex < cardValueCount; cardValueIndex++)
                this.cardValues[cardValueIndex] = tmpCardValues[cardValueIndex];
        }
        /// <summary>
        /// 重置卡牌数据
        /// </summary>
        public override void ResetCards()
        {
            base.ResetCards();
        }
        /// <summary>
        /// 随机获取一张卡牌
        /// </summary>
        /// <returns></returns>
        public override string GetRandomCard()
        {
            if (this.resCards.Count == 0)
                return "没有扑克牌可以抓取了~";
            int index = this.ranCardIndex.Next(0, this.resCards.Count);
            string cardStr = this.resCards[index];
            this.resCards.RemoveAt(index);
            return cardStr;
        }
        #endregion

        #region 增加房间信息
        /// <summary>
        /// 增加房间信息
        /// </summary>
        /// <param name="roomInfo"></param>
        public override void AddRoomInfo(RoomInfo roomInfo)
        {
            if (roomInfo != null)
            {
                Dictionary<UserInfo, List<string>> userCardDict = new Dictionary<UserInfo, List<string>>
                {
                    { roomInfo.UserInfos[0], new List<string>() }
                };
                if (!this.roomUserExistCardDict.ContainsKey(roomInfo))
                    this.roomUserExistCardDict.Add(roomInfo, userCardDict);
                if (!this.sortCardValueDict.ContainsKey(roomInfo))
                    this.sortCardValueDict.Add(roomInfo, new Dictionary<UserInfo, Dictionary<int, List<int>>>() { { roomInfo.UserInfos[0], new Dictionary<int, List<int>>() } });
            }
        }

        /// <summary>
        /// 通过房间编号，更改存储用户信息和用户持有卡牌的数据字典(这个方法主要用来预先做好存储玩家所持有的卡牌列表)
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userinfoArray"></param>
        public void ChangeRoomUserExistCardDictionaryByRoomId(int roomId, params UserInfo[] userinfoArray)
        {
            RoomInfo tmpRoomInfo = this.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间信息数据
            if (tmpRoomInfo != null)//如果获取到了房间
            {
                for (int userIndex = 0; userIndex < userinfoArray.Length; userIndex++)//使用循环遍历新进房间来的用户
                {
                    if (!this.roomUserExistCardDict[tmpRoomInfo].ContainsKey(userinfoArray[userIndex]))
                        this.roomUserExistCardDict[tmpRoomInfo].Add(userinfoArray[userIndex], new List<string>());//将他们添加到数据字典中
                    if (!this.sortCardValueDict[tmpRoomInfo].ContainsKey(userinfoArray[userIndex]))
                        this.sortCardValueDict[tmpRoomInfo].Add(userinfoArray[userIndex], new Dictionary<int, List<int>>());
                }
            }
        }
        #endregion

        #region 分配阵营
        //分配玩家阵营
        /// <summary>
        /// 根据房间号分配玩家阵营(这是默认的固定分配玩家的实现逻辑)
        /// </summary>
        /// <param name="roomId"></param>
        public void AssignUserCamp(int roomId)
        {
            this.userCampDict.Add(1, new List<UserInfo>());//添加阵营一
            this.userCampDict.Add(2, new List<UserInfo>());//添加阵营二
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);//根据指定房间编号获取房间数据
            if (roomInfo != null)
            {
                List<UserInfo> userInfos = roomInfo.UserInfos;//取出房间内的玩家数据列表
                //for (int userIndex = 0; userIndex < userInfos.Count; userIndex++)//遍历玩家列表
                //{
                //    int index = this.ranIndex.Next(0, userInfos.Count);//取出一个随机索引
                //    UserInfo userInfo = userInfos[index];//取出随机索引对应的玩家
                //    if (userIndex == 0 || userIndex == 2)//如果玩家小于两个人
                //        this.userCampDict[1].Add(userInfo);//将玩家添加至阵营一
                //    else//如果玩家不小于两个人
                //        this.userCampDict[2].Add(userInfo);//将玩家添加至阵营一
                //}
                this.userCampDict[1].Add(userInfos[0]);//把一号玩家添加到阵营一中
                this.userCampDict[2].Add(userInfos[1]);//把二号玩家添加到阵营二中
                this.userCampDict[1].Add(userInfos[2]);//把三号玩家添加到阵营一中
                this.userCampDict[2].Add(userInfos[3]);//把四号玩家添加到阵营二中

                #region 通知服务端应用层显示玩家阵营信息
                LogMessage.Instance.SetLogMessage("阵营 [ " + 1 + " ] 的玩家个数有 [ " + this.userCampDict[1].Count + " ] 个.");
                foreach (UserInfo user in this.userCampDict[1])
                    LogMessage.Instance.SetLogMessage("阵营 [ " + 1 + " ] 的玩家: " + user.UserName);
                LogMessage.Instance.SetLogMessage("阵营 [ " + 2 + " ] 的玩家个数有 [ " + this.userCampDict[2].Count + " ] 个.");
                foreach (UserInfo user in this.userCampDict[2])
                    LogMessage.Instance.SetLogMessage("阵营 [ " + 2 + " ] 的玩家: " + user.UserName);
                #endregion
            }
        }
        //随机分配玩家阵营 
        /// <summary>
        ///  根据房间号随机分配玩家阵营
        /// </summary>
        /// <param name="roomId"></param>
        public void RandomAssignUserCamp(int roomId)
        {
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);
            if (roomInfo != null)
            {
                int userCount = roomInfo.UserInfos.Count;
                int campCount = userCount / 2;
                for (int campIndex = 1; campIndex <= campCount; campIndex++)
                    this.userCampDict.Add(campIndex, new List<UserInfo>());
                List<UserInfo> userInfos = roomInfo.UserInfos;
                int campI = 0;
                for (int userIndex = 0; userIndex < userCount; userIndex++)
                {
                    int index = this.ranIndex.Next(0, userInfos.Count);//取出一个随机索引
                    UserInfo userInfo = userInfos[index];//取出随机索引对应的玩家
                    float rate = (float)this.ranIndex.Next(0, 1);
                    if (rate < 0.5f)
                        campI = this.ranIndex.Next(0, campCount);
                    else
                        campI = this.ranIndex.Next(0, campCount);
                    switch (campI)
                    {
                        case 1: this.userCampDict[campI].Add(userInfo); break;//将玩家添加至阵营一
                        case 2: this.userCampDict[campI].Add(userInfo); break;//将玩家添加至阵营二
                    }
                }
            }
        }
        #endregion

        #region 设定默认要打的手牌
        /// <summary>
        /// 处理获取一张默认要打的手牌 
        /// </summary>
        /// <param name="defaultCardStr">默认卡牌字符串</param>
        /// <returns></returns>
        public string SetDefaultPlayCard(string defaultCardStr) => this.resCards.Find(str => str.Contains(defaultCardStr));
        #endregion

        #region 玩家摸牌
        private List<int> cardNumbers = new List<int>();//存储所有的卡牌对应的数字
        private List<string> heartCardValues = new List<string>();//存储当前玩家抓取到的红桃卡牌
        private List<string> spadeCardValues = new List<string>();//存储当前玩家抓取到的黑桃卡牌
        private List<string> squareCardValues = new List<string>();//存储当前玩家抓取到的梅花卡牌
        private List<string> clubCardValues = new List<string>();//存储当前玩家抓取到的方片卡牌
        private List<string> otherCardValues = new List<string>();//存储当前玩家抓取到的其它卡牌
        private List<string> allValues = new List<string>();//存储玩家抓取到的所有牌
        private List<string> threeCardValues = new List<string>();//存储3的所有牌
        private List<string> fourCardValues = new List<string>();//存储4的所有牌
        private List<string> fiveCardValues = new List<string>();//存储5的所有牌
        private List<string> sixCardValues = new List<string>();//存储6的所有牌
        private List<string> sevenCardValues = new List<string>();//存储7的所有牌
        private List<string> eightCardValues = new List<string>();//存储8的所有牌
        private List<string> nineCardValues = new List<string>();//存储9的所有牌
        private List<string> tenCardValues = new List<string>();//存储10的所有牌
        private List<string> jackCardValues = new List<string>();//存储J的所有牌
        private List<string> queenCardValues = new List<string>();//存储Q的所有牌
        private List<string> kingCardValues = new List<string>();//存储K的所有牌
        private List<string> oneCardValues = new List<string>();//存储A的所有牌
        private List<string> twoCardValues = new List<string>();//存储2的所有牌
        private List<string> dawangCardValues = new List<string>();//存储小王的所有牌
        private List<string> xiaowangCardValues = new List<string>();//存储大王的所有牌
        private List<string> teCardValues = new List<string>();//存储特的所有牌

        private List<string> resValues = new List<string>();//存储卡牌值的结果
        private List<string> resCardValues = new List<string>();//存储最终要发送给玩家的手牌

        private List<string> userOneCardValues = new List<string>();//存储五轰六炸业务类别下的第一个玩家的所有牌
        private List<string> userTwoCardValues = new List<string>();//存储五轰六炸业务类别下的第二个玩家的所有牌
        private List<string> userThreeCardValues = new List<string>();//存储五轰六炸业务类别下的第三个玩家的所有牌
        private List<string> userFourCardValues = new List<string>();//存储五轰六炸业务类别下的第四个玩家的所有牌

        //处理玩家摸牌 
        /// <summary>
        /// 摸牌(默认情况下,是一下全部分发每个玩家持有的所有手牌)
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        public void TouchCard(int roomId)
        {
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间数据
            if (roomInfo != null)//检查获取到的房间是否为空
            {
                // int touchCount = this.resCards.Count / 4;//用于来限定玩家可以摸几次牌
                int touchCount = this.resCards.Count / 4;//用于来限定玩家可以摸几次牌
                                                         //   Console.WriteLine("一个玩家可以摸：" + touchCount + "张牌~");
                for (int i = 0; i < 4; i++)
                {
                    for (int touchIndex = 0; touchIndex < touchCount; touchIndex++)//这儿做循环遍历 目的是根据玩家摸牌的次数  一下将所有牌都分发给玩家
                    {
                        string card = this.GetRandomCard();//随机取出一张卡牌数据
                        while (card == string.Empty)//如果取到的卡牌为空 就继续取
                        {
                            card = this.GetRandomCard();//随机取出一张卡牌
                            touchIndex--;//等于空就将循环的遍历次数减少一次
                        }
                        if (card != string.Empty)//如果取到的卡牌不为空了(则表示抓到了一张牌)
                        {
                            //玩家摸牌之后 需要将玩家摸到的牌存储下来;
                            switch (i)
                            {
                                case 0:
                                    if (this.userOneCardValues.Count <= 40)
                                        this.userOneCardValues.Add(card);
                                    break;
                                case 1:
                                    if (this.userTwoCardValues.Count <= 40)
                                        this.userTwoCardValues.Add(card);
                                    break;
                                case 2:
                                    if (this.userThreeCardValues.Count <= 40)
                                        this.userThreeCardValues.Add(card);
                                    break;
                                case 3:
                                    if (this.userFourCardValues.Count <= 40)
                                        this.userFourCardValues.Add(card);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 清空卡牌集合
        /// </summary>
        private void ClearCardList()
        {
            #region 清空存牌的集合
            this.cardNumbers.Clear();
            this.threeCardValues.Clear();
            this.fourCardValues.Clear();
            this.fiveCardValues.Clear();
            this.sixCardValues.Clear();
            this.sevenCardValues.Clear();
            this.eightCardValues.Clear();
            this.nineCardValues.Clear();
            this.tenCardValues.Clear();
            this.jackCardValues.Clear();
            this.queenCardValues.Clear();
            this.kingCardValues.Clear();
            this.oneCardValues.Clear();
            this.twoCardValues.Clear();
            this.dawangCardValues.Clear();
            this.xiaowangCardValues.Clear();
            this.teCardValues.Clear();
            this.heartCardValues.Clear();
            this.spadeCardValues.Clear();
            this.squareCardValues.Clear();
            this.clubCardValues.Clear();
            this.otherCardValues.Clear();
            this.allValues.Clear();//先把之前的所有牌移除,防止多次添加(这里必须这么做,为了防止出现多次添加的操作)

            this.resValues.Clear();
            this.resCardValues.Clear();
            #endregion
        }

        /// <summary>
        /// 根据卡牌值获取卡牌颜色 和 对应的值
        /// </summary>
        /// <param name="strCardValue">要获取的卡牌值</param>
        /// <param name="cardColorNumber">取到的卡牌颜色数字</param>
        /// <param name="cardValue">取到的值</param>
        private void GetCardColorwithColorNumberByCardValue(string strCardValue, out int cardColorNumber, out string cardValue)
        {
            cardColorNumber = 0;
            cardValue = string.Empty;
            string tmpCardValue = strCardValue;

            #region 查找对应的卡牌颜色
            //遍历所有卡牌的颜色
            for (int colorIndex = 0; colorIndex < this.cardColors.Length; colorIndex++)
            {
                string tmpColorNumber = this.cardColors[colorIndex];//取出当前遍历到的卡牌颜色
                                                                    //判断存不存在
                if (tmpCardValue.Contains(tmpColorNumber))//如果存在,则是常规卡牌
                {
                    switch (tmpColorNumber)//判断取到的卡牌
                    {
                        case "Heart": cardColorNumber = 1; break;//红桃卡牌颜色
                        case "Spade": cardColorNumber = 2; break;//黑桃卡牌颜色
                        case "Square": cardColorNumber = 3; break;//梅花卡牌颜色
                        case "Club": cardColorNumber = 4; break;//方片卡牌颜色
                    }
                }
                else
                {
                    if (tmpCardValue.Contains("Other1"))//设置小王的卡牌颜色数字
                        cardColorNumber = 5;
                    else if (tmpCardValue.Contains("Other2"))//设置大王的卡牌颜色数字
                        cardColorNumber = 6;
                    else if (tmpCardValue.Contains("Other3"))//设置特的卡牌颜色数字
                        cardColorNumber = 7;
                }
            }
            #endregion

            #region 查找对应的卡牌值
            //遍历所有卡牌的值
            for (int cardValueIndex = 0; cardValueIndex < this.cardValues.Length; cardValueIndex++)
            {
                string tmpCardValueNumber = this.cardValues[cardValueIndex];
                if (tmpCardValue.Contains(tmpCardValueNumber))
                    cardValue = tmpCardValueNumber;
                else if (cardColorNumber == 5)
                    cardValue = "小王";
                else if (cardColorNumber == 6)
                    cardValue = "大王";
                else if (cardColorNumber == 7)
                    cardValue = "特";
            }
            #endregion

        }

        /// <summary>
        /// 给客户端对象发牌
        /// </summary>
        /// <param name="roomId"></param>
        public void GiveCard(int roomId)
        {
            StringBuilder stringBuilder = new StringBuilder();//用于拼接要发送给客户端对象的数据
            string sendCardData = string.Empty;//用于存储要发送给客户端对象的数据
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间信息
            List<UserInfo> userInfos = roomInfo.UserInfos;//获取房间内的玩家列表
            this.roomUserExistCardDict[roomInfo][userInfos[0]] = this.userOneCardValues;//设置玩家一的手牌
            this.roomUserExistCardDict[roomInfo][userInfos[1]] = this.userTwoCardValues;//设置玩家二的手牌
            this.roomUserExistCardDict[roomInfo][userInfos[2]] = this.userThreeCardValues;//设置玩家三的手牌
            this.roomUserExistCardDict[roomInfo][userInfos[3]] = this.userFourCardValues;//设置玩家四的手牌
            for (int userIndex = 0; userIndex < userInfos.Count; userIndex++)//循环遍历房间内的玩家列表
            {
                UserInfo user = userInfos[userIndex];//取出当前遍历到的玩家对象
                foreach (string existCardValueItem in this.roomUserExistCardDict[roomInfo][user])//循环遍历这个玩家持有的手牌列表
                {
                    this.GetCardColorwithColorNumberByCardValue(existCardValueItem, out int cardColorNumber, out string cardValue);//根据卡牌值获取卡牌颜色 和 值
                    this.pokerProcess.SetPokerValueNumberByPoker(cardColorNumber, cardValue, out int targetCardColorNumber, out int targetCardValue);//根据卡牌原有颜色数字 和 值的数字 取出 对应的 目标卡牌颜色数字 和 值的数字
                    //判断要排序的数据字典中是否存在这个颜色数字
                    if (!this.sortCardValueDict[roomInfo][user].ContainsKey(targetCardColorNumber))//不存在这个卡牌颜色数字
                        this.sortCardValueDict[roomInfo][user].Add(targetCardColorNumber, new List<int>() { targetCardValue });//进行添加这个卡牌颜色数字 和 对应的卡牌值
                    else
                        this.sortCardValueDict[roomInfo][user][targetCardColorNumber].Add(targetCardValue);//存在只需要添加对应的卡牌值即可
                }
            }

            for (int userIndex = 0; userIndex < userInfos.Count; userIndex++)//循环遍历玩家列表
            {
                stringBuilder.Clear();//将用于拼接卡牌数据的工具清空
                UserInfo userInfo = userInfos[userIndex];//取出当前遍历到的玩家对象
                Dictionary<int, List<int>> dict = this.sortCardValueDict[roomInfo][userInfo];//取出这位玩家要进行排序的手牌集合
                List<string> userCardValues = this.SortPokerCard(dict);//进行一个排序的骚操作
                userCardValues = this.GetSplitCardValue(userCardValues);//排序好后进行一个骚操作
                foreach (string card in userCardValues)//遍历这个骚操作
                    stringBuilder.Append(card + ",");//拼接卡牌数据,卡牌与卡牌之间使用,间隔开来
                //用来校验是否要移除最后一位没有价值的间隔符
                if (stringBuilder.Length > 0)
                    sendCardData = stringBuilder.ToString().Remove(stringBuilder.Length - 1, 1);//移除最后一位没有价值的间隔符

                #region 给玩家发牌
                ClientPeer peer = this.roomCache.GetClientPeerByUserInfo(userInfo);//根据用户信息获取客户端连接对象
                //更改网络消息
                //标识码[0:废话,1:摸牌,2:移除手牌,]-玩家摸到的手牌数据|客户端座位索引
                this.message.ChangeMessage(OperationCode.Service, (int)ServiceCode.FivebombsWithSixbombs_Response, "1" + "-" + sendCardData + "|" + userInfo.ClientIndex);
                peer.OnSendMessage(this.message);//给这个客户端连接对象发送网络消息 
                #endregion

            }
        }

        #endregion

        #region 拆分玩家手中的牌
        /// <summary>
        /// 拆分玩家的手牌值
        /// </summary>
        /// <param name="sourceCardValues"></param>
        /// <returns></returns>
        private List<string> GetSplitCardValue(List<string> sourceCardValues)
        {
            this.ClearCardList();
            foreach (string card in sourceCardValues)
            {
                string[] strDataSplits = card.Split(',');
                string cardColor = strDataSplits[0];//卡牌的颜色
                string cardValue = strDataSplits[1];//卡牌的值
                switch (cardColor)
                {
                    case "Heart": heartCardValues.Add(cardValue + "," + cardColor); break;
                    case "Spade": spadeCardValues.Add(cardValue + "," + cardColor); break;
                    case "Square": squareCardValues.Add(cardValue + "," + cardColor); break;
                    case "Club": clubCardValues.Add(cardValue + "," + cardColor); break;
                    case "Other1": otherCardValues.Add(cardValue + "," + cardColor); break;
                    case "Other2": otherCardValues.Add(cardValue + "," + cardColor); break;
                    case "Other3": otherCardValues.Add(cardValue + "," + cardColor); break;
                }
            }

            #region 添加所有颜色的卡牌
            this.allValues.AddRange(this.heartCardValues);//添加红桃颜色的卡牌
            this.allValues.AddRange(this.spadeCardValues);//添加黑桃颜色的卡牌
            this.allValues.AddRange(this.squareCardValues);//添加梅花颜色的卡牌
            this.allValues.AddRange(this.clubCardValues);//添加方片颜色的卡牌
            this.allValues.AddRange(this.otherCardValues);//添加其他标识颜色的卡牌 
            Console.WriteLine("当前玩家持有的卡牌总个数：" + this.allValues.Count);
            #endregion

            #region 做一个转换的操作
            foreach (string value in this.allValues)//遍历玩家手中持有的所有卡牌
            {
                string[] valueDataSplits = value.Split(',');
                string cardValue = valueDataSplits[0];
                switch (cardValue)
                {
                    case "Three": this.threeCardValues.Add(value); break;
                    case "Four": this.fourCardValues.Add(value); break;
                    case "Five": this.fiveCardValues.Add(value); break;
                    case "Six": this.sixCardValues.Add(value); break;
                    case "Seven": this.sevenCardValues.Add(value); break;
                    case "Eight": this.eightCardValues.Add(value); break;
                    case "Nine": this.nineCardValues.Add(value); break;
                    case "Ten": this.tenCardValues.Add(value); break;
                    case "Jack": this.jackCardValues.Add(value); break;
                    case "Queen": this.queenCardValues.Add(value); break;
                    case "King": this.kingCardValues.Add(value); break;
                    case "One": this.oneCardValues.Add(value); break;
                    case "Two": this.twoCardValues.Add(value); break;
                    case "小王": this.xiaowangCardValues.Add(value); break;
                    case "大王": this.dawangCardValues.Add(value); break;
                }
            }
            #endregion

            #region 向存储结果卡牌值的集合中添加各个不同存储卡牌值的集合对象
            this.resValues.AddRange(threeCardValues);
            this.resValues.AddRange(fourCardValues);
            this.resValues.AddRange(fiveCardValues);
            this.resValues.AddRange(sixCardValues);
            this.resValues.AddRange(sevenCardValues);
            this.resValues.AddRange(eightCardValues);
            this.resValues.AddRange(nineCardValues);
            this.resValues.AddRange(tenCardValues);
            this.resValues.AddRange(jackCardValues);
            this.resValues.AddRange(queenCardValues);
            this.resValues.AddRange(kingCardValues);
            this.resValues.AddRange(oneCardValues);
            this.resValues.AddRange(twoCardValues);
            this.resValues.AddRange(xiaowangCardValues);
            this.resValues.AddRange(dawangCardValues);
            #endregion

            #region 做一个转换的操作
            foreach (string value in this.resValues)
            {
                string[] valueDataSplits = value.Split(',');
                string cardValue = valueDataSplits[0];
                string cardColor = valueDataSplits[1];
                string res = cardColor + "" + cardValue;
                if (!string.IsNullOrEmpty(res))
                    this.resCardValues.Add(res);
            }
            #endregion

            return this.resCardValues;//将排序好的手牌返回
        }
        #endregion

        #region 玩家出牌
        /// <summary>
        /// 存储玩家当前出的牌的数据字典
        /// </summary>
        private Dictionary<string, int> playCardDict = new Dictionary<string, int>();
        /// <summary>
        /// 存储玩家的扑克牌细信息数据
        /// </summary>
        private PokerInfoDto pokerInfoDto = new PokerInfoDto();
        //处理玩家出牌 
        /// <summary>
        /// 出牌
        /// </summary>
        /// <param name="clientPeer">出牌的客户端连接对象</param>
        /// <param name="roomId">房间编号</param>
        /// <param name="removeCardArray">玩家出的牌(也就是要移除的牌,这个牌可能有多个,而不是就一个)</param>
        public void PlayCard(ClientPeer clientPeer, int roomId, params string[] removeCardArray)
        {
            this.playCardDict.Clear();//每次出牌之前先把之前存储的牌清空n
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);
            if (roomInfo != null)
            {
                //校验玩家是否出完牌了
                if (this.roomWinUserDict[roomInfo].ContainsKey(roomInfo.UserInfos.Find(user => user.ClientUserSocket == clientPeer.ClientSocket)))//这个玩家已经出完牌了
                {
                    //提示玩家：你已经没有手牌了,不能进行出牌~
                    //更改网络消息
                    //标识码[0:废话,1:摸牌,2:移除手牌,]-提示信息|客户端座位索引
                    this.message.ChangeMessage(OperationCode.Service, (int)ServiceCode.FivebombsWithSixbombs_Response, "0" + "-" + "你已经没有手牌了,不能进行出牌~" + "|" + this.roomCache.GetUserInfoByClientPeer(clientPeer).ClientIndex.ToString());
                    clientPeer.OnSendMessage(this.message);//发送给指定客户端对象
                    return;//直接return 不继续执行余下的代码体了
                }
                UserInfo userInfo = null;//存储出牌的玩家
                                         //发送消息通知这个客户端,需要移除的手牌
                StringBuilder builder = new StringBuilder();
                string resStr = string.Empty;//存储拼接好的卡牌数据
                foreach (UserInfo user in roomInfo.UserInfos)
                {
                    if (user.ClientUserSocket == clientPeer.ClientSocket)
                    {
                        //玩家出牌之后 需要根据玩家所出的牌 从玩家持有的手牌列表中移除该手牌即可
                        List<string> tmpCards = this.roomUserExistCardDict[roomInfo][user];
                        if (tmpCards.Count > 0)//大于0说明玩家手里还有手牌(也就是可以出牌)
                        {
                            for (int removeCardIndex = 0; removeCardIndex < removeCardArray.Length; removeCardIndex++)//遍历要移除的手牌数组
                            {
                                string tmpRemoveCard = removeCardArray[removeCardIndex];
                                if (tmpCards.Contains(tmpRemoveCard))//存在 就是表示玩家手中持有这张要移除的手牌
                                {

                                    #region 可有可无的骚操作
                                    if (!this.playCardDict.ContainsKey(tmpRemoveCard))
                                        this.playCardDict.Add(tmpRemoveCard, 1);
                                    else
                                        this.playCardDict[tmpRemoveCard]++;
                                    #endregion

                                    builder.Append(tmpRemoveCard + ",");//拼接要移除的卡牌数据
                                    tmpCards.Remove(tmpRemoveCard);//从数据集合中移除手牌
                                }
                                else//不存在 就是表示玩家手中已经不持有这张要移除的手牌了
                                    continue;  //不做任何处理 
                            }
                            //Todo:处理玩家出牌以后,需要将玩家手中牌进行一个排序(这里也可以不用排序)
                            userInfo = user;//记录出牌的玩家对象
                        }
                        else//不大于0说明玩家手里已经没有手牌了(也就是不能出牌)
                        {
                            //提示玩家：你已经没有手牌了,不能进行出牌~
                            //更改网络消息
                            //标识码[0:废话,1:移除手牌,]-提示信息|客户端座位索引
                            this.message.ChangeMessage(OperationCode.Service, (int)ServiceCode.FivebombsWithSixbombs_Response, "0" + "-" + "你已经没有手牌了,不能进行出牌~" + "|" + userInfo.ClientIndex);
                            clientPeer.OnSendMessage(this.message);//发送给指定客户端对象
                            if (!this.roomWinUserDict.ContainsKey(roomInfo))
                                this.roomWinUserDict.Add(roomInfo, new Dictionary<UserInfo, int>() { { userInfo, this.concurrentInteger.AddWithGet() } });
                            return;
                        }
                        this.roomUserExistCardDict[roomInfo][user] = tmpCards;//移除完之后,做一个为其重新赋值的骚操作.
                        //校验需不需要移除最后一个拼接符
                        if (builder.Length > 0)//需要
                            resStr = builder.ToString().Remove(builder.Length - 1, 1);//移除最后一个拼接符
                        //更改网络消息
                        //标识码[0:废话,1:摸牌,2:移除手牌,]-移除的手牌|移除手牌的客户端对象
                        this.message.ChangeMessage(OperationCode.Service, (int)ServiceCode.FivebombsWithSixbombs_Response, "2" + "-" + resStr + "|" + userInfo.ClientIndex);
                        clientPeer.OnSendMessage(this.message);//给出牌的客户端对象发送消息(是时候通知它需要移除一部分手牌了~)
                        break;
                    }
                    else
                        continue;//Todo:不做任何处理 
                }
                List<string> sourceCardValues = new List<string>();//存储玩家要出的所有牌
                foreach (KeyValuePair<string, int> playCardItem in this.playCardDict)
                    sourceCardValues.Add(playCardItem.Key);//将要出的牌添加到一个集合中
                //判断玩家要出牌的牌个数
                this.CheckUserPlayCardCount(userInfo, sourceCardValues.ToArray());//检查玩家出牌的个数
            }
        }

        /// <summary>
        /// 检查校验玩家要出牌的个数
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="cards"></param>
        /// <returns></returns>
        private void CheckUserPlayCardCount(UserInfo userInfo, params string[] cards)
        {
            Dictionary<string, List<string>> cardDict = new Dictionary<string, List<string>>();
            List<string> cardValueList = new List<string>();//构建一个临时存储玩家要出的手牌值的集合
            List<PokerInfoDto> pokerInfoDtos = new List<PokerInfoDto>();//构建一个扑克信息数据集合
            for (int cardIndex = 0; cardIndex < cards.Length; cardIndex++)//遍历玩家要出的手牌
            {
                string color = this.GetCardColorByCardData(cards[cardIndex]);//获取卡牌颜色
                string value = this.GetCardValueByCardData(cards[cardIndex]);//获取卡牌值
                //判断存不存在这个卡牌颜色数据
                if (!cardDict.ContainsKey(color))//不存在
                    cardDict.Add(color, new List<string>() { value });                //将卡牌颜色 和 对应的值 添加到卡牌数据字典中
                else//存在
                    cardDict[color].Add(value);//直接添加对应的值即可
            }
            foreach (KeyValuePair<string, List<string>> cardItem in cardDict)//遍历存储卡牌的数据字典
            {
                for (int cardIndex = 0; cardIndex < cardItem.Value.Count; cardIndex++)
                {
                    this.pokerInfoDto = new PokerInfoDto() { Color = cardItem.Key, Value = cardItem.Value[cardIndex] };
                    if (!pokerInfoDtos.Contains(this.pokerInfoDto))
                    {
                        pokerInfoDtos.Add(this.pokerInfoDto);
                        this.pokerInfoDto.Number = 1;
                    }
                    else
                    {
                        int index = pokerInfoDtos.FindIndex(poker => poker.Color == this.pokerInfoDto.Color && poker.Value == this.pokerInfoDto.Value && poker.Number == this.pokerInfoDto.Number);
                        pokerInfoDtos[index].Number++;
                    }
                }
            }
            string[] cardValues = cardValueList.ToArray();//取出玩家要出的手牌值的个数
            string resStr = string.Empty;//用于存储要发送给客户端对象的手牌结果[格式就是,手牌与手牌之间使用]
            switch (cardValues.Length)//判断出的手牌个数
            {
                case 1://处理单张手牌
                    if (this.IsOne(cardValues))
                    {
                        resStr = cardValues[0] + "," + 1;//手牌值+","+手牌个数
                        this.pokerInfoDto.ChangePokerInfo(pokerInfoDtos[0].Color, pokerInfoDtos[0].Value, pokerInfoDtos[0].Number);//使用扑克信息数据传输类记录玩家要打出的手牌数据信息
                    }

                    break;
                case 2://处理两张手牌
                    if (this.IsTwoSame(cardValues))
                    {
                        resStr = cardValues[0] + "," + 2;//手牌值+","+手牌个数
                        this.pokerInfoDto.ChangePokerInfo(pokerInfoDtos[0].Color, pokerInfoDtos[0].Value, pokerInfoDtos[0].Number);//使用扑克信息数据传输类记录玩家要打出的手牌数据信息
                    }

                    break;
                case 3://处理三张手牌
                    if (this.IsThreeSame(cardValues))
                    {
                        resStr = cardValues[0] + "," + 3;//手牌值+","+手牌个数
                        this.pokerInfoDto.ChangePokerInfo(pokerInfoDtos[0].Color, pokerInfoDtos[0].Value, pokerInfoDtos[0].Number);//使用扑克信息数据传输类记录玩家要打出的手牌数据信息
                    }
                    else if (this.IsSkybombs(cardValues))
                    {
                        resStr = cardValues[0] + "," + 3;//手牌值+","+手牌个数
                        this.pokerInfoDto.ChangePokerInfo(pokerInfoDtos[0].Color, pokerInfoDtos[0].Value, pokerInfoDtos[0].Number);//使用扑克信息数据传输类记录玩家要打出的手牌数据信息
                    }
                    else if (this.IsFloorbombs(cardValues))
                    {
                        resStr = cardValues[0] + "," + 3;//手牌值+","+手牌个数
                        this.pokerInfoDto.ChangePokerInfo(pokerInfoDtos[0].Color, pokerInfoDtos[0].Value, pokerInfoDtos[0].Number);//使用扑克信息数据传输类记录玩家要打出的手牌数据信息
                    }

                    break;
                case 4://处理四张手牌
                    if (this.IsFourSame(cardValues))
                    {
                        resStr = cardValues[0] + "," + 4;//手牌值+","+手牌个数
                        this.pokerInfoDto.ChangePokerInfo(pokerInfoDtos[0].Color, pokerInfoDtos[0].Value, pokerInfoDtos[0].Number);//使用扑克信息数据传输类记录玩家要打出的手牌数据信息
                    }

                    break;
                case 5://处理五张手牌
                    if (this.IsFivebombs(cardValues))
                    {
                        resStr = cardValues[0] + "," + 5;//手牌值+","+手牌个数
                        this.pokerInfoDto.ChangePokerInfo(pokerInfoDtos[0].Color, pokerInfoDtos[0].Value, pokerInfoDtos[0].Number);//使用扑克信息数据传输类记录玩家要打出的手牌数据信息
                    }

                    break;
                case 6://处理六张手牌
                    if (this.IsSixbombs(cardValues))
                    {
                        resStr = cardValues[0] + "," + 6;//手牌值+","+手牌个数
                        this.pokerInfoDto.ChangePokerInfo(pokerInfoDtos[0].Color, pokerInfoDtos[0].Value, pokerInfoDtos[0].Number);//使用扑克信息数据传输类记录玩家要打出的手牌数据信息
                    }
                    else if (this.IsAllDuizi(cardValues))
                    {
                        for (int cardIndex = 0; cardIndex < cardValues.Length; cardIndex += 2)
                            resStr += cardValues[cardIndex] + "," + 2 + "|";
                        if (resStr.Length > 0)
                            resStr = resStr.Remove(resStr.Length - 1, 1);//手牌值+","+手牌个数
                    }
                    break;
                case 7://处理七张手牌
                    if (this.IsSevenbombs(cardValues))
                    {
                        resStr = cardValues[0] + "," + 7;
                        this.pokerInfoDto.ChangePokerInfo(pokerInfoDtos[0].Color, pokerInfoDtos[0].Value, pokerInfoDtos[0].Number);//使用扑克信息数据传输类记录玩家要打出的手牌数据信息
                    }

                    break;
                case 8://处理八张手牌
                    if (this.IsEightbombs(cardValues))
                    {
                        resStr = cardValues[0] + "," + 8;
                        this.pokerInfoDto.ChangePokerInfo(pokerInfoDtos[0].Color, pokerInfoDtos[0].Value, pokerInfoDtos[0].Number);//使用扑克信息数据传输类记录玩家要打出的手牌数据信息
                    }
                    else if (this.IsAllDuizi(cardValues))
                    {
                        for (int cardIndex = 0; cardIndex < cardValues.Length; cardIndex += 2)
                            resStr += cardValues[cardIndex] + "," + 2 + "-";
                        if (resStr.Length > 0)
                            resStr = resStr.Remove(resStr.Length - 1, 1);//手牌值+","+手牌个数
                    }
                    break;
                case 9://处理九张手牌
                    if (this.IsNinebombs(cardValues))
                    {
                        resStr = cardValues[0] + "," + 9;//手牌值+","+手牌个数
                        this.pokerInfoDto.ChangePokerInfo(pokerInfoDtos[0].Color, pokerInfoDtos[0].Value, pokerInfoDtos[0].Number);//使用扑克信息数据传输类记录玩家要打出的手牌数据信息
                    }

                    break;
                case 10://处理十张手牌
                    if (this.IsTenbombs(cardValues))
                    {
                        resStr = cardValues[0] + "," + 10;//手牌值+","+手牌个数
                        this.pokerInfoDto.ChangePokerInfo(pokerInfoDtos[0].Color, pokerInfoDtos[0].Value, pokerInfoDtos[0].Number);//使用扑克信息数据传输类记录玩家要打出的手牌数据信息
                    }
                    else if (this.IsAllDuizi(cardValues))
                    {
                        for (int cardIndex = 0; cardIndex < cardValues.Length; cardIndex += 2)
                            resStr += cardValues[cardIndex] + "," + 2 + "-";
                        if (resStr.Length > 0)
                            resStr = resStr.Remove(resStr.Length - 1, 1);//手牌值+","+手牌个数
                    }
                    break;
                case 11://处理十一张手牌
                    if (this.IsElevenbombs(cardValues))
                    {
                        resStr = cardValues[0] + "," + 11;//手牌值+","+手牌个数
                        this.pokerInfoDto.ChangePokerInfo(pokerInfoDtos[0].Color, pokerInfoDtos[0].Value, pokerInfoDtos[0].Number);//使用扑克信息数据传输类记录玩家要打出的手牌数据信息
                    }

                    break;
                case 12://处理十二张手牌
                    if (this.IsTwelvebombs(cardValues))
                    {
                        resStr = cardValues[0] + "," + 12;//手牌值+","+手牌个数
                        this.pokerInfoDto.ChangePokerInfo(pokerInfoDtos[0].Color, pokerInfoDtos[0].Value, pokerInfoDtos[0].Number);//使用扑克信息数据传输类记录玩家要打出的手牌数据信息
                    }
                    else if (this.IsAllDuizi(cardValues))
                    {
                        for (int cardIndex = 0; cardIndex < cardValues.Length; cardIndex += 2)
                            resStr += cardValues[cardIndex] + "," + 2 + "-";
                        if (resStr.Length > 0)
                            resStr = resStr.Remove(resStr.Length - 1, 1);//手牌值+","+手牌个数
                    }
                    break;
            }
            ClientPeer clientPeer = this.roomCache.GetClientPeerByUserInfo(userInfo);//根据用户信息获取客户端连接对象
            int roomId = 0;
            #region 第一套发送字符串结果的方案
            //更改构建的网络消息
            //标识码[0:废话,1:显示其他玩家打出的手牌]-打出的手牌|打出手牌的客户端对象
            this.message.ChangeMessage(OperationCode.Service, (int)ServiceCode.FivebombsWithSixbombs_BroadcastResponse, "1" + "-" + resStr + "|" + userInfo.ClientIndex);
            //判断玩家在不在房间内
            if (this.roomCache.IsInRoom(clientPeer, out roomId))//如果在房间内
                this.roomCache.BroadcastMessageByRoomId(roomId, this.message);//房间进行广播消息一次,告知每个玩家,这个出牌的玩家出了什么牌,以及出牌的个数,包括这个玩家所在的座位号 
            #endregion

            #region 第二套发送数据传输类的方案
            this.pokerInfoDto.OwnerClientIndex = userInfo.ClientIndex;//设置客户端座位索引号
            //更改构建的网络消息
            //标识码[0:废话,1:显示其他玩家打出的手牌]-扑克信息数据传输类|打出手牌的客户端对象
            this.message.ChangeMessage(OperationCode.Service, (int)ServiceCode.FivebombsWithSixbombs_BroadcastResponse, "1" + "-" + this.pokerInfoDto + "|" + userInfo.ClientIndex);
            //判断玩家在不在房间内
            if (this.roomCache.IsInRoom(clientPeer, out roomId))//如果在房间内
                this.roomCache.BroadcastMessageByRoomId(roomId, this.message);//房间进行广播消息一次,告知每个玩家,这个出牌的玩家出了什么牌,以及出牌的个数,包括这个玩家所在的座位号 
            #endregion
        }

        #region 判断玩家出牌的牌型
        #region [方案一]
        /// <summary>
        /// 是否是一张
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        private bool IsOne(params string[] cards) => cards.Length == 1;//只需要判断是否只有一个元素即可
        /// <summary>
        /// 是否是两张相同的
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        private bool IsTwoSame(params string[] cards)
        {
            Console.WriteLine("处理两张相同的~");
            if (cards.Length != 2)//如果出牌的元素个数低于2 表示不是两张牌
                return false;
            return cards[0] == cards[1];//否则 判断两张要出的牌是否相同的
        }
        /// <summary>
        /// 是否是三张相同的
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        private bool IsThreeSame(params string[] cards)
        {
            Console.WriteLine("处理三张相同的~");
            if (cards.Length != 3)//如果出牌的元素个数低于3 表示不是三张牌
                return false;
            return cards[0] == cards[1] && cards[0] == cards[2];//否则 判断三张要出的牌是否相同的
        }
        /// <summary>
        /// 是否是四张相同的
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        private bool IsFourSame(params string[] cards)
        {
            if (cards.Length != 4)//如果出牌的元素个数低于4 表示不是四张牌
                return false;
            return cards[0] == cards[1] && cards[0] == cards[2] && cards[0] == cards[3];//否则 判断四张要出的牌是否相同的
        }
        /// <summary>
        /// 是否是五轰
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        private bool IsFivebombs(params string[] cards)
        {
            if (cards.Length != 5)
                return false;
            return cards[0] == cards[1] && cards[0] == cards[2] && cards[0] == cards[3] && cards[0] == cards[4];
        }
        /// <summary>
        /// 是否是六炸
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        private bool IsSixbombs(params string[] cards)
        {
            if (cards.Length != 6)
                return false;
            return cards[0] == cards[1] && cards[0] == cards[2] && cards[0] == cards[3] && cards[0] == cards[4] && cards[0] == cards[5];
        }
        /// <summary>
        /// 是否是七炸
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        private bool IsSevenbombs(params string[] cards)
        {
            if (cards.Length != 7)
                return false;
            return cards[0] == cards[1] && cards[0] == cards[2] && cards[0] == cards[3] && cards[0] == cards[4] && cards[0] == cards[5] && cards[0] == cards[6];
        }
        /// <summary>
        /// 是否是八炸
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        private bool IsEightbombs(params string[] cards)
        {
            if (cards.Length != 8)
                return false;
            return cards[0] == cards[1] && cards[0] == cards[2] && cards[0] == cards[3] && cards[0] == cards[4] && cards[0] == cards[5] && cards[0] == cards[6] && cards[0] == cards[7];
        }
        /// <summary>
        /// 是否是九炸
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        private bool IsNinebombs(params string[] cards)
        {
            if (cards.Length != 9)
                return false;
            return cards[0] == cards[1] && cards[0] == cards[2] && cards[0] == cards[3] && cards[0] == cards[4] && cards[0] == cards[5] && cards[0] == cards[6] && cards[0] == cards[7] && cards[0] == cards[8];
        }
        /// <summary>
        /// 是否是十炸
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        private bool IsTenbombs(params string[] cards)
        {
            if (cards.Length != 10)
                return false;
            return cards[0] == cards[1] && cards[0] == cards[2] && cards[0] == cards[3] && cards[0] == cards[4] && cards[0] == cards[5] && cards[0] == cards[6] && cards[0] == cards[7] && cards[0] == cards[8] && cards[0] == cards[9];
        }
        /// <summary>
        /// 是否是十一炸
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns> 
        private bool IsElevenbombs(params string[] cards)
        {
            if (cards.Length != 11)
                return false;
            return cards[0] == cards[1] && cards[0] == cards[2] && cards[0] == cards[3] && cards[0] == cards[4] && cards[0] == cards[5] && cards[0] == cards[6] && cards[0] == cards[7] && cards[0] == cards[8] && cards[0] == cards[9] && cards[0] == cards[10];
        }
        /// <summary>
        /// 是否是十二炸
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns> 
        private bool IsTwelvebombs(params string[] cards)
        {
            if (cards.Length != 12)
                return false;
            return cards[0] == cards[1] && cards[0] == cards[2] && cards[0] == cards[3] && cards[0] == cards[4] && cards[0] == cards[5] && cards[0] == cards[6] && cards[0] == cards[7] && cards[0] == cards[8] && cards[0] == cards[9] && cards[0] == cards[10] && cards[0] == cards[11];
        }
        /// <summary>
        /// 判断玩家出的牌是否全是对子(就是玩家出的牌是否都是两连对)
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        private bool IsAllDuizi(params string[] cards)
        {
            bool isSuccess = true;
            for (int strIndex = 0; strIndex < cards.Length; strIndex += 2)
            {
                int perStrIndex = strIndex - 1;//取出上一个位置的索引值
                if (perStrIndex == -1)
                    perStrIndex = cards.Length - 1;
                int nextStrIndex = strIndex + 1;//取出下一个位置的索引值
                if (nextStrIndex > cards.Length - 1)
                    nextStrIndex = cards.Length - 1;
                string perStr = cards[perStrIndex];//取出上一个位置的元素
                string nextStr = cards[nextStrIndex];//取出下一个位置的元素
                if (perStr != nextStr)//如果两个元素不相等 则表示不是顺子    
                    isSuccess = false;
            }
            return isSuccess;
        }
        /// <summary>
        /// 是否是天轰
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        private bool IsSkybombs(params string[] cards)
        {
            if (!this.IsThreeSame(cards))
                return false;
            int valueNumber1 = this.GetValueNumberByCardData(cards[0]);
            int valueNumber2 = this.GetValueNumberByCardData(cards[1]);
            int valueNumber3 = this.GetValueNumberByCardData(cards[2]);
            return valueNumber1 == 16 && valueNumber2 == 16 && valueNumber3 == 16;
        }
        /// <summary>
        /// 是否是地轰
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        private bool IsFloorbombs(params string[] cards)
        {
            if (!this.IsThreeSame(cards))
                return false;
            int valueNumber1 = this.GetValueNumberByCardData(cards[0]);
            int valueNumber2 = this.GetValueNumberByCardData(cards[1]);
            int valueNumber3 = this.GetValueNumberByCardData(cards[2]);
            return valueNumber1 == 15 && valueNumber2 == 15 && valueNumber3 == 15;
        }
        #endregion
        #endregion

        #region 根据卡牌数据获取一些值的操作
        /// <summary>
        /// 获取值的数字
        /// </summary>
        /// <param name="cardData"></param>
        /// <returns></returns>
        private int GetValueNumberByCardData(string cardData)
        {
            int valueNumber = 0;
            foreach (string cardItem in this.cardValues)
            {
                if (cardData.Contains(cardItem))
                {
                    switch (cardItem)
                    {
                        case "Two": valueNumber = 2; break;
                        case "Three": valueNumber = 3; break;
                        case "Four": valueNumber = 4; break;
                        case "Five": valueNumber = 5; break;
                        case "Six": valueNumber = 6; break;
                        case "Seven": valueNumber = 7; break;
                        case "Eight": valueNumber = 8; break;
                        case "Nine": valueNumber = 9; break;
                        case "Ten": valueNumber = 10; break;
                        case "Jack": valueNumber = 11; break;
                        case "Queen": valueNumber = 12; break;
                        case "King": valueNumber = 13; break;
                        case "One": valueNumber = 14; break;
                        case "小王": valueNumber = 15; break;
                        case "大王": valueNumber = 16; break;
                        case "特": valueNumber = 17; break;
                    }
                }
            }
            return valueNumber;
        }

        /// <summary>
        /// 获取颜色数字
        /// </summary>
        /// <param name="cardColor"></param>
        /// <returns></returns>
        private int GetColorNumberByCardColor(string cardColor)
        {
            int colorNumber = 0;
            switch (cardColor)
            {
                case "Heart": colorNumber = 1; break;
                case "Spade": colorNumber = 2; break;
                case "Square": colorNumber = 3; break;
                case "Club": colorNumber = 4; break;
            }

            return colorNumber;
        }

        /// <summary>
        /// 根据卡牌数据获取卡牌值
        /// </summary>
        /// <param name="cardData"></param>
        /// <returns></returns>
        private string GetCardValueByCardData(string cardData)
        {
            string tmpCardValue = string.Empty;
            foreach (string cardItem in this.cardValues)
            {
                if (cardData.Contains(cardItem))
                {
                    switch (cardItem)
                    {
                        case "Two": tmpCardValue = cardItem; break;
                        case "Three": tmpCardValue = cardItem; break;
                        case "Four": tmpCardValue = cardItem; break;
                        case "Five": tmpCardValue = cardItem; break;
                        case "Six": tmpCardValue = cardItem; break;
                        case "Seven": tmpCardValue = cardItem; break;
                        case "Eight": tmpCardValue = cardItem; break;
                        case "Nine": tmpCardValue = cardItem; break;
                        case "Ten": tmpCardValue = cardItem; break;
                        case "Jack": tmpCardValue = cardItem; break;
                        case "Queen": tmpCardValue = cardItem; break;
                        case "King": tmpCardValue = cardItem; break;
                        case "One": tmpCardValue = cardItem; break;
                        case "小王": tmpCardValue = cardItem; break;
                        case "大王": tmpCardValue = cardItem; break;
                        case "特": tmpCardValue = cardItem; break;
                    }
                }
            }
            return tmpCardValue;
        }
        /// <summary>
        /// 根据卡牌数据获取卡牌颜色
        /// </summary>
        /// <param name="cardData"></param>
        /// <returns></returns>
        private string GetCardColorByCardData(string cardData)
        {
            string tmpCardColor = string.Empty;
            foreach (string cardItem in this.cardColors)
            {
                if (cardData.Contains(cardItem))
                {
                    switch (cardItem)
                    {
                        case "Heart": tmpCardColor = cardItem; break;
                        case "Spade": tmpCardColor = cardItem; break;
                        case "Square": tmpCardColor = cardItem; break;
                        case "Club": tmpCardColor = cardItem; break;
                        case "Other1": tmpCardColor = cardItem; break;
                        case "Other2": tmpCardColor = cardItem; break;
                        case "Other3": tmpCardColor = cardItem; break;
                    }
                }
            }
            return tmpCardColor;
        }
        #endregion

        #endregion

        #region 排序玩家的牌
        /// <summary>
        /// 排序扑克牌
        /// </summary>
        /// <param name="pokerValueNumberDict"></param>
        /// <returns></returns>
        private List<string> SortPokerCard(Dictionary<int, List<int>> pokerValueNumberDict)
        {
            List<string> resPokerValues = new List<string>();
            try
            {
                string pokerValue = string.Empty;
                foreach (KeyValuePair<int, List<int>> pokerValueItem in pokerValueNumberDict)
                {
                    int pokerValueColor = pokerValueItem.Key;//扑克牌颜色
                    List<int> pokerValueNumbers = pokerValueItem.Value;
                    pokerValueNumbers.Sort(this.pokerProcess);
                    for (int pokerValueNumberIndex = 0; pokerValueNumberIndex < pokerValueNumbers.Count; pokerValueNumberIndex++)
                    {
                        int pokerValueNumber = pokerValueNumbers[pokerValueNumberIndex];
                        if (pokerValueNumber == 15)//处理大王 和 小王扑克牌
                            pokerValue = this.pokerProcess.SetPokerValueByPokerValueNumber(pokerValueColor, pokerValueNumber, 0);
                        else if (pokerValueNumber == 16)
                            pokerValue = this.pokerProcess.SetPokerValueByPokerValueNumber(pokerValueColor, pokerValueNumber, 0);
                        else if (pokerValueNumber == 17)
                            pokerValue = this.pokerProcess.SetPokerValueByPokerValueNumber(pokerValueColor, pokerValueNumber, 0);
                        else//处理其它扑克牌
                            pokerValue = this.pokerProcess.SetPokerValueByPokerValueNumber(pokerValueColor, pokerValueNumber, 1);
                        // Console.WriteLine("颜色： " + pokerValueColor + "值： " + pokerValueNumber + "合成的值： " + pokerValue);
                        resPokerValues.Add(pokerValue);

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.StackTrace);
            }
            return resPokerValues;
        }
        #endregion

        #region 玩家管牌
        //处理玩家管牌
        /// <summary>
        /// 玩家管牌
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        /// <param name="cards"></param>
        public void TubeCard(ClientPeer clientPeer, int roomId, params string[] cards)
        {
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);
            if (roomInfo != null)
            {
                Dictionary<UserInfo, List<string>> lastUserPlayCards = this.roomCurrentPlayCardDict[roomInfo];
                foreach (KeyValuePair<UserInfo, List<string>> userItem in lastUserPlayCards)
                {
                    if (userItem.Key.ClientUserSocket == clientPeer.ClientSocket)//玩家自己管自己(不能处理管,因为违反游戏的逻辑性)
                    {
                        break;//不做任何处理 直接跳出循环即可
                    }
                    else//玩家管别人(正常管即可)
                    {
                        string tmpLastUserPlayCard = userItem.Value[0];//第0个索引的元素基本上就是当前玩家所要管的牌
                        string tmpTubaCard = cards[0];//第0个索引就是当前玩家出的牌   
                        string[] playCardSplits = tmpLastUserPlayCard.Split(',');//解析出牌玩家的牌
                        string[] tubaCardSplits = tmpTubaCard.Split(',');//解析管牌玩家的牌
                        if (playCardSplits.Length == tubaCardSplits.Length)//首先必须确保两个玩家牌的个数保持一致
                        {
                            //Todo:需要对玩家管得牌 和 被管得牌做一个排序功能 也就是需要一套卡牌之间的比较明细的方案
                            //也就是需要做一个牌与牌之间比较大小的需求
                            //还得判断玩家管牌的时候 是否是连对管连对        

                        }
                        else
                        {
                            //如果不一致可能是特殊情况,特殊情况特殊处理即可(暂时先不用处理)
                            break;
                        }
                    }
                    break;//因为当前出牌的玩家只可能有一个,所以这里使用break 关键字.
                }
            }
        }

        #endregion

        #region 玩家获胜
        //处理玩家获胜
        /// <summary>
        /// 根据房间号处理玩家获胜的逻辑
        /// </summary>
        /// <param name="clientPeer">获胜的玩家连接对象</param>
        /// <param name="roomId">哪个房间获胜的</param>
        public void UserWin(ClientPeer clientPeer, int roomId)
        {

        }
        //处理送给同阵营玩家的顺风车
        /// <summary>
        /// /处理一个阵营某一个玩家获胜后送车的逻辑
        /// </summary>
        /// <param name="clientPeer">获胜的玩家对象</param>
        /// <param name="roomId">在哪个房间获胜的</param>
        public void GiveCar(ClientPeer clientPeer, int roomId)
        {

        }
        #endregion

        #region 玩家阵营获胜后升级
        //处理玩家获胜后升级 
        /// <summary>
        /// 处理玩家获胜后牌面升级的逻辑
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="campIndex"></param>
        /// <param name="roomId"></param>
        public void LevelUp(ClientPeer clientPeer, int campIndex, int roomId)
        {

        }
        #endregion

        #region 玩家凉水
        //处理玩家凉水
        /// <summary>
        /// 处理玩家凉水的逻辑
        /// </summary>
        /// <param name="clientPeer">哪个客户端对象触发的凉水事件</param>
        /// <param name="roomId">哪个房间内发生了凉水事件</param>
        /// <param name="camps">哪些阵营之间产生了凉水效应</param>
        public void Coldwater(ClientPeer clientPeer, int roomId, params int[] camps)
        {

        }
        #endregion

        #region 玩家完胜
        //处理玩家完胜 
        /// <summary>
        /// 处理一方阵营完胜的逻辑
        /// </summary>
        /// <param name="roomId">哪个房间发生了完胜事件</param>
        /// <param name="campIndex">哪方阵营完胜了</param>
        public void FullWin(int roomId, int campIndex)
        {

        }
        #endregion

        #region 计算玩家完胜后赢得钱数
        //计算赢得钱数
        /// <summary>
        /// 处理计算一方阵营完胜后所获得的钱数
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="campIndex"></param>
        public void CalcMoney(int roomId, int campIndex)
        {

        }
        #endregion
    }
}

/// <summary>
/// 扑克处理类
/// </summary>
public class PokerProcess : IComparer<int>
{
    /// <summary>
    /// 设置扑克牌对应的数字
    /// </summary>
    /// <param name="sourceColorNumber">源颜色数字</param>
    /// <param name="sourceValue">源值</param>
    /// <param name="targetCardColorNumber">目标卡牌颜色值</param>
    /// <param name="targetCardValue">目标卡牌值</param>
    public void SetPokerValueNumberByPoker(int sourceColorNumber, string sourceValue, out int targetCardColorNumber, out int targetCardValue)
    {
        int pokerValueNumber = 0;
        switch (sourceValue)
        {
            case "Two":
                pokerValueNumber = 2;
                break;
            case "Three":
                pokerValueNumber = 3;
                break;
            case "Four":
                pokerValueNumber = 4;
                break;
            case "Five":
                pokerValueNumber = 5;
                break;
            case "Six":
                pokerValueNumber = 6;
                break;
            case "Seven":
                pokerValueNumber = 7;
                break;
            case "Eight":
                pokerValueNumber = 8;
                break;
            case "Nine":
                pokerValueNumber = 9;
                break;
            case "Ten":
                pokerValueNumber = 10;
                break;
            case "Jack":
                pokerValueNumber = 11;
                break;
            case "Queen":
                pokerValueNumber = 12;
                break;
            case "King":
                pokerValueNumber = 13;
                break;
            case "One":
                pokerValueNumber = 14;
                break;
            case "小王":
                pokerValueNumber = 15;
                break;
            case "大王":
                pokerValueNumber = 16;
                break;
            case "特":
                pokerValueNumber = 17;
                break;
        }
        targetCardColorNumber = sourceColorNumber;
        targetCardValue = pokerValueNumber;
    }

    /// <summary>
    /// 根据扑克牌数字设置扑克牌的值
    /// </summary>
    /// <param name="pokerColorNumber"></param>
    /// <param name="pokerValueNumber"></param>
    /// <returns></returns>
    public string SetPokerValueByPokerValueNumber(int pokerColorNumber, int pokerValueNumber, int setCode)
    {
        //    this.cardColors = new string[] { "Heart", "Spade", "Square", "Club" };
        string pokerColor = string.Empty;//存储要返回的扑克牌颜色的值
        string pokerValue = string.Empty;//存储要返回的扑克牌的值
        if (setCode != 0)
        {
            switch (pokerColorNumber)//判断扑克颜色数字
            {
                case 1: pokerColor = "Heart"; break;//设置扑克牌颜色为 红桃
                case 2: pokerColor = "Spade"; break;//设置扑克牌颜色为 黑桃
                case 3: pokerColor = "Square"; break;//设置扑克牌颜色为 梅花
                case 4: pokerColor = "Club"; break;//设置扑克牌颜色为 方块
            }
            switch (pokerValueNumber)
            {
                case 2: pokerValue = "Two"; break;//2
                case 3: pokerValue = "Three"; break;//3
                case 4: pokerValue = "Four"; break;//4
                case 5: pokerValue = "Five"; break;//5
                case 6: pokerValue = "Six"; break;//6
                case 7: pokerValue = "Seven"; break;//7
                case 8: pokerValue = "Eight"; break;//8
                case 9: pokerValue = "Nine"; break;//9
                case 10: pokerValue = "Ten"; break;//10
                case 11: pokerValue = "Jack"; break;//J
                case 12: pokerValue = "Queen"; break;//Q
                case 13: pokerValue = "King"; break;//K
                case 14: pokerValue = "One"; break;//A
            }
        }
        else
        {
            switch (pokerColorNumber)//判断扑克颜色数字
            {
                case 5: pokerColor = "Other1"; break;//设置扑克牌颜色为 小王
                case 6: pokerColor = "Other2"; break;//设置扑克牌颜色为 大王
                case 7: pokerColor = "Other3"; break;//设置扑克牌颜色为 特
            }
            switch (pokerValueNumber)
            {
                case 15: pokerValue = "小王"; break;//小王
                case 16: pokerValue = "大王"; break;//大王
                case 17: pokerValue = "特"; break;//特
            }
        }
        return pokerColor + "," + pokerValue;
    }

    /// <summary>
    /// 比较两个牌的大小
    /// </summary>
    /// <param name="lastCardValue"></param>
    /// <param name="nextCardValue"></param>
    /// <returns></returns>
    public int Compare(int lastCardValue, int nextCardValue)
    {
        if (lastCardValue > nextCardValue)
            return lastCardValue;
        else if (lastCardValue == nextCardValue)
            return 0;
        else if (lastCardValue < nextCardValue)
            return nextCardValue;
        else
            return -1;
    }
}
