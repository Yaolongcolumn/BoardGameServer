namespace Dlzyff.BoardGame.Protocol.Codes
{
    /// <summary>
    /// 房间操作码
    /// </summary>
    public enum RoomCode
    {
        /// <summary>
        ///     获取房间列表请求码
        /// </summary>
        GetRoomList_Request,

        /// <summary>
        ///     获取房间列表响应码
        /// </summary>
        GetRoomList_Response,

        /// <summary>
        ///     创建房间请求码
        /// </summary>
        CreateRoom_Request,

        /// <summary>
        ///     创建房间响应码
        /// </summary>
        CreateRoom_Response,

        /// <summary>
        ///     加入房间请求码
        /// </summary>
        JoinRoom_Request,

        /// <summary>
        ///     加入房间响应码
        /// </summary>
        JoinRoom_Response,

        /// <summary>
        ///     玩家准备的请求码
        /// </summary>
        Userready_Request,

        /// <summary>
        ///     玩家准备的响应码
        /// </summary>
        Userready_Response,

        /// <summary>
        ///    玩家取消准备的请求码
        /// </summary>
        UserCancelready_Request,

        /// <summary>
        ///     玩家取消准备的响应码
        /// </summary>
        UserCancelready_Response,

        /// <summary>
        ///  开始游戏的请求码
        /// </summary>
        Startgame_Request,

        /// <summary>
        ///     房间聊天请求码
        /// </summary>
        RoomChat_Request,

        /// <summary>
        ///    离开房间请求码
        /// </summary>
        LeaveRoom_Request,

        /// <summary>
        ///     解散房间请求码
        /// </summary>
        DisbanadRoom_Request,

        /// <summary>
        ///     离开房间广播响应码
        /// </summary>
        LeaveRoom_BroadcastResponse,

        /// <summary>
        ///     开始游戏广播响应码
        /// </summary>
        StartGame_BroadcastResponse,

        /// <summary>
        ///     房间聊天广播响应码
        /// </summary>
        RoomChat_BroadcastResponse,

        /// <summary>
        ///     房间解散广播响应码
        /// </summary>
        DisbanadRoom_BroadcastResponse
    }
}
