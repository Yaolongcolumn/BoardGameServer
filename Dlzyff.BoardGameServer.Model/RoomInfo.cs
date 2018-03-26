using System.Collections.Generic;

namespace Dlzyff.BoardGameServer.Model
{
    /// <summary>
    /// 游戏房间实体类
    /// </summary>
    public class RoomInfo
    {
        private int _Id;
        private int _enterCode;
        private string _name;
        private int _ownerUserId;
        private int _personNumber;
        private List<UserInfo> _userInfos = new List<UserInfo>();
        /// <summary>
        /// 房间编号
        /// </summary>
        public int Id
        {
            get { return this._Id; }
            set { this._Id = value; }
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
        /// 房间的进入码(每一个房间创建时唯一的一个进入码)
        /// 只有创建房间的玩家才能看见(也就是相当于使用房卡创建的玩家才能看得见)
        /// </summary>
        public int EnterCode
        {
            get { return this._enterCode; }
            set { this._enterCode = value; }
        }
        /// <summary>
        /// 房主编号
        /// </summary>
        public int OwnerUserId
        {
            get { return this._ownerUserId; }
            set { this._ownerUserId = value; }
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
        /// 存储当前房间内的客户端对象
        /// </summary>
        public List<UserInfo> UserInfos
        {
            get { return this._userInfos; }
            set { this._userInfos = value; }
        }
    }
}
