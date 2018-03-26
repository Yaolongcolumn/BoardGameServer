namespace Dlzyff.BoardGame.BottomServer.Models
{
    public delegate void TimerDelegate();
    /// <summary>
    /// 计时任务数据模型映射类
    /// </summary>
    public class TimerModel
    {
        /// <summary>
        /// 计时任务唯一编号
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 计时任务执行的时间
        /// </summary>
        private long Time { get; set; }
        private TimerDelegate TimeDelegate { get; set; }
        public TimerModel(int id, long time, TimerDelegate timeDelegate)
        {
            this.Id = id;
            this.Time = time;
            this.TimeDelegate = timeDelegate;
        }
        /// <summary>
        /// 触发运行计时器任务
        /// </summary>
        public void Run()
        {
            if (this.TimeDelegate != null)
            {
                this.TimeDelegate();
            }
        }
    }
}
