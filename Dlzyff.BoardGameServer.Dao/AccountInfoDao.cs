using Dlzyff.BoardGameServer.Dao.Tools;
using Dlzyff.BoardGameServer.Model;

namespace Dlzyff.BoardGameServer.Dao
{
    /// <summary>
    /// 账户信息数据访问类(就是跟数据库中的账户表打交道的类)
    /// </summary>
    public class AccountInfoDao
    {
        /// <summary>
        /// 注册账户
        /// Todo:玩家使用第三方平台登录以后,取到登录后的用户信息以后,服务端重新为该用户注册一个属于当前应用的对应账户
        /// </summary>
        /// <param name="accountName">注册的账户名</param>
        /// <param name="accountPwd">注册的密码</param>
        public void RegesiterAccount(string accountName, string accountPwd)
        {
            //Todo:首先判断要注册的账户名是否已经被注册过了
            //如果没有被注册过 则开始进行注册
            //使用数据库连接工具类 进行数据的写入(这里写入需要往数据库中的账户表中写入)
            //以下的操作需要将校验结果返回给逻辑处理层 进行程序逻辑编写
            //写入完成之后,校验是否写入成功
            //如果写入成功,将信息通过日志管理类进行消息转发到服务端表现层显示出来
            AccountInfo newAccount = new AccountInfo(accountName, accountPwd);
            BroadgameDBTool.InsertData<AccountInfo>(Tables.AccountInfo, newAccount);
        }

        /// <summary>
        /// 注销账户
        /// </summary>
        /// <param name="accountName">注销的账户名</param>
        public void CloseAccount(string accountName)
        {
            //Todo:首先判断要注册的账户名是否存在
            //如果存在
            //使用数据库连接工具类 进行数据的读写(这里读写需要从数据库中的账户表中读取数据)
            //读取完成之后 执行逻辑删除的SQL命令 
            //以下的操作需要将校验结果返回给逻辑处理层 进行程序逻辑编写
            //执行完成之后,校验是否注销账户成功
            //如果注销账户成功,将信息通过日志管理类进行消息转发到服务端表现层显示出来
        }

        /// <summary>
        /// 检查账户信息数据是否匹配
        /// </summary>
        /// <param name="accountName">验证的账户名</param>
        /// <param name="accountPwd">验证的账户密码</param>
        public void CheckAccountInfoIsMatch(string accountName, string accountPwd)
        {
            //Todo:首先判断要注册的账户名是否存在
            //如果存在 再判断密码是否和账户名匹配
            //以下的操作需要将校验结果返回给逻辑处理层 进行程序逻辑编写
            //如果判断结果为真 提示玩家登陆成功
            //否则判断结果为假 提示玩家登录失败
        }
    }
}
