﻿using Dlzyff.BoardGame.BottomServer.Applications;
using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGame.BottomServer.Tools;
using Dlzyff.BoardGame.Protocol.Codes;
using Dlzyff.BoardGameServer.Handles;
using Dlzyff.BoardGameServer.LogicHandle;
using System;

namespace Dlzyff.BoardGameServer.ServerView.Centers
{
    /// <summary>
    /// 网络消息转发中心
    /// </summary>
    public class NetMessageCenter : IApplicationBase
    {
        /// <summary>
        /// 房间逻辑处理对象
        /// </summary>
        private IHandler roomHandler = new RoomHandler();
        public void OnDisconnect(ClientPeer clientPeer)
        {
            this.roomHandler.OnDisconnect(clientPeer);
        }

        public void OnReceiveMessage(ClientPeer clientPeer, SocketMessage socketMessage)
        {
            switch (socketMessage.OperationCode)
            {
                case OperationCode.Account:
                    //Console.WriteLine(socketMessage.OperationCode.ToString() + " " + socketMessage.SubOperationCode.ToString() + " " + socketMessage.DataValue.ToString());
                    //         this.accountHandle.OnReceiveMessage(clientPeer, socketMessage.SubOperationCode, socketMessage.DataValue);
                    break;
                case OperationCode.User:
                    //      this.userHandle.OnReceiveMessage(clientPeer, socketMessage.SubOperationCode, socketMessage.DataValue);
                    break;
                case OperationCode.Room:
                    this.roomHandler.OnReceiveMessage(clientPeer, socketMessage.SubOperationCode, socketMessage.DataValue);
                    break;
                case OperationCode.Service:
                    break;
            }
        }
    }
}
