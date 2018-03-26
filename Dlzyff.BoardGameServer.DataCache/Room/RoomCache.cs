﻿using Dlzyff.BoardGame.BottomServer.Concurrents;
using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGame.BottomServer.Tools;
using Dlzyff.BoardGameServer.Log;
using Dlzyff.BoardGameServer.Model;
using System;
using System.Collections.Generic;

namespace Dlzyff.BoardGameServer.DataCache.Room
{
    /// <summary>
    /// 房间数据缓存类
    /// </summary>
    public class RoomCache
    {

        #region 常量
        /// <summary>
        /// 房间内的最大玩家数
        /// </summary>
        public const int MAX_PERSON_NUMBER = 4; 
        #endregion

        #region 成员变量
        /// <summary>
        /// 线程安全的整数类型
        /// </summary>
        private ConcurrentInteger concurrentInteger = new ConcurrentInteger(0);

        /// <summary>
        /// 随机的房间进入码
        /// </summary>
        private Random ranRandomEnterCode = new Random(0); 
        #endregion

        #region 字段
        /// <summary>
        /// 房间编号对应的房间数据实体模型的数据字典
        /// </summary>
        private Dictionary<int, RoomInfo> _roomIdRooms = null;

        /// <summary>
        /// 房间数据实体模型对应的房间内的玩家对象的数据字典 
        /// </summary>
        private Dictionary<RoomInfo, List<ClientPeer>> _roomClientDict = null;

        /// <summary>
        /// 房间数据实体模型对应的房间内的准备的玩家对象的数据字典
        /// </summary>
        private Dictionary<RoomInfo, Dictionary<ClientPeer, bool>> _roomReadyClientDict = null;

        /// <summary>
        /// 房间内准备的玩家的数据字典
        /// </summary>
        private Dictionary<ClientPeer, bool> _readyClientDict = null;

        /// <summary>
        /// 存储所有房间信息数据的列表
        /// </summary>
        private List<RoomInfo> rooms = new List<RoomInfo>();

        /// <summary>
        /// 用来存储房间内的客户端座位号的数据字典
        /// </summary>
        private Dictionary<RoomInfo, Dictionary<ClientPeer, int>> roomClientIndexDict = new Dictionary<RoomInfo, Dictionary<ClientPeer, int>>();

        #endregion

        #region 属性
        /// <summary>
        /// 房间编号对应的房间数据实体模型的数据字典
        /// </summary>
        public Dictionary<int, RoomInfo> RoomIdRooms
        {
            get
            {
                return this._roomIdRooms;
            }
            private set
            {
                this._roomIdRooms = value;
            }
        }

        /// <summary>
        ///  房间数据实体模型对应的房间内的玩家对象的数据字典 
        /// </summary>
        public Dictionary<RoomInfo, List<ClientPeer>> RoomClientsDict
        {
            get
            {
                return this._roomClientDict;
            }
            private set
            {
                this._roomClientDict = value;
            }
        }

        /// <summary>
        /// 房间数据实体模型对应的房间内的准备的玩家对象的数据字典
        /// </summary>
        public Dictionary<RoomInfo, Dictionary<ClientPeer, bool>> RoomReadyClientDict
        {
            get { return this._roomReadyClientDict; }
            set { this._roomReadyClientDict = value; }
        }

        /// <summary>
        /// 用来存储房间内的客户端座位号的数据字典
        /// </summary>
        public Dictionary<RoomInfo, Dictionary<ClientPeer, int>> RoomClientIndexDict
        {
            get
            {
                return this.roomClientIndexDict;
            }
            set
            {
                this.roomClientIndexDict = value;
            }
        }

        /// <summary>
        /// 房间列表
        /// </summary>
        public List<RoomInfo> Rooms
        {
            get
            {
                return this.rooms;
            }
            set
            {
                this.rooms = value;
            }
        }
        #endregion

        public RoomCache()
        {
            this._roomIdRooms = new Dictionary<int, RoomInfo>();
            this._roomClientDict = new Dictionary<RoomInfo, List<ClientPeer>>();
            this.RoomReadyClientDict = new Dictionary<RoomInfo, Dictionary<ClientPeer, bool>>();
            this._readyClientDict = new Dictionary<ClientPeer, bool>();
        }

