using System;

namespace Dlzyff.BoardGame.Protocol.Dto
{
    /// <summary>
    /// 扑克信息数据传输类
    /// </summary>
    [Serializable]
    public class PokerInfoDto
    {
        private string _color;
        private string _value;
        private int _number;
        private int _ownerClientIndex;
        /// <summary>
        /// 扑克牌的颜色
        /// </summary>
        public string Color
        {
            get
            {
                return this._color;
            }
            set
            {
                this._color = value;
            }
        }
        /// <summary>
        /// 扑克牌的值
        /// </summary>
        public string Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
            }
        }
        /// <summary>
        /// 扑克牌的个数
        /// </summary>
        public int Number
        {
            get
            {
                return this._number;
            }
            set
            {
                this._number = value;
            }
        }
        /// <summary>
        /// 持有这张牌的客户端座位索引号
        /// </summary>
        public int OwnerClientIndex
        {
            get
            {
                return this._ownerClientIndex;
            }
            set
            {
                this._ownerClientIndex = value;
            }
        }
        public PokerInfoDto() { }
        public PokerInfoDto(string color, string value, int number)
        {
            this.Color = color;
            this.Value = value;
            this.Number = number;
        }
        /// <summary>
        /// 更改要传输的扑克牌数据信息
        /// </summary>
        /// <param name="color"></param>
        /// <param name="value"></param>
        /// <param name="number"></param>
        public void ChangePokerInfo(string color, string value, int number)
        {
            this.Color = color;
            this.Value = value;
            this.Number = number;
        }
    }
}
