using Dlzyff.BoardGame.BottomServer.Concurrents;
using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGameServer.Model;
using System.Collections.Generic;

namespace Dlzyff.BoardGameServer.DataCache.Users
{
    /// <summary>
    /// 用户数据缓冲类
    /// </summary>
    public class UserCache
    {
        //RoomId
        /// <summary>
        /// 线程安全的整数类型
        /// </summary>
        private ConcurrentInteger userIdInteger = new ConcurrentInteger(0);

        /// <summary>
        /// 用户编号对应存储用户数据的数据字典
        /// </summary>
        private Dictionary<int, UserInfo> userIdUserDict = new Dictionary<int, UserInfo>();

        /// <summary>
        /// 客户端连接对象对应存储用户数据的数据字典
        /// </summary>
        private Dictionary<ClientPeer, UserInfo> clientUserDict = new Dictionary<ClientPeer, UserInfo>();

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="userInfo"></param>
        public void AddUser(ClientPeer clientPeer, UserInfo userInfo)
        {
            this.userIdUserDict.Add(userIdInteger.AddWithGet(), userInfo);
            this.clientUserDict.Add(clientPeer, userInfo);
        }

        /// <summary>
        /// 根据用户信息获取用户编号
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public int GetUserIdByUserInfo(UserInfo userInfo)
        {
            int userId = 0;
            foreach (KeyValuePair<int,UserInfo> userItem in this.userIdUserDict)
            {
                if (userItem.Value==userInfo)
                {
                    userId = userItem.Key;
                    break;
                }
            }
            return userId;
        }

        /// <summary>
        /// 为指定客户端连接对象加钱
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="money"></param>
        public void PlusMoney(ClientPeer clientPeer, int money)
        {
            if (money <= 0)
                return;
            else
            {
                this.clientUserDict[clientPeer].Money += money;
            }
        }

        /// <summary>
        /// 为指定客户端连接对象减钱
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="money"></param>
        public void SubMoney(ClientPeer clientPeer, int money)
        {
            if (money <= 0)
                return;
            else
            {
                if (money > this.clientUserDict[clientPeer].Money)
                    return;
                else
                {
                    this.clientUserDict[clientPeer].Money += money;
                }
            }
        }
    }
}