        #region 创建
        /// <summary>
        /// 创建房间
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <returns></returns>
        public RoomInfo CreateRoom(ClientPeer clientPeer)
        {
            int newRoomId = this.concurrentInteger.AddWithGet();
            RoomInfo room = new RoomInfo()
            {
                Id = newRoomId,
                Name = "测试房间[" + newRoomId + "]",
                EnterCode = this.ranRandomEnterCode.Next(100000, 999999)
            };
            this.RoomIdRooms.Add(newRoomId, room);//将创建好的房间保存起来
            this.RoomClientsDict.Add(room, new List<ClientPeer>() { clientPeer });//将创建房间的客户端对象保存起来
            this.Rooms.Add(room);//将创建好的新房间保存起来

            try
            {
                if (!this.RoomClientIndexDict.ContainsKey(room))
                    this.RoomClientIndexDict.Add(room, new Dictionary<ClientPeer, int>() { { clientPeer, 1 } });
                else
                    this.RoomClientIndexDict[room][clientPeer] = 1;
            }
            catch (Exception ex)
            {
                //LogMessage.Instance.SetLogMessage(ex.StackTrace);
                //  System.Console.WriteLine(ex.StackTrace);
            }

            return room;
        }
        #endregion

        #region 加入
        /// <summary>
        /// 根据指定房间编号加入房间
        /// </summary> 
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public RoomInfo JoinRoomByRoomId(ClientPeer clientPeer, int roomId = 1)
        {
            //这儿后期需要修改至根据客户端要指定加入的房间进行房间数据的获取处理
            //而不是默认为0的房间
            RoomInfo tmpRoomInfo = this._roomIdRooms[roomId];
            Join(clientPeer, roomId, tmpRoomInfo);

            return tmpRoomInfo;
        }

        /// <summary>
        /// 根据房间进入码加入房间
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomEnterCode"></param>
        /// <returns></returns>
        public RoomInfo JoinRoomByRoomEnterCode(ClientPeer clientPeer, int roomEnterCode)
        {
            //这儿后期需要修改至根据客户端要指定加入的房间进行房间数据的获取处理
            //而不是默认为0的房间
            RoomInfo tmpRoomInfo = null;
            foreach (RoomInfo room in this.rooms)
            {
                if (room.EnterCode == roomEnterCode)
                {
                    tmpRoomInfo = room;
                    break;
                }
            }
            this.Join(clientPeer, tmpRoomInfo.Id, tmpRoomInfo);
            return tmpRoomInfo;
        }

        /// <summary>
        /// 加入
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        /// <param name="tmpRoomInfo"></param>
        private void Join(ClientPeer clientPeer, int roomId, RoomInfo tmpRoomInfo)
        {
            this._roomClientDict[tmpRoomInfo].Add(clientPeer);
            try
            {
                if (!this.RoomClientIndexDict[tmpRoomInfo].ContainsKey(clientPeer))
                {
                    RoomInfo roomInfo = this.GetRoomInfoByRoomId(roomId);
                    List<ClientPeer> clients = this.RoomClientsDict[roomInfo];
                    Dictionary<ClientPeer, int> clientDict = this.RoomClientIndexDict[tmpRoomInfo];
                    this.RoomClientIndexDict[tmpRoomInfo].Add(clientPeer, ++clientDict[clients[0]]);//保存玩家加入房间后的座位索引值
                }
            }
            catch (System.Exception ex)
            {
                // System.Console.WriteLine(ex.StackTrace);
            }
        }
        #endregion

        #region 离开
        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public RoomInfo LeaveRoom(ClientPeer clientPeer, int roomId)
        {
            //离开的时候需要把客户端用户从房间内移除
            List<UserInfo> userInfos = this._roomIdRooms[roomId].UserInfos;
            try
            {
                for (int userIndex = 0; userIndex < userInfos.Count; userIndex++)
                {
                    if (userInfos[userIndex].ClientUserSocket == clientPeer.ClientSocket)
                    {
                        this.RoomIdRooms[roomId].UserInfos.Remove(userInfos[userIndex]);
                        break;
                    }
                }
                this._roomClientDict[this._roomIdRooms[roomId]].Remove(clientPeer);//从房间客户端对象中移除指定客户端连接对象
                this._roomIdRooms[roomId].PersonNumber--;//离开房间以后需要将人数减少1
                LogMessage.Instance.SetLogMessage(clientPeer.ClientSocket.RemoteEndPoint.ToString() + " 离开了 [ " + roomId + " ] 房间~");
            }
            catch
            {
            }
            return this._roomIdRooms[roomId];
        }
        #endregion

