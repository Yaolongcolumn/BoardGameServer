﻿using System;

namespace Dlzyff.BoardGame.Protocol.Dto
{
    /// <summary>
    /// 用户信息数据传输类
    /// </summary>
    [Serializable]
    public class UserInfoDto
    {
        private int _id;
        private string _userName;
        private int _winCount;
        private int _loseCount;
        private int _runCount;
        private int _level;
        private int _expValue;
        private int _money;

        private int _clientIndex;
        private int _roomId;

        /// <summary>
        /// 用户唯一编号
        /// </summary>
        public int Id
        {
            get { return this._id; }
            set { this._id = value; }
        }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName
        {
            get { return this._userName; }
            set { this._userName = value; }
        }

        /// <summary>
        /// 用户胜利场次
        /// </summary>
        public int WinCount
        {
            get { return this._winCount; }
            set { this._winCount = value; }
        }

        /// <summary>
        /// 用户失败场次
        /// </summary>
        public int LoseCount
        {
            get { return this._loseCount; }
            set { this._loseCount = value; }
        }

        /// <summary>
        /// 用户逃跑场次
        /// </summary>
        public int RunCount
        {
            get { return this._runCount; }
            set { this._runCount = value; }
        }

        /// <summary>
        /// 用户等级
        /// </summary>
        public int Level
        {
            get { return this._level; }
            set { this._level = value; }
        }

        /// <summary>
        /// 用户经验值
        /// </summary>
        public int ExpValue
        {
            get { return this._expValue; }
            set { this._expValue = value; }
        }

        /// <summary>
        /// 用户钱数
        /// </summary>
        public int Money
        {
            get { return this._money; }
            set { this._money = value; }
        }

        /// <summary>
        /// 客户端编号
        /// </summary>
        public int ClientIndex
        {
            get { return this._clientIndex; }
            set { this._clientIndex = value; }
        }

        /// <summary>
        /// 用户所在房间编号
        /// </summary>
        public int RoomId {
            get { return this._roomId; }
            set { this._roomId = value; }
        }

        public UserInfoDto()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userName"></param>
        /// <param name="accountId"></param>
        public UserInfoDto(int id, string userName)
        {
            this.Id = id;
            this.UserName = userName;
            this.WinCount = 0;
            this.LoseCount = 0;
            this.RunCount = 0;
            this.Level = 1;
            this.ExpValue = 0;
        }
    }
}
