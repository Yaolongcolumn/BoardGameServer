using Dlzyff.BoardGame.BottomServer.Concurrents;
using Dlzyff.BoardGame.BottomServer.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;

namespace Dlzyff.BoardGame.BottomServer.Managers
{
    /// <summary>
    /// 计时任务管理器
    /// </summary>
    public class TimerManager
    {
        private static TimerManager _instance;
        public static TimerManager Instance
        {
            get
            {
                lock (_instance)
                {
                    if (_instance == null)
                        _instance = new TimerManager();
                    return _instance;
                }
            }
        }

        /// <summary>
        /// 计时任务的唯一编号
        /// </summary>
        private ConcurrentInteger concurrentIntegerId = new ConcurrentInteger(-1);
        /// <summary>
        /// 时间类
        /// </summary>
        private Timer timer = null;
        /// <summary>
        /// 这个字典存储：计时任务的唯一编号和计时任务模型,主要是为这两者做数据映射
        /// </summary>
        private ConcurrentDictionary<int, TimerModel> timerDictionary = new ConcurrentDictionary<int, TimerModel>();
        /// <summary>
        /// 存储要移除的计时任务唯一编号
        /// </summary>
        private List<int> removeTimerModelById = new List<int>();
        public TimerManager()
        {
            this.timer = new Timer(1000);
            this.timer.Elapsed += this.OnTimerelapsed;//注册一个时间间隔的事件函数
        }
        /// <summary>
        /// 达到一定时间间隔的时候进行触发的事件回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimerelapsed(object sender, ElapsedEventArgs e)
        {
            lock (this.removeTimerModelById)
            {
                TimerModel tmpModel = null;
                if (this.removeTimerModelById.Count > 0)
                {
                    foreach (var modelId in this.removeTimerModelById)//循环遍历要移除的计时任务编号集合
                        this.timerDictionary.TryRemove(modelId, out tmpModel);//进行移除
                    this.removeTimerModelById.Clear();
                }
            }
            if (this.timerDictionary.Count > 0)
            {
                foreach (TimerModel model in this.timerDictionary.Values)//循环遍历存储计时任务的字典
                    model.Run();//开始执行计时任务
            }
        }
        /// <summary>
        /// 根据指定日期时间添加一个计时任务事件
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="timerDelegate"></param>
        public void OnAddTimerEvent(DateTime dateTime, TimerDelegate timerDelegate)
        {
            long delayTime = dateTime.Ticks - DateTime.Now.Ticks;
            if (delayTime <= 0)
                return;
            this.OnAddTimerEvent(delayTime, timerDelegate);
        }
        /// <summary>
        /// 根据指定延迟时间添加一个计时任务事件
        /// </summary>  
        /// <param name="delayTime"></param>
        /// <param name="timerDelegate"></param>
        public void OnAddTimerEvent(long delayTime, TimerDelegate timerDelegate)
        {
            TimerModel tmpTimerModel = new TimerModel(this.concurrentIntegerId.AddWithGet(), DateTime.Now.Ticks + delayTime, timerDelegate);
            this.timerDictionary.TryAdd(tmpTimerModel.Id, tmpTimerModel);//将构建的计时任务添加存储到字典中 
        }
    }
}
