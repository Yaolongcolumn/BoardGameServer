namespace Dlzyff.BoardGame.BottomServer.Concurrents
{
    /// <summary>
    /// 线程安全的Integer辅助类
    /// </summary>
    public class ConcurrentInteger
    {
        private int value;

        public ConcurrentInteger(int value)
        {
            this.value = value;
        }
        /// <summary>
        /// 添加并获取值
        /// </summary>
        /// <returns></returns>
        public int AddWithGet()
        {
            lock (this)
            {
                this.value++;
                return this.value;
            }
        }
        /// <summary>
        /// 减少并获取值
        /// </summary>
        /// <returns></returns>
        public int ReduceWithGet()
        {
            lock (this)
            {
                this.value--;
                return this.value;
            }
        }
        /// <summary>
        /// 获取值
        /// </summary>
        /// <returns></returns>
        public int Get()
        {
            return this.value;
        }
    }
}
