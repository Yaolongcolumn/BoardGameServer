using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGameServer.Model;
using System;
using System.Collections.Generic;

namespace Dlzyff.BoardGameServer.DataCache.Services
{
    /// <summary>
    /// 番种类别
    /// </summary>
    public enum GuttaseedType
    {
        四方大发,
        /// <summary>
        /// [庄家起牌即和,任何胡牌型均算,计分168分]
        /// </summary>
        天和,
        /// <summary>
        /// [庄家出第一张牌后,下家自摸即是地和,牌型不限,计分158分]
        /// </summary>
        地和,
        /// <summary>
        /// [第一圈内胡牌,牌型不限,计分108分]
        /// </summary>
        人和,
        /// <summary>
        /// [由4副风刻（杠）组成的和牌。不计圈风刻、门风刻、三风刻、碰碰和,计分88分]
        /// </summary>
        大四喜,
        /// <summary>
        /// [和牌中，有中发白3副刻子。不计箭刻,计分88分]
        /// </summary>
        大三元,
        /// <summary>
        /// [由23468条及发字中的任何牌组成的顺子、刻子、将的和牌。不计混一色。如无“发”字组成的和牌，可计清一色  计分 88分]
        /// </summary>
        绿一色,
        /// <summary>
        /// [  计分 88分]
        /// </summary>
        九莲宝灯,
        /// <summary>
        /// [  计分 88分]
        /// </summary>
        四杠,
        /// <summary>
        /// [  计分 88分]
        /// </summary>
        连七对,
        /// <summary>
        /// [  计分 88分]
        /// </summary>
        十三幺,
        /// <summary>
        /// [  计分 88分]
        /// </summary>
        混杠,
        /// <summary>
        ///[  计分 64分] 
        /// </summary>
        清幺九,
        /// <summary>
        /// [  计分 64分]
        /// </summary>
        小四喜,
        /// <summary>
        /// [  计分 64分]
        /// </summary>
        小三元,
        /// <summary>
        /// [  计分 64分]
        /// </summary>
        字一色,
        /// <summary>
        /// [  计分 64分]
        /// </summary>
        四暗刻,//四暗杠
        /// <summary>
        /// [  计分 64分]
        /// </summary>
        一色双龙会,
        /// <summary>
        /// [  计分 48分]
        /// </summary>
        一色四同顺,
        /// <summary>
        /// [  计分 48分]
        /// </summary>
        一色四节高,
        /// <summary>
        /// [  计分 32分]
        /// </summary>
        一色四步高,
        /// <summary>
        /// [  计分 32分]
        /// </summary>
        三杠,
        /// <summary>
        /// [  计分 32分]
        /// </summary>
        混幺九,
        /// <summary>
        /// [  计分 24分]
        /// </summary>
        七对,
        /// <summary>
        /// [  计分 24分]
        /// </summary>
        七星不靠,
        /// <summary>
        /// [  计分 24分]
        /// </summary>
        全双刻,
        /// <summary>
        /// [  计分 24分]
        /// </summary>
        清一色,
        /// <summary>
        /// [  计分 24分]
        /// </summary>
        一色三同顺,
        /// <summary>
        /// [  计分 24分]
        /// </summary>
        一色三节高,
        /// <summary>
        /// [  计分 24分]
        /// </summary>
        全大,
        /// <summary>
        /// [  计分 24分]
        /// </summary>
        全中,
        /// <summary>
        /// [  计分 24分]
        /// </summary>
        全小,
        /// <summary>
        /// [  计分 16分]
        /// </summary>
        清龙,
        /// <summary>
        /// [  计分 16分]
        /// </summary>
        三色双龙会,
        /// <summary>
        /// [  计分 16分]
        /// </summary>
        一色三步高,
        /// <summary>
        /// [  计分 16分]
        /// </summary>
        全带五,
        /// <summary>
        /// [  计分 16分]
        /// </summary>
        三同刻,//三个序数相同的刻子(杠)
        /// <summary>
        /// [  计分 16分]
        /// </summary>
        三暗刻,//三个暗刻(杠)
        /// <summary>
        /// [  计分 12分]
        /// </summary>
        全不靠,
        /// <summary>
        /// [  计分 12分]
        /// </summary>
        组合龙,
        /// <summary>
        /// [  计分 12分]
        /// </summary>
        大于五,
        /// <summary>
        /// [  计分 12分]
        /// </summary>
        小于五,
        /// <summary>
        /// [  计分 12分]
        /// </summary>
        三风刻,
        /// <summary>
        /// [  计分 8分]
        /// </summary>
        花龙,
        /// <summary>
        /// [  计分 8分]
        /// </summary>
        推不倒,
        /// <summary>
        /// [  计分 8分]
        /// </summary>
        三色三同顺,
        /// <summary>
        /// [  计分 8分]
        /// </summary>
        三色三节高,
        /// <summary>
        /// [  计分 8分]
        /// </summary>
        无番和,
        /// <summary>
        /// [  计分 8分]
        /// </summary>
        妙手回春,
        /// <summary>
        /// [  计分 8分]
        /// </summary>
        海底捞月,
        /// <summary>
        /// [  计分 8分]
        /// </summary>
        杠上开花,
        /// <summary>
        /// [  计分 8分]
        /// </summary>
        抢杠和,
        /// <summary>
        /// [  计分 8分]
        /// </summary>
        杠杠和,
        /// <summary>
        /// [  计分 6分]
        /// </summary>
        碰碰和,
        /// <summary>
        /// [  计分 6分]
        /// </summary>
        混一色,
        /// <summary>
        /// [  计分 6分]
        /// </summary>
        三色三步高,
        /// <summary>
        /// [  计分 6分]
        /// </summary>
        五门齐,
        /// <summary>
        /// [  计分 6分]
        /// </summary>
        全求人,
        /// <summary>
        /// [  计分 6分]
        /// </summary>
        双暗杠,//两个暗杠
        /// <summary>
        /// [  计分 6分]
        /// </summary>
        双箭刻,//2副杠
        /// <summary>
        /// [  计分 6分]
        /// </summary>
        混四节,
        /// <summary>
        /// [  计分 4分]
        /// </summary>
        全带幺,
        /// <summary>
        /// [  计分 4分]
        /// </summary>
        不求人,
        /// <summary>
        /// [  计分 4分]
        /// </summary>
        双明杠,
        /// <summary>
        /// [  计分 4分]
        /// </summary>
        和绝张,
        /// <summary>
        /// [  计分 4分]
        /// </summary>
        混四步,
        /// <summary>
        /// [  计分 4分]
        /// </summary>
        混三节,
        /// <summary>
        /// [  计分 2分]
        /// </summary>
        箭刻,
        /// <summary>
        /// [  计分 2分]
        /// </summary>
        圈风刻,
        /// <summary>
        /// [  计分 2分]
        /// </summary>
        门风刻,
        /// <summary>
        /// [  计分 2分]
        /// </summary>
        门前清,
        /// <summary>
        /// [  计分 2分]
        /// </summary>
        平和,
        /// <summary>
        /// [  计分 2分]
        /// </summary>
        四归一,
        /// <summary>
        /// [  计分 2分]
        /// </summary>
        双同刻,
        /// <summary>
        /// [  计分 2分]
        /// </summary>
        双暗刻,
        /// <summary>
        /// [  计分 2分]
        /// </summary>
        暗杠,
        /// <summary>
        /// [  计分 2分]
        /// </summary>
        断幺九,
        /// <summary>
        /// [  计分 2分]
        /// </summary>
        混三步,
        /// <summary>
        /// [  计分 1分]
        /// </summary>
        一般高,
        /// <summary>
        /// [  计分 1分]
        /// </summary>
        喜相逢,
        /// <summary>
        /// [  计分 1分]
        /// </summary>
        连六,
        /// <summary>
        /// [  计分 1分]
        /// </summary>
        老少副,
        /// <summary>
        /// [  计分 1分]
        /// </summary>
        幺九刻,
        /// <summary>
        /// [  计分 1分]
        /// </summary>
        明杠,
        /// <summary>
        /// [  计分 1分]
        /// </summary>
        缺一门,
        /// <summary>
        /// [  计分 1分]
        /// </summary>
        无字,
        /// <summary>
        /// [  计分 1分]
        /// </summary>
        边张,
        /// <summary>
        /// [和2张牌之间的牌。4556和5也为坎张，手中有45567和6不算坎张  计分 1分]
        /// </summary>
        坎张,
        /// <summary>
        /// [钓单张牌作将成和  计分 1分]
        /// </summary>
        单钓将,
        /// <summary>
        /// [自己抓进牌成和牌  计分 1分]
        /// </summary>
        自摸,
        /// <summary>
        /// [和牌中,由序数牌的258做将牌  计分 1分]
        /// </summary>
        二五八将,
        /// <summary>
        /// [和牌中,由序数牌的19做将牌  计分 1分]
        /// </summary>
        幺九头
    }

