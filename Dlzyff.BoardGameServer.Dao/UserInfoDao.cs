using Dlzyff.BoardGameServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dlzyff.BoardGameServer.Dao
{
    /// <summary>
    /// 用户信息数据访问类(就是跟数据库中的用户表打交道的类)
    /// </summary>
    public class UserInfoDao
    {
        /// <summary>
        /// 根据账户编号添加用户
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="accountId"></param>
        public void AddUserInfo(UserInfo userInfo, int accountId)
        {
            //Todo:首先判断这个账户编号下是否已经存在这个要添加的用户信息
            //如果存在 直接跳出方法即可(说明这个账户已经创建添加了这个用户) 
            //并将结果返回给上层逻辑层 
            //如果不存在 则直接通过数据库工具类 进行对数据库中的用户表进行写入操作
            //以下的操作需要将校验结果返回给逻辑处理层 进行程序逻辑编写
            //写入完成之后,校验是否写入成功
            //如果写入成功,将信息通过日志管理类进行消息转发到服务端表现层显示出来
        }

        /// <summary>
        /// 根据用户编号移除用户信息
        /// </summary>
        /// <param name="userId"></param>
        public void RemoveUserInfoByUserId(int userId)
        {
            //Todo:首先判断这个用户编号下是否存在
            //如果不存在 直接跳出方法即可(说明这个用户没有被创建添加)
            //如果存在 則直接通過数据库工具类 进行对数据库中的用户表和账户表进行逻辑删除操作
            //以下的操作需要将校验结果返回给逻辑处理层 进行程序逻辑编写
            //完成逻辑删除操作之后,校验是否逻辑删除成功
            //如果逻辑删除成功,将信息通过日志管理类进行消息转发到服务端表现层显示出来
        }

        /// <summary>
        /// 根据用户编号增加钱数
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="money"></param>

        public void AddMoneyByUserId(int userId, int money)
        {
            //Todo:首先判断这个用户编号下是否存在
            //如果不存在 直接跳出方法即可(说明这个用户不存在于数据库中)
            //如果存在 則直接通過数据库工具类 进行对数据库中的用户表进行读取用户数据操作
            //读取完之后 在用户原有的钱数的基础上 累加要给这个用户的累加的钱数
            //累加之后 在通过数据库工具类 进行对数据库中的用户表进行写入用户数据操作
            //以下的操作需要将校验结果返回给逻辑处理层 进行程序逻辑编写
            //显示玩家新的钱数信息即可
        }

        /// <summary>
        /// 根据用户编号减少钱数
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="money"></param>

        public void SubMoneyByUserId(int userId, int money)
        {
            //Todo:首先判断这个用户编号下是否存在
            //如果不存在 直接跳出方法即可(说明这个用户不存在于数据库中)
            //如果存在 則直接通過数据库工具类 进行对数据库中的用户表进行逻辑读取用户数据操作
            //读取完之后 在用户原有的钱数的基础上 累减要给这个用户的累减的钱数
            //累减之后 在通过数据库工具类 进行对数据库中的用户表进行写入用户数据操作
            //以下的操作需要将校验结果返回给逻辑处理层 进行程序逻辑编写
            //显示玩家新的钱数信息即可
        }

        public UserInfo GetUserInfoByUserId(int userId)
        {
            return new UserInfo();
        }

        /// <summary>
        /// 获取所有用户信息
        /// </summary>
        /// <returns></returns>
        public List<UserInfo> GetAllUserinfo()
        {
            return new List<UserInfo>();
        }
    }
}
