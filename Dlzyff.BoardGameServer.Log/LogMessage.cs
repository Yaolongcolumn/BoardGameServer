using System;

namespace Dlzyff.BoardGameServer.Log
{
    /// <summary>
    /// 日志消息类
    /// </summary>
    public class LogMessage
    {
        private static LogMessage _instance = null;
        private static object o = new object();
        public event Action<string> AddMessageEvent;
        public static LogMessage Instance
        {
            get {
                lock (o) {
                    if (_instance==null)
                    {
                        _instance = new LogMessage();
                    }
                    return _instance;
                }
            }
        }
        private LogMessage() { }
        /// <summary>
        /// 设置日志消息
        /// </summary>
        /// <param name="message">要设置的消息</param>
        public void SetLogMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            else
            {
                if (this.AddMessageEvent == null)
                    return;
                else
                {
                    this.AddMessageEvent(message);
                }
            }
        }
    }
}
