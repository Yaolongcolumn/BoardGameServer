using Dlzyff.BoardGame.BottomServer.Concurrents;
using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGame.BottomServer.Tools;
using Dlzyff.BoardGame.Protocol.Codes;
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
        /// 随机索引值
        /// </summary>
        private Random ranIndex = new Random();

        private PokerProcess pokerProcess = null;

        #region 初始化和重置卡牌数据
        /// <summary>
        /// 初始化卡牌数据
        /// </summary>
        public sealed override void InitCardsData()
        {
            base.InitCardsData();
            // 13 [卡牌值得个数] * 3 [卡牌的副数]
            List<string> tmpCardValues = new List<string>();//定义一个临时存储卡牌值的集合
            string[] cardValues = new string[] { "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King", "One" };//构建一副卡牌值的数组
            for (int i = 0; i < 3; i++)//外层循环遍历3次(这里3次是因为五轰六炸卡牌游戏为3副扑克牌)
            {
                for (int j = 0; j < cardValues.Length; j++)//内层循环遍历一副卡牌值的个数
                    tmpCardValues.Add(cardValues[j]);//每次遍历完成 添加到存储卡牌值的临时集合中
                tmpCardValues.Add("大王");
                tmpCardValues.Add("小王");
            }
            this.pokerProcess = new PokerProcess();
            this.cardValues = new string[tmpCardValues.Count];//重新构建存储卡牌值的集合数组
            for (int cardValueIndex = 0; cardValueIndex < tmpCardValues.Count; cardValueIndex++)
                this.cardValues[cardValueIndex] = tmpCardValues[cardValueIndex];
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
                Dictionary<UserInfo, List<string>> userCardDict = new Dictionary<UserInfo, List<string>>();
                foreach (UserInfo user in roomInfo.UserInfos)
                {
                    if (!userCardDict.ContainsKey(user))
                        userCardDict.Add(user, new List<string>());
                }
                if (!this.roomUserExistCardDict.ContainsKey(roomInfo))
                    this.roomUserExistCardDict.Add(roomInfo, userCardDict);
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
                    this.roomUserExistCardDict[tmpRoomInfo].Add(userinfoArray[userIndex], new List<string>());//将他们添加到数据字典中
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
            List<UserInfo> userInfos = roomInfo.UserInfos;//取出房间内的玩家数据列表
            int campIndex = 0;
            for (int userIndex = 0; userIndex < userInfos.Count; userIndex++)//遍历玩家列表
            {
                int index = this.ranIndex.Next(0, userInfos.Count);//取出一个随机索引
                UserInfo userInfo = userInfos[index];//取出随机索引对应的玩家
                if (userIndex < 2)//如果玩家小于两个人
                    campIndex = 1;//将玩家放置到阵营一中去
                else//如果玩家不小于两个人
                    campIndex = 2;//将玩家放置到阵营二中去
                switch (campIndex)
                {
                    case 1:
                        this.userCampDict[1].Add(userInfo);//将玩家添加至阵营一
                        break;
                    case 2:
                        this.userCampDict[2].Add(userInfo);//将玩家添加至阵营二
                        break;
                }
            }
            #region 通知服务端应用层显示玩家阵营信息
            LogMessage.Instance.SetLogMessage("阵营 [ " + 1 + " ] 的玩家个数有 [ " + this.userCampDict[1].Count + " ] 个.");
            foreach (UserInfo user in this.userCampDict[1])
            {
                LogMessage.Instance.SetLogMessage("阵营 [ " + 1 + " ] 的玩家: " + user.UserName);
            }
            LogMessage.Instance.SetLogMessage("阵营 [ " + 2 + " ] 的玩家个数有 [ " + this.userCampDict[2].Count + " ] 个.");
            foreach (UserInfo user in this.userCampDict[2])
            {
                LogMessage.Instance.SetLogMessage("阵营 [ " + 2 + " ] 的玩家: " + user.UserName);
            }
            #endregion
        }
        //随机分配玩家阵营 
        /// <summary>
        ///  根据房间号随机分配玩家阵营
        /// </summary>
        /// <param name="roomId"></param>
        public void RandomAssignUserCamp(int roomId)
        {

        }
        #endregion

        #region 设定默认要打的手牌
        /// <summary>
        /// 处理获取一张默认要打的手牌 
        /// </summary>
        /// <param name="defaultCardStr">默认卡牌字符串</param>
        /// <returns></returns>
        public string GetDefaultPlayCard(string defaultCardStr)
        {
            return this.resCards.Find(str => str.Contains(defaultCardStr));
        }
        #endregion

        #region 玩家摸牌
        //处理玩家摸牌 
        /// <summary>
        /// 摸牌(默认情况下,是一下全部分发每个玩家持有的所有手牌)
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        public void TouchCard(ClientPeer clientPeer, int roomId)
        {
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);//根据房间编号获取房间数据
            if (roomInfo != null)//检查获取到的房间是否为空
            {
                int touchCount = (13 * 3) / 3;//用于来限定玩家可以摸几次牌
                StringBuilder stringBuilder = new StringBuilder();//用于拼接卡牌数据
                string cardStr = string.Empty;//用于存储要发送给客户端对象的卡牌数据
                int clientIndex = 0;//用户存储客户端索引
                foreach (UserInfo user in roomInfo.UserInfos)
                {
                    if (user.ClientUserSocket == clientPeer.ClientSocket)//判断当前遍历到房间内玩家列表的玩家对象 是不是与 要摸牌的玩家的连接对象持有一份共同引用
                    {
                        for (int touchIndex = 0; touchIndex < touchCount; touchIndex++)//这儿做循环遍历 目的是根据玩家摸牌的次数  一下将所有牌都分发给玩家
                        {
                            string card = this.GetRandomCard();//随机取出一张卡牌数据
                            stringBuilder.Append(card + ",");
                            //Todo:玩家摸牌之后 需要将玩家摸到的牌存储下来
                            if (!this.roomUserExistCardDict[roomInfo].ContainsKey(user))
                                this.roomUserExistCardDict[roomInfo].Add(user, new List<string>() { card });
                            else
                                this.roomUserExistCardDict[roomInfo][user].Add(card);
                        }
                        clientIndex = user.ClientIndex;//设置存储客户端索引(方便客户端拿到卡牌数据之后进行更新显示的操作)
                        break;
                    }
                    else
                        continue;
                }
                if (stringBuilder.Length > 0)
                    cardStr = stringBuilder.ToString().Remove(stringBuilder.Length - 1, 1);
                //构造一条网络消息 发送给发起摸牌请求的客户端玩家对象
                clientPeer.OnSendMessage
                    (
                        new SocketMessage()
                        {
                            OperationCode = OperationCode.Service,
                            SubOperationCode = (int)ServiceCode.FivebombsWithSixbombs_Response,
                            DataValue = cardStr + "|" + clientIndex
                        }
                    );
            }
        }
        #endregion

        #region 排序玩家的牌
        private Dictionary<int, string> sortCardDict = new Dictionary<int, string>();
        /// <summary>
        /// 排序指定房间内指定玩家的牌
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        public void SortCard(UserInfo userInfo, int roomId)
        {
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);
            if (roomInfo != null)
            {
                List<string> cards = this.roomUserExistCardDict[roomInfo][userInfo];
                List<int> cardNumbers = new List<int>();
                //Todo:按照一定格式来进行排序
                foreach (string cardItem in cards)
                {
                    int cardNumber = this.pokerProcess.SetPokerValueNumberByPoker(cardItem);//根据卡牌值 设置对应的数据
                    sortCardDict.Add(cardNumber, cardItem);//将设置好的数据存储起来
                    cardNumbers.Add(cardNumber);//存储数据
                }
                cardNumbers.Sort(pokerProcess);//进行一个排序工作
                int count = 0;
                foreach (int cardNumber in cardNumbers)//循环遍历，看排序玩家所持有的卡牌，然后的结果。
                {
                    if (count == 5)
                    {
                        Console.WriteLine();
                        count = 0;
                    }
                    Console.Write(this.sortCardDict[cardNumber]);
                    count++;
                }
            }
        }
        #endregion

        #region 玩家出牌
        //处理玩家出牌 
        /// <summary>
        /// 出牌
        /// </summary>
        /// <param name="clientPeer">出牌的客户端连接对象</param>
        /// <param name="roomId">房间编号</param>
        /// <param name="removeCardArray">玩家出的牌(也就是要移除的牌,这个牌可能有多个,而不是就一个)</param>
        public void PlayCard(ClientPeer clientPeer, int roomId, params string[] removeCardArray)
        {
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);
            if (roomInfo != null)
            {
                //校验玩家是否出完牌了
                if (this.roomWinUserDict[roomInfo].ContainsKey(roomInfo.UserInfos.Find(user => user.ClientUserSocket == clientPeer.ClientSocket)))//这个玩家已经出完牌了
                {
                    SocketMessage msg = new SocketMessage()
                    {
                        OperationCode = OperationCode.Service,
                        SubOperationCode = (int)ServiceCode.FivebombsWithSixbombs_Response,
                        //玩家打出的手牌|打出手牌的玩家客户端索引值
                        DataValue = "你已经出完牌了,不能再进行出牌了~"
                    };
                    clientPeer.OnSendMessage(msg);//发送给指定客户端对象
                    return;//直接return 不继续执行余下的代码体了
                }
                StringBuilder stringBuilder = new StringBuilder();//用来方便的拼接发送的网络消息
                string resStr = string.Empty;//存储拼接后的字符串
                UserInfo userInfo = null;//存储出牌的玩家
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
                                    //移除手牌即可 
                                    //每次存在 只需要进行拼接即可 移除的手牌之间使用|进行分割处理即可
                                    stringBuilder.Append(tmpRemoveCard + ",");
                                    tmpCards.Remove(tmpRemoveCard);//移除手牌
                                }
                                else//不存在 就是表示玩家手中已经不持有这张要移除的手牌了
                                    continue;  //不做任何处理 
                            }
                            userInfo = user;//记录出牌的玩家对象
                        }
                        else//不大于0说明玩家手里已经没有手牌了(也就是不能出牌)
                        {
                            //提示玩家：你已经没有手牌了,不能进行出牌~
                            //构造网络消息
                            SocketMessage msg = new SocketMessage()
                            {
                                OperationCode = OperationCode.Service,
                                SubOperationCode = (int)ServiceCode.FivebombsWithSixbombs_Response,
                                //玩家打出的手牌|打出手牌的玩家客户端索引值
                                DataValue = "你已经没有手牌了,不能进行出牌~"
                            };
                            clientPeer.OnSendMessage(msg);//发送给指定客户端对象
                            if (!this.roomWinUserDict.ContainsKey(roomInfo))
                                this.roomWinUserDict.Add(roomInfo, new Dictionary<UserInfo, int>() { { userInfo, this.concurrentInteger.AddWithGet() } });
                            return;
                        }
                        break;
                    }
                    else
                        continue;//Todo:不做任何处理 
                }
                if (stringBuilder.ToString().Length > 0)
                    resStr = stringBuilder.ToString().Remove(stringBuilder.ToString().Length - 1, 1);
                //Todo:中间还有可能做一些其它操作.....
                //构建一个网络消息(业务模块/五轰六炸子模块 数据->玩家打出的手牌+,+该玩家的客户端索引值)
                SocketMessage message = new SocketMessage()
                {
                    OperationCode = OperationCode.Service,
                    SubOperationCode = (int)ServiceCode.FivebombsWithSixbombs_Response,
                    //玩家打出的手牌|打出手牌的玩家客户端索引值
                    DataValue = resStr + "|" + userInfo.ClientIndex
                };
                //记录当前出牌的玩家
                if (!this.roomCurrentPlayCardDict.ContainsKey(roomInfo))//如果不存在这个房间信息数据
                    this.roomCurrentPlayCardDict.Add(roomInfo, new Dictionary<UserInfo, List<string>>() { { userInfo, new List<string>() { resStr } } });//进行添加房间信息数据
                else//如果存在了
                {
                    if (this.roomCurrentPlayCardDict[roomInfo].ContainsKey(userInfo))//如果这个房间数据中的当前玩家的键已经存在 
                        this.roomCurrentPlayCardDict[roomInfo].Remove(userInfo);//则进行移除操作
                    else//如果不存在
                        this.roomCurrentPlayCardDict[roomInfo].Add(userInfo, new List<string>());//则进行添加操作
                    this.roomCurrentPlayCardDict[roomInfo][userInfo].Add(resStr);//给当前出牌的玩家保存出牌时的数据
                }
                //目前只是单方面 发送,往后做的情况下,可能要通过广播消息的方式,通知房间内的每一个玩家,告知他们哪个玩家打出了什么手牌
                //clientPeer.OnSendMessage(message);//给出牌的玩家发送消息
                this.roomCache.BroadcastMessageByRoomId(roomId, message); //给房间内的所有玩家推送消息 告诉哪个玩家出牌了 出的是什么牌
                                                                          // Todo:这儿需要将出牌的信息显示到服务端前端表现层 显示出来
                                                                          //Todo:玩家出牌后,需要找出比该出牌玩家出的手牌还牛逼的玩家中的手牌,因为出牌后紧接着就是需要处理管牌
                                                                          //这儿需要做的是,服务端检查当前游戏局内谁的手牌可以管,则提示该玩家是否进行管牌
            }
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
                            // this.SortCard(userItem.Key, roomId);
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
    /// 设置扑克牌的值
    /// </summary>
    public int SetPokerValueNumberByPoker(string poker)
    {
        int pokerValueNumber = 0;
        switch (poker)
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
            case "大王":
                pokerValueNumber = 15;
                break;
            case "小王":
                pokerValueNumber = 16;
                break;
        }
        return pokerValueNumber;
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
