using Dlzyff.BoardGame.Protocol.Codes;

namespace Dlzyff.BoardGame.BottomServer.Tools
{
    /// <summary>
    /// 网络消息类
    /// </summary>
    public class SocketMessage
    {
        private OperationCode _operationCode;
        private int _subOperationCode;
        private object _dataValue;
        /// <summary>
        /// 操作指令码
        /// </summary>
        public OperationCode OperationCode
        {
            get { return this._operationCode; }
            set { this._operationCode = value; }
        }
        /// <summary>
        /// 子操作指令码
        /// </summary>
        public int SubOperationCode
        {
            get { return this._subOperationCode; }
            set { this._subOperationCode = value; }
        }
        /// <summary>
        /// 数据值
        /// </summary>
        public object DataValue
        {
            get { return this._dataValue; }
            set { this._dataValue = value; }
        }
        public SocketMessage()
        {

        }
        /// <summary>
        /// 带参数的SocketMessage类的构造函数
        /// </summary>
        /// <param name="operationCode">操作指令码</param>
        /// <param name="subOperationCode">子操作指令码</param>
        /// <param name="dataValue">数据值</param>
        public SocketMessage(OperationCode operationCode, int subOperationCode, object dataValue)
        {
            this.OperationCode = operationCode;
            this.SubOperationCode = subOperationCode;
            this.DataValue = dataValue;
        }
        /// <summary>
        /// 更改网络消息
        /// </summary>
        /// <param name="operationCode">操作指令码</param>
        /// <param name="subOperationCode">子操作指令码</param>
        /// <param name="dataValue">数据值</param>
        public void ChangeMessage(OperationCode operationCode, int subOperationCode, object dataValue)
        {
            this.OperationCode = operationCode;
            this.SubOperationCode = subOperationCode;
            this.DataValue = dataValue;
        }
    }
}