        #region 准备
        /// <summary>
        /// 房间内玩家准备
        /// </summary>
        /// <param name="clientPeer">准备的玩家</param>
        /// <param name="roomId">玩家所在房间</param>
        public void Ready(ClientPeer clientPeer, int roomId)
        {
            if (!this.RoomReadyClientDict.ContainsKey(this.GetRoomInfoByRoomId(roomId)))
            {
                this.RoomReadyClientDict.Add(this.GetRoomInfoByRoomId(roomId), this._readyClientDict);
                this.RoomReadyClientDict[this.GetRoomInfoByRoomId(roomId)].Add(clientPeer, true);
                LogMessage.Instance.SetLogMessage("玩家 [" + clientPeer.ClientSocket.RemoteEndPoint.ToString() + "] 在房间编号为 [" + roomId + "] 进行了准备~");
            }
            else
            {
                this.RoomReadyClientDict[this.GetRoomInfoByRoomId(roomId)].Add(clientPeer, true);
                LogMessage.Instance.SetLogMessage("玩家 [" + clientPeer.ClientSocket.RemoteEndPoint.ToString() + "] 在房间编号为 [" + roomId + "] 进行了准备~");
            }
        }
        #endregion

        #region 取消准备
        /// <summary>
        /// 房间内玩家取消准备
        /// </summary>
        /// <param name="clientPeer">取消准备的玩家</param>
        /// <param name="roomId">玩家所在的房间</param>
        public void CancelReady(ClientPeer clientPeer, int roomId)
        {
            if (this.RoomReadyClientDict.ContainsKey(this.GetRoomInfoByRoomId(roomId)))
            {
                this.RoomReadyClientDict[this.GetRoomInfoByRoomId(roomId)][clientPeer] = false;
                LogMessage.Instance.SetLogMessage("玩家 [" + clientPeer.ClientSocket.RemoteEndPoint.ToString() + "] 在房间编号为 [" + roomId + "] 取消了准备~");
            }
            else
                return;
        }
        #endregion

        #region 获取房间数据
        /// <summary>
        /// 通过房间编号获取房间信息数据
        /// </summary>
        /// <param name="roomId">要获取房间信息数据的房间编号</param>
        /// <returns></returns>
        public RoomInfo GetRoomInfoByRoomId(int roomId)
        {
            return this.Rooms.Find(roomInfo => roomInfo.Id == roomId);
        }
        #endregion

        #region 获取房间内的座位号
        /// <summary>
        /// 通过房间编号获取房间内的客户端座位号
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public int GetRoomClientIndexByRoomId(ClientPeer clientPeer, int roomId)
        {
            RoomInfo tmpRoomInfo = this.GetRoomInfoByRoomId(roomId);
            int index = this.RoomClientIndexDict[tmpRoomInfo][clientPeer];
            return index;
        }
        #endregion

        #region 是不是房主
        /// <summary>
        /// 校验指定的客户端连接对象是不是房主(说白了 就是创建房间的客户端)
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <returns></returns>
        public bool IsHouseOwner(ClientPeer clientPeer, int roomId)
        {
            bool isHouseOwner = false;
            RoomInfo roomInfo = this.GetRoomInfoByRoomId(roomId);
            if (roomInfo != null)
                isHouseOwner = this.RoomClientsDict[roomInfo][0] == clientPeer;
            return isHouseOwner;
        }

        #endregion

