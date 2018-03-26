namespace Dlzyff.BoardGame.Protocol.Codes
{
    /// <summary>
    /// 账户操作码
    /// </summary>
    public enum AccountCode
    {
        /// <summary>
        ///     注册请求码
        /// </summary>
        Register_Request,
        /// <summary>
        ///     注册响应码
        /// </summary>
        Register_Response,
        /// <summary>
        ///     登录请求码
        /// </summary>
        Login_Request,
        /// <summary>
        ///     登录响应码
        /// </summary>
        Login_Response
    }
}
