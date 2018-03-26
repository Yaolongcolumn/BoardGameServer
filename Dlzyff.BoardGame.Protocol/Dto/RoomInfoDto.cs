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
        private int _id;
        private int _enterCode;
        private string _name;
        private int _personNumber;
        private List<UserInfoDto> _users = new List<UserInfoDto>();

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
    }
}