        #region 是否可以开始游戏
        /// <summary>
        /// 是否可以开始游戏
        /// </summary>
        /// <returns></returns>
        public bool IsCanStartGame(int roomId)
        {
            bool isStartGame = false;
            RoomInfo tmpRoomInfo = this.GetRoomInfoByRoomId(roomId);//获取房间信息数据
            //判断房间内的玩家个数 是否 和 房间内准备的玩家个数是否保持一致
            int readyCount = 0;
            //如果取到的房间不为空的话
            if (this.RoomReadyClientDict[tmpRoomInfo] != null)
            {
                Dictionary<ClientPeer, bool> clientDict = this.RoomReadyClientDict[tmpRoomInfo];//取得房间内对应准备的玩家数据字典
                foreach (KeyValuePair<ClientPeer, bool> clientItem in clientDict)//遍历这个数据字典
                {
                    if (clientItem.Value == true)//如果当前遍历到的玩家对象的标志位为true的话 表示玩家准备了
                        readyCount++;   //准备了就让准备个数+1(表示房间内有一个玩家准备了)
                }
            }
            if (tmpRoomInfo.UserInfos.Count == readyCount)//如果准备的玩家个数 和 房间内的玩家个数 保持一致的话 则表示可以进行开始游戏了~
            {
                isStartGame = true;//设置可以开始游戏的标志位
                LogMessage.Instance.SetLogMessage("房间编号为 [" + roomId.ToString() + "] 准备开始游戏了~");
            }
            else
            {
                LogMessage.Instance.SetLogMessage("房间编号为 [" + roomId.ToString() + "] 现在还不能开始游戏~");
                LogMessage.Instance.SetLogMessage("为什么不能开始游戏呢?因为房间编号为 [" + roomId.ToString() + "] 的总玩家个数为 [" + tmpRoomInfo.UserInfos.Count + "]" + ",但是现在准备的玩家只有 [" + this.RoomReadyClientDict.Values.Count.ToString() + "].");
                return isStartGame;
            }
            return isStartGame;//将标志位返回
        }
        #endregion

        #region 解散房间
        /// <summary>
        /// 根据房间编号来解散房间解散房间
        /// </summary>
        /// <param name="roomId">要解散的房间编号</param>
        public void DisbanadRoom(int roomId)
        {
            //在解散房间的时候 需要把和这个房间编号有关的数据结构中的数据进行移除处理
            RoomInfo roomInfo = this.RoomIdRooms[roomId];
            this.RoomClientsDict.Remove(roomInfo);
            this.RoomReadyClientDict.Remove(roomInfo);
            this.Rooms.Remove(roomInfo);
            this.RoomIdRooms.Remove(roomId);
        }
        #endregion

        #region 广播消息给房间内的每一个客户端对象
        /// <summary>
        /// 房间内消息广播
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        public void BroadcastMessageByRoomId(int roomId, SocketMessage message)
        {
            byte[] dataValueBytes = EncodeHelper.EncodeMessage(message);//第一次打包消息
            byte[] sendMessageDataBytes = EncodeHelper.EncodeMessage(dataValueBytes);//第二次打包消息
            foreach (RoomInfo roomItem in this._roomIdRooms.Values)//遍历所有房间 直到找到要广播消息的房间
            {
                if (roomItem.Id == roomId)
                {
                    List<ClientPeer> clientPeers = this._roomClientDict[roomItem];
                    for (int clientIndex = 0; clientIndex < clientPeers.Count; clientIndex++)
                    {
                        ClientPeer tmpClientPeer = clientPeers[clientIndex];
                        tmpClientPeer.SendMessage(sendMessageDataBytes);
                    }
                }
            }
        }

        /// <summary>
        /// 房间内消息广播跳过一个指定的客户端对象
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="roomId"></param>
        /// <param name="message"></param>
        public void BroadCastMessageByExClient(ClientPeer clientPeer, int roomId, SocketMessage message)
        {
            byte[] dataValueBytes = EncodeHelper.EncodeMessage(message);//第一次打包消息
            byte[] sendMessageDataBytes = EncodeHelper.EncodeMessage(dataValueBytes);//第二次打包消息
            foreach (RoomInfo roomItem in this._roomIdRooms.Values)//遍历所有房间 直到找到要广播消息的房间
            {
                if (roomItem.Id == roomId)
                {
                    List<ClientPeer> clientPeers = this._roomClientDict[roomItem];
                    for (int clientIndex = 0; clientIndex < clientPeers.Count; clientIndex++)
                    {
                        ClientPeer tmpClientPeer = clientPeers[clientIndex];
                        if (tmpClientPeer != clientPeer)
                            tmpClientPeer.SendMessage(sendMessageDataBytes);
                    }
                }
            }
        }
        #endregion
    }
}
