namespace Dlzyff.BoardGame.Protocol.Codes.GameCode
{
    /// <summary>
    /// 五轰六炸游戏码
    /// </summary>
    public enum FivebombsWithSixbombsGameCode
    {
        /// <summary>
        /// 摸牌的请求码
        /// </summary>
        TouchCard_Request,
        /// <summary>
        /// 摸牌的响应码
        /// </summary>
        TouchCard_Response,
        /// <summary>
        /// 出牌的请求码
        /// </summary>
        PlayCard_Request,
        /// <summary>
        /// 出牌的响应码
        /// </summary>
        PlayCard_Response,
        /// <summary>
        /// 出牌的广播响应码
        /// </summary>
        PlayCard_BroadcastResponse,
        /// <summary>
        /// 胜利的广播响应码
        /// </summary>
        Win_BroadcastResponse
    }
}
