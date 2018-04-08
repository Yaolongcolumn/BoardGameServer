using System.Threading;

namespace BoardGameServer
{
    /// <summary>
    /// 声明定义一个委托类型(作用：指向一个需要执行的方法)
    /// </summary>
    public delegate void ExecuteDelegate();
    /// <summary>
    /// 单线程池
    /// </summary>
    public class SingleExecute
    {
        private static SingleExecute _instance =new SingleExecute();
        public static SingleExecute Instance
        {
            get {
                if (_instance == null)
                    _instance = new SingleExecute();
                return _instance;
            }
        }
        //private static object o = new object();
        /// <summary>
        /// 互斥锁
        /// </summary>
        public Mutex mutex;
        private SingleExecute()
        {
            this.mutex = new Mutex();
        }
        /// <summary>
        /// 单线程处理逻辑
        /// </summary>
        /// <param name="executeDelegate"></param>
        public void Execute(ExecuteDelegate executeDelegate)
        {
            lock (this)
            {
                this.mutex.WaitOne();
                executeDelegate();
                this.mutex.ReleaseMutex();
            }
        }
    }
}