    /// <summary>
    /// 麻将业务缓存类
    /// </summary>
    public class MahjongServiceCache : BaseServiceCache
    {
        #region 条 万 饼
        /// <summary>
        /// 存储条子、字符、圆饼(条、万、饼)这三个麻将牌种类的集合
        /// </summary>
        private List<string> suitTypes = null;
        /// <summary>
        /// 存储条子、字符、圆饼(条、万、饼)这三个麻将牌种类下的每个值的集合
        /// </summary>
        private List<string> suitNumbers = null;
        /// <summary>
        /// 存储条、万、饼的所有牌值的集合
        /// </summary>
        private List<string> suitValues = new List<string>();
        #endregion

        #region 东西南北风
        /// <summary>
        /// 存储风的麻将种类的集合
        /// </summary>
        private List<string> winds = null;
        /// <summary>
        /// 存储风的所有牌值的集合
        /// </summary>
        private List<string> windValues = new List<string>();
        #endregion

        #region 红中 发财 白板 小鸡
        /// <summary>
        /// 存储红中、发财、白板、小鸡的集合
        /// </summary>
        private List<string> dragons = null;
        /// <summary>
        /// 存储红中、发财、白板、小鸡牌的所有值的集合
        /// </summary>
        private List<string> dragonValues = new List<string>();
        #endregion

