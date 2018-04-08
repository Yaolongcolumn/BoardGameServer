using System.Collections.Generic;

namespace Dlzyff.BoardGameServer.Model
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
    /// <summary>
    /// 房间状态
    /// </summary>
    public enum RoomState
    {
        /// <summary>
        /// 等待
        /// </summary>
        Waiting = 0,
        /// <summary>
        /// 开始
        /// </summary>
        Starting = 1,
        /// <summary>
        /// 游戏
        /// </summary>
        Playing = 2,
        /// <summary>
        /// 结束
        /// </summary>
        Ending = 3
    }
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
        private RoomGameServiceType _serviceType;
        private RoomState _roomState;

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
        /// <summary>
        /// 业务类型
        /// </summary>
        public RoomGameServiceType ServiceType
        {
            get { return this._serviceType; }
            set { this._serviceType = value; }
        }
        /// <summary>
        /// 房间状态
        /// </summary>
        public RoomState RoomState
        {
            get { return this._roomState; }
            set { this._roomState = value; }
        }
    }
}
