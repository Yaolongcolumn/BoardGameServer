namespace Dlzyff.BoardGameServer.Model
{
    /// <summary>
    /// 账户数据实体模型
    /// </summary>
    public class AccountInfo
    {
        private int _id;
        private string _name;
        private string _password;
        /// <summary>
        /// 账户的唯一编号
        /// </summary>
        public int Id
        {
            get { return this._id; }
            set { this._id = value; }
        }
        /// <summary>
        /// 账户登录时的用户名
        /// </summary>
        public string Name
        {
            get { return this._name; }
            set { this._name = value; }
        }
        /// <summary>
        /// 账户登录时的密码
        /// </summary>
        public string Password
        {
            get { return this._password; }
            set { this._password = value; }
        }

        public AccountInfo() { }

        /// <summary>
        /// 带参的构造方法
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accountPassword"></param>
        public AccountInfo(string accountName, string accountPassword) {
            this.Name = accountName;
            this.Password = accountPassword;
        }
      
    }
}
