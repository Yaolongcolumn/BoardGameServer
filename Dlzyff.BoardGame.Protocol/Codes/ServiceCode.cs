namespace Dlzyff.BoardGame.Protocol.Codes
{
    /// <summary>
    /// 游戏业务操作码
    /// </summary>
    public enum ServiceCode
    {
        None,
        /// <summary>
        /// 帕斯请求
        /// </summary>
        Passe_Request,
        /// <summary>
        /// 帕斯响应
        /// </summary>
        Passe_Response,
        /// <summary>
        /// 帕斯获取最大值响应码
        /// </summary>
        Passe_GetMaxScoreResponse,
        /// <summary>
        /// 跟牌响应码 
        /// </summary>
        Passe_FollowResponse,
        /// <summary>
        /// 帕斯下注响应码
        /// </summary>
        Passe_BottomPourResponse,
        /// <summary>
        /// 五轰六炸请求
        /// </summary>
        FivebombsWithSixbombs_Request,
        /// <summary>
        /// 五轰六炸响应
        /// </summary>
        FivebombsWithSixbombs_Response,
        /// <summary>
        /// 麻将请求
        /// </summary>
        Mahjong_Request,
        /// <summary>
        /// 麻将响应
        /// </summary>
        Mahjong_Response,
        /// <summary>
        /// 帕斯消息广播响应
        /// </summary>
        Passe_BroadcastResponse,
        /// <summary>
        /// 五轰六炸消息广播响应
        /// </summary>
        FivebombsWithSixbombs_BroadcastResponse
    }
}
