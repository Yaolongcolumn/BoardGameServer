namespace Dlzyff.BoardGameServer.DataCache.Services
{
    /// <summary>
    /// 基本扑克类业务数据缓存类
    /// </summary>
    public abstract class PokerServiceCache : BaseServiceCache
    {
        /// <summary>
        /// 存储卡牌花色
        /// </summary>
        protected string[] cardColors = null;

        /// <summary>
        /// 存储卡牌的值
        /// </summary>
        protected string[] cardValues = null;

        public override void InitCardsData()
        {
            this.cardColors = new string[] { "Heart", "Spade", "Square", "Club" };
        }

        /// <summary>
        /// 重置卡牌牌数据
        /// </summary>
        public override void ResetCards()
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

    }
}
