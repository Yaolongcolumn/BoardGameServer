namespace Dlzyff.BoardGame.Protocol
{
    public enum ActionCode
    {
        None,

        /// <summary>
        ///     用户登录
        /// </summary>
        UserLogin,

        /// <summary>
        ///     获取房间列表
        /// </summary>
        GetRoomList,

        /// <summary>
        ///     创建房间
        /// </summary>
        CreateRoom,

        /// <summary>
        ///     加入房间
        /// </summary>
        JoinRoom,

        /// <summary>
        ///     开始游戏
        /// </summary>
        StartGame,

        /// <summary>
        ///     房间聊天
        /// </summary>
        RoomChat,

        /// <summary>
        ///     记录房间内的椅子索引
        /// </summary>
        RecordChairIndex,

        /// <summary>
        ///     洗牌
        /// </summary>
        ShuffleChessWithCards,

        /// <summary>
        ///     摸牌(发牌)
        /// </summary>
        TouchChessWithCards,

        /// <summary>
        ///     出牌(打牌)
        /// </summary>
        PlayChessWithCards
    }
}