using System;

namespace Dlzyff.BoardGameServer.DataCache.Services
{
    /// <summary>
    /// 基本扑克类业务数据缓存类
    /// </summary>
    public abstract class PokerServiceCache : BaseServiceCache
    {
        /// <summary>
        /// 存储扑克牌花色
        /// </summary>
        protected string[] cardColors = null;

        /// <summary>
        /// 存储扑克牌的值
        /// </summary>
        protected string[] cardValues = null;

        public int XiaowangCount = 0;
        public int DawangCount = 0;
        public int TeCount = 0;

        public override void InitCardsData()
        {
            //扑克牌颜色数组的初始值: 红桃,黑桃,梅花,方片
            //根据不同的类型来初始化扑克牌颜色数组
            if (this is FivebombsWithSixbombsServiceCache)
                //this.cardColors = new string[] { "Heart", "Spade", "Square", "Club", "Other1", "Other2" };
                this.cardColors = new string[] { "Heart", "Spade", "Square", "Club" };
            else if (this is PasseServiceCache)
                this.cardColors = new string[] { "Heart", "Spade", "Square", "Club" };
        }

        /// <summary>
        /// 重置扑克牌数据
        /// </summary>
        public override void ResetCards()
        {
            //生成所有扑克牌
            int cardCount = 0;//取得卡牌的总个数
            if (this is FivebombsWithSixbombsServiceCache)
                cardCount = 54 * 3;
            else if (this is PasseServiceCache)
                cardCount = 24;
            for (int cardColorIndex = 0; cardColorIndex < this.cardColors.Length; cardColorIndex++)
            {
                for (int cardValueIndex = 0; cardValueIndex < this.cardValues.Length; cardValueIndex++)
                {
                    string tmpCardColor = this.cardColors[cardColorIndex];
                    string tmpCardValue = this.cardValues[cardValueIndex];
                    string tmpValue = string.Empty;
                    if (this is FivebombsWithSixbombsServiceCache)
                    {
                        #region 测试代码
                        //if (tmpCardColor != "Other1" && tmpCardColor != "Other2" && tmpCardColor != "Other3")
                        //{
                        //    switch (tmpCardValue)
                        //    {
                        //        case "小王":
                        //            if (xiaowang < this.XiaowangCount + 1)
                        //            {
                        //                tmpValue = "Other1" + tmpCardValue;
                        //                ++xiaowang;
                        //            }
                        //            break;
                        //        case "大王":
                        //            if (dawang < this.DawangCount + 1)
                        //            {
                        //                tmpValue = "Other2" + tmpCardValue;
                        //                ++dawang;
                        //            }
                        //            break;
                        //        case "特":
                        //            if (te < this.TeCount + 1)
                        //            {
                        //                tmpValue = "Other3" + tmpCardValue;
                        //                ++te;
                        //            }
                        //            break;
                        //    }
                        //}
                        //switch (tmpCardColor)
                        //{
                        //    case "Heart":
                        //        if (tmpCardValue != "小王" && tmpCardValue != "大王" && tmpCardValue != "特")
                        //            tmpValue = tmpCardColor + tmpCardValue;
                        //        break;
                        //    case "Spade":
                        //        if (tmpCardValue != "小王" && tmpCardValue != "大王" && tmpCardValue != "特")
                        //            tmpValue = tmpCardColor + tmpCardValue;
                        //        break;
                        //    case "Square":
                        //        if (tmpCardValue != "小王" && tmpCardValue != "大王" && tmpCardValue != "特")
                        //            tmpValue = tmpCardColor + tmpCardValue;
                        //        break;
                        //    case "Club":
                        //        if (tmpCardValue != "小王" && tmpCardValue != "大王" && tmpCardValue != "特")
                        //            tmpValue = tmpCardColor + tmpCardValue;
                        //        break;
                        //} 
                        #endregion
                        if (tmpCardColor != "Other1" && tmpCardColor != "Other2" && tmpCardColor != "Other3")
                            tmpValue = tmpCardColor + tmpCardValue;
                    }
                    else if (this is PasseServiceCache)
                    {
                        if (tmpCardColor != "Other1" && tmpCardColor != "Other2" && tmpCardColor != "Other3")
                            tmpValue = tmpCardColor + tmpCardValue;
                    }
                    this.allCards.Add(tmpValue);
                }
            }
            if (this is FivebombsWithSixbombsServiceCache)
            {
                for (int i = 0; i < 3; i++)
                {
                    this.allCards.Add("Other1小王");
                    this.allCards.Add("Other2大王");
                }
            }
            //打乱扑克牌
            for (int cardIndex = 0; cardIndex < cardCount; cardIndex++)//根据卡牌总个数进行循环遍历处理
            {
                int ranIndex = this.ranCardIndex.Next(0, this.allCards.Count);//从所有卡牌中随机取出一个卡牌的索引值
                this.resCards.Add(this.allCards[ranIndex]);//根据取出的索引值获取对应的数据 添加到存放打乱卡牌的集合中
                this.allCards.RemoveAt(ranIndex);//取出添加完毕 将数据删除
            }
            Console.WriteLine("洗完牌后的卡牌个数：" + this.resCards.Count.ToString());
        }

    }
}
