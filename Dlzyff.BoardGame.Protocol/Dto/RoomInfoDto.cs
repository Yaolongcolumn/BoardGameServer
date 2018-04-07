using System;
using System.Collections.Generic;

namespace Dlzyff.BoardGame.Protocol.Dto
{
    /// <summary>
    /// 房间信息数据传输类
    /// </summary>
    [Serializable]
    public class RoomInfoDto
    {
        /// <summary>
        /// 房间游戏业务类别
        /// </summary>
        public enum RoomGameServiceType
        {
            /// <summary>
            /// 帕斯业务
            /// </summary>
            PasseService = 0,
            /// <summary>
            /// 五轰六炸业务
            /// </summary>
            FivebombsWithSixbombsService = 1,
            /// <summary>
            /// 麻将业务
            /// </summary>
            MahjongService = 2
        }
        private int _id;
        private int _enterCode;
        private string _name;
        private int _personNumber;
        private List<UserInfoDto> _users = new List<UserInfoDto>();
        private RoomGameServiceType _serviceType;

        /// <summary>
        /// 房间编号
        /// </summary>
        public int Id
        {
            get { return this._id; }
            set { this._id = value; }
        }

        /// <summary>
        /// 房间进入码
        /// </summary>
        public int EnterCode
        {
            get { return this._enterCode; }
            set { this._enterCode = value; }
        }

        /// <summary>
        /// 房间名称
        /// </summary>
        public string Name
        {
            get { return this._name; }
            set { this._name = value; }
        }

        /// <summary>
        /// 房间人数
        /// </summary>
        public int PersonNumber
        {
            get { return this._personNumber; }
            set { this._personNumber = value; }
        }

        /// <summary>
        /// 房间玩家列表
        /// </summary>
        public List<UserInfoDto> Users
        {
            get { return this._users; }
            set { this._users = value; }
        }

        /// <summary>
        /// 房间游戏业务类别
        /// </summary>
        public RoomGameServiceType ServiceType
        {
            get
            {
                return this._serviceType;
            }
            set
            {
                this._serviceType = value;
            }
        }
    }
}
