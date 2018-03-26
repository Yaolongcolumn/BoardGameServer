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
    }
}
