using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGame.Protocol.Codes.GameCode;
using Dlzyff.BoardGameServer.Handles;
using Dlzyff.BoardGameServer.LogicHandle.ServiceHandlers;
using System;

namespace Dlzyff.BoardGameServer.LogicHandle
{
    /// <summary>
    /// 游戏业务处理类
    /// </summary>
    public class ServiceHandler : IHandler
    {
        /// <summary>
        /// 帕斯游戏业务处理对象
        /// </summary>
        private IHandler passeServiceHandler = new PasseServiceHandler();

        /// <summary>
        /// 五轰六炸游戏业务处理对象
        /// </summary>
        private IHandler fivebombsWithSixbombsServiceHandler = new FivebombsWithSixbombsServiceHandler();

        /// <summary>
        /// 麻将游戏业务处理对象
        /// </summary>
        private IHandler mahjongServiceHandler = new MahjongServiceHandler();

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="clientPeer"></param>
        public void OnDisconnect(ClientPeer clientPeer)
        {
            if (clientPeer != null)
                clientPeer.OnDisconnect();
        }

        /// <summary>
        /// 接收客户端发送过来的消息
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="subOperationCode"></param>
        /// <param name="dataValue"></param>
        public void OnReceiveMessage(ClientPeer clientPeer, int subOperationCode, object dataValue)
        {
            GameServiceTypeCode serviceCode = (GameServiceTypeCode)Enum.Parse(typeof(GameServiceTypeCode), subOperationCode.ToString());
            string[] dataSplits = dataValue.ToString().Split(',');
            string serviceSubCode = dataSplits[0];
            string data = dataSplits[1];
            switch (serviceCode)
            {
                case GameServiceTypeCode.PasseService:
                    //Todo:需要解析客户端传递过来的消息附带的数据
                    //解析格式:具体的子操作码,附带的数据
                    {
                        this.passeServiceHandler.OnReceiveMessage(clientPeer, Convert.ToInt32(serviceSubCode), data);//将接收到的消息发送给帕斯业务模块自己去处理
                    }
                    break;
                case GameServiceTypeCode.FivebombsWithSixbombsService:
                    //Todo:需要解析客户端传递过来的消息附带的数据
                    //解析格式:具体的子操作码,附带的数据
                    {
                        this.fivebombsWithSixbombsServiceHandler.OnReceiveMessage(clientPeer, Convert.ToInt32(serviceSubCode), data);//将接收到的消息发送给五轰六炸业务模块自己去处理
                    }
                    break;
                case GameServiceTypeCode.MahjongService:
                    //Todo:需要解析客户端传递过来的消息附带的数据
                    //解析格式:具体的子操作码,附带的数据
                    {
                        this.mahjongServiceHandler.OnReceiveMessage(clientPeer, Convert.ToInt32(serviceSubCode), data);//将接收到的消息发送给麻将业务模块自己去处理
                    }
                    break;
            }
        }
    }
}
