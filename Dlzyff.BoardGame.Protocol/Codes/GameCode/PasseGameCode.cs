namespace Dlzyff.BoardGame.Protocol.Codes.GameCode
{
    /// <summary>
    /// 帕斯游戏码
    /// </summary>
    public enum PasseGameCode
    {
        /// <summary>
        /// 显示跟请求
        /// </summary>
        DispalyFollow_Request,
        /// <summary>
        /// 跟牌请求码
        /// </summary>
        Follow_Request,

        /// <summary>
        /// 跟牌响应码
        /// </summary>
        Follow_Response,

        /// <summary>
        /// 不跟牌请求码
        /// </summary>
        NoFollow_Request,

        /// <summary>
        /// 踢牌请求码
        /// </summary>
        Play_Request,

        /// <summary>
        /// 不踢牌请求码
        /// </summary>
        NoPlay_Request,

        /// <summary>
        /// 弃牌请求码
        /// </summary>
        Discard_Request,

        /// <summary>
        /// 下注请求码
        /// </summary>
        BottomPour_Request,

        /// <summary>
        /// 下注响应码
        /// </summary>
        BottomPour_Response,

        /// <summary>
        /// 下注响应码
        /// </summary>
        BottomPour_BroadcastResponse,

        /// <summary>
        /// 跟牌广播响应码
        /// </summary>
        Follow_BroadcastResponse,

        /// <summary>
        /// 踢牌广播响应吗
        /// </summary>
        Play_BroadcastResponse,

        /// <summary>
        /// 粘锅响应码
        /// </summary>
        StickyPot_BroadcastResponse,

        /// <summary>
        /// 帕斯游戏广播响应码
        /// </summary>
        PasseGame_BroadcastResponse,

        /// <summary>
        /// 显示最大分数广播响应码
        /// </summary>
        DisplayMaxScore_BroadcastResponse
    }
}
