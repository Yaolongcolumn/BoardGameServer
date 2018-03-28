namespace Dlzyff.BoardGameServer.Model
{
    /// <summary>
    /// 账户数据实体模型
    /// </summary>
    public class AccountInfo
    {
        private int _accountId;
        private string _accountUsername;
        private string _accountPassword;
        /// <summary>
        /// 账户的唯一编号
        /// </summary>
        public int AccountId
        {
            get { return this._accountId; }
            set { this._accountId = value; }
        }
        /// <summary>
        /// 账户登录时的用户名
        /// </summary>
        public string AccountUsername
        {
            get { return this._accountUsername; }
            set { this._accountUsername = value; }
        }
        /// <summary>
        /// 账户登录时的密码
        /// </summary>
        public string AccountPassword
        {
            get { return this._accountPassword; }
            set { this._accountPassword = value; }
        }

        public AccountInfo() { }

        /// <summary>
        /// 带参的构造方法
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accountPassword"></param>
        public AccountInfo(string accountName, string accountPassword) {
            this.AccountUsername = accountName;
            this.AccountPassword = accountPassword;
        }
      
    }
}
