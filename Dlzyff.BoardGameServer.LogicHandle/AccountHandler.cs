using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGame.Protocol.Codes;
using Dlzyff.BoardGameServer.Handles;
using System;

namespace Dlzyff.BoardGameServer.LogicHandle
{
    /// <summary>
    /// 账户逻辑处理类
    /// </summary>
    public class AccountHandler : IHandler
    {
        public void OnDisconnect(ClientPeer clientPeer)
        {
            if (clientPeer != null)
                clientPeer.OnDisconnect();
        }   

        public void OnReceiveMessage(ClientPeer clientPeer, int subOperationCode, object dataValue)
        {
            AccountCode accountCode = (AccountCode)Enum.Parse(typeof(AccountCode), subOperationCode.ToString());
            switch (accountCode)
            {
                case AccountCode.Register_Request:
                    break;
                case AccountCode.Login_Request:
                    break;
            }
        }
        private void ProcessRegisterRequest() { }
        private void ProcessLoginRequest() { }
    }
}
