using Dlzyff.BoardGame.Protocol.Codes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Dlzyff.BoardGame.BottomServer.Tools
{
    /// <summary>
    /// 对消息体进行编码的工具类
    /// </summary>
    public class EncodeHelper
    {
        #region 粘包拆包问题 封装一个有规定的数据

        /// <summary>
        /// 构造消息体： 消息头 + 消息尾
        /// </summary>
        /// <param name="data">要重新打包构造的消息体数据</param>
        /// <returns></returns>
        public static byte[] EncodeMessage(byte[] data)
        {
            //构建一个新的消息体数据缓冲区 用于存储打包后的消息体
            byte[] newBytes = null;
            //使用内存流将要重新打包的消息体读取出来
            using (MemoryStream ms = new MemoryStream())
            {
                //使用二进制的方式重新写入内存流中
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    //写入消息体长度
                    bw.Write(data.Length);
                    //写入消息体内容
                    bw.Write(data);
                    newBytes = new byte[(int)ms.Length];
                    //使用拷贝数据的方式将打包好的数据拷贝到一个新的字节数组中去
                    Buffer.BlockCopy(ms.GetBuffer(), 0, newBytes, 0, (int)ms.Length);
                }
            }
            return newBytes;//将打包好的消息体数据返回(打包完成)
        }

        /// <summary>
        /// 解析消息体：从数据缓冲区中取出一个一个完整的消息
        /// </summary>
        /// <param name="dataCache"></param>
        /// <returns></returns>
        public static byte[] DecodeMessage(ref List<byte> dataCache)
        {
            if (dataCache.Count < 4)//如果数据缓冲区的字节个数小于4个,则不能构成一个完整的消息
                //throw new Exception("消息体数据不够长,不能构成一个完整的消息~");
                return null;
            byte[] data = null;//用于存储解析消息后的消息体数据
            //使用内存流读取数据缓冲区中的消息体
            using (MemoryStream ms = new MemoryStream(dataCache.ToArray()))
            {
                //使用二进制方式从内存流中读取消息体
                using (BinaryReader br = new BinaryReader(ms))
                {
                    //读取消息体长度(数据的长度)
                    int length = br.ReadInt32();
                    //获取剩余的消息体长度(剩余数据的长度)
                    int dataRemainLength = (int)(ms.Length - ms.Position);
                    //如果数据的长度>剩余数据的长度(则表示不符合消息体的解析规定)
                    if (length > dataRemainLength)
                        //throw new Exception("消息体数据不够消息头约定的长度,不能构成一个完整的消息~");
                        return null;
                    //根据取到的消息体长度,进行消息体的读取
                    data = br.ReadBytes(length);
                    //清空消息体数据缓冲区(表示读取完一条完整的消息体)
                    dataCache.Clear();
                    //将读取后的消息体重新存放到消息体数据缓冲区中存储
                    dataCache.AddRange(br.ReadBytes(dataRemainLength));
                }
            }
            return data;//将解析好的消息体数据返回
        }

        #endregion

        #region 构造发送的SocketMessage类
        /// <summary>
        /// 打包要进行发送的网络消息(打包完成后返回一个存储消息体数据的字节数组)
        /// </summary>
        /// <param name="message">要构造的网络消息</param>
        /// <returns></returns>
        public static byte[] EncodeMessage(SocketMessage message)
        {
            MemoryStream ms = new MemoryStream();//构建内存流对象用于读取要构造的网络消息
            BinaryWriter bw = new BinaryWriter(ms);//构建二进制写入对象用于将网络消息写入内存
            bw.Write((int)message.OperationCode);//通过二进制的方式写入操作码
            bw.Write(message.SubOperationCode);//通过二进制的方式写入子操作码
            if (message.DataValue != null)//如果网络消息中的数据值不为空的情况下
            {
                //将数据值进行写入处理
                byte[] tmpDataValueBytes = EncodeObject(message.DataValue);
                bw.Write(tmpDataValueBytes);
            }
            //将内存流中的缓冲区数据拷贝到一个新的字节数组中(完成打包消息体)
            byte[] dataValueBytes = new byte[(int)ms.Length];
            Buffer.BlockCopy(ms.GetBuffer(), 0, dataValueBytes, 0, (int)ms.Length);
            bw.Close();
            ms.Close();
            return dataValueBytes;//将打包好的消息体返回
        }
        /// <summary>
        /// 解析接收到消息体数据(解析完成返回一个网络消息类型的对象)
        /// </summary>
        /// <param name="dataValueBytes">要解析的字节数据</param>
        /// <returns></returns>
        public static SocketMessage DecodeMessage(byte[] dataValueBytes)
        {
            MemoryStream ms = new MemoryStream(dataValueBytes);//构建内存流对象用于读取要解析的网络消息
            BinaryReader br = new BinaryReader(ms);//构建二进制写入对象将消息体数据从内存流中读取出来
            SocketMessage message = new SocketMessage();//构建一个网络消息
            int operationCode = br.ReadInt32();//读取操作码
            int subOperationCode = br.ReadInt32();//读取子操作码
            OperationCode opCode = (OperationCode)Enum.Parse(typeof(OperationCode), operationCode.ToString());
            message.OperationCode = opCode;
            message.SubOperationCode = subOperationCode;
            if (ms.Length > ms.Position)//表示从内存流中读取的消息体数据还有剩余待读取的字节个数(也就是在解析数据的过程中还附带数据,需要对数据的解析)
            {
                //读取数据值
                byte[] tmpDataValueBytes = br.ReadBytes((int)(ms.Length - ms.Position));
                object value = DecodeObject(tmpDataValueBytes);
                message.DataValue = value;
            }
            br.Close();
            ms.Close();
            return message;//将解析好的网络消息返回
        }
        #endregion

        #region 序列化和反序列化数据对象
        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="dataValue"></param>
        /// <returns></returns>
        public static byte[] EncodeObject(object dataValue)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, dataValue);
                byte[] dataValueBytes = new byte[(int)ms.Length];
                Buffer.BlockCopy(ms.GetBuffer(), 0, dataValueBytes, 0, (int)ms.Length);
                return dataValueBytes;
            }
        }
        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <param name="dataValueBytes"></param>
        /// <returns></returns>
        public static object DecodeObject(byte[] dataValueBytes)
        {
            using (MemoryStream ms = new MemoryStream(dataValueBytes))
            {
                BinaryFormatter bf = new BinaryFormatter();
                object obj = bf.Deserialize(ms);
                return obj;
            }
        }
        #endregion
    }
}