        #region 初始化和重置卡牌数据
        /// <summary>
        /// 初始化麻将牌数据
        /// </summary>
        public override void InitCardsData()
        {
            //初始化条、万、饼种类的集合：条,万,饼
            this.suitTypes = new List<string>() { "Bamboos", "Characters", "Circles" };

            //初始化条、万、饼种类值的集合：1,2,3,4,5,6,7,8,9
            this.suitNumbers = new List<string>() { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };

            //初始化风的麻将种类的集合：东,西,南,北
            this.winds = new List<string>() { "East", "West", "South", "North" };

            //初始化红中、发财、白板、小鸡的集合：红中,发财,白板,小鸡
            this.dragons = new List<string>() { "Red", "Green", "White", "Chicken" };

            //清空集合
            this.suitValues.Clear();
            this.windValues.Clear();
            this.dragonValues.Clear();
        }
        /// <summary>
        /// 重置麻将牌数据
        /// </summary>
        public override void ResetCards()
        {
            //条 -> 4 [颜色] * 9 [个数] = 36 张牌
            //万 -> 4 [颜色] * 9 [个数] = 36 张牌
            //饼 -> 4 [颜色] * 9 [个数] = 36 张牌
            //东、西、南、北：4 * 4 = 16 张牌
            //红中、发财、白板、小鸡：4 * 4 = 16 张牌
            //累计：( 36 * 3 ) + 32 =  张牌 

            #region 生成条 万 饼
            //生成 条、万、饼 牌的所有值
            for (int index = 0; index < 4; index++)
            {
                for (int suitTypeIndex = 0; suitTypeIndex < this.suitTypes.Count; suitTypeIndex++)
                {
                    for (int suitNumberIndex = 0; suitNumberIndex < this.suitNumbers.Count; suitNumberIndex++)
                        this.suitValues.Add(this.suitTypes[suitTypeIndex] + "" + this.suitNumbers[suitNumberIndex]);
                }
            }
            Console.WriteLine("条、万、饼个数：" + this.suitValues.Count.ToString());
            #endregion

            #region 生成风
            //生成风 牌的所有值
            for (int index = 0; index < 4; index++)
            {
                for (int windIndex = 0; windIndex < this.winds.Count; windIndex++)
                    this.windValues.Add(this.winds[windIndex]);
            }
            Console.WriteLine("风个数：" + this.windValues.Count.ToString());
            #endregion

            #region 生成红中 发财 白板 小鸡
            //生成其它 牌的所有值
            for (int index = 0; index < 4; index++)
            {
                for (int dragonIndex = 0; dragonIndex < this.dragons.Count; dragonIndex++)
                    this.dragonValues.Add(this.dragons[dragonIndex]);
            }
            Console.WriteLine("其它个数：" + this.dragonValues.Count.ToString());
            #endregion

            this.suitValues.ForEach(suitValue =>
            {
                this.allCards.Add(suitValue);
            });//循环遍历 [条、万、饼] 所有值的集合 添加到所有牌值的集合中
            this.windValues.ForEach(windValue =>
            {
                this.allCards.Add(windValue);
            });//循环遍历 [风] 所有值的集合 添加到所有牌值的集合中
            this.dragonValues.ForEach(dragonValue =>
            {
                this.allCards.Add(dragonValue);
            });//循环遍历 [红中、发财、白板、小鸡] 所有值的集合 添加到所有牌值的集合中

            int cardCount = this.suitValues.Count + this.windValues.Count + this.dragonValues.Count;//取得麻将牌值的总和
            for (int cardIndex = 0; cardIndex < cardCount; cardIndex++)
            {
                int index = this.ranCardIndex.Next(0, this.allCards.Count);
                string tmpCard = this.allCards[index];
                this.resCards.Add(tmpCard);
                this.allCards.RemoveAt(index);
            }//循环遍历所有麻将牌,并将麻将牌的顺序打乱,放置到一个新的集合中存储
        }
        #endregion

        #region 随机获取一张麻将牌
        /// <summary>
        /// 随机获取一张麻将牌
        /// </summary>
        /// <returns></returns>
        public override string GetRandomCard()
        {
            if (this.resCards.Count == 0)
                return "没有麻将牌可以抓取了~";
            else
                return this.resCards[this.ranCardIndex.Next(0, this.resCards.Count)];
        }
        #endregion

        #region 针对房间相关的操作
        /// <summary>
        /// 添加房间信息数据(其实就是将一个房间信息保存在当前业务缓存类中进行处理)
        /// </summary>
        /// <param name="roomInfo"></param>
        public override void AddRoomInfo(RoomInfo roomInfo)
        {

        }
        #endregion

        #region 玩家摸牌
        /// <summary>
        /// 摸牌
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        public void TouchCard(ClientPeer clientPeer, int roomId)
        {
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);
            if (roomInfo != null)
            {

            }
        }
        #endregion

        #region 玩家出牌
        /// <summary>
        /// 出牌
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        public void PlayCard(ClientPeer clientPeer, int roomId)
        {
            RoomInfo roomInfo = this.roomCache.GetRoomInfoByRoomId(roomId);
            if (roomInfo != null)
            {

            }
        }
        #endregion
    }
}
