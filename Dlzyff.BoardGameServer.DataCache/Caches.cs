using Dlzyff.BoardGameServer.DataCache.Room;
using Dlzyff.BoardGameServer.DataCache.Services;
using Dlzyff.BoardGameServer.DataCache.Users;

namespace Dlzyff.BoardGameServer.Cache
{
    /// <summary>
    /// 存储所有数据缓存对象
    /// </summary>
    public class Caches
    {

        /// <summary>
        /// 用户数据缓存对象
        /// </summary>
        public static UserCache UserCache { get; set; }

        /// <summary>
        /// 房间数据缓存对象
        /// </summary>
        public static RoomCache RoomCache { get; set; }

        /// <summary>
        /// 帕斯业务数据缓存对象
        /// </summary>
        public static PasseServiceCache PasseServiceCache { get; set; }

        static Caches()
        {
            UserCache = new UserCache();
            RoomCache = new RoomCache();
            PasseServiceCache = new PasseServiceCache();
        }
    }
}
