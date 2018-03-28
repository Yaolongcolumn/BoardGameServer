using Dlzyff.BoardGameServer.Log;
using Dlzyff.BoardGameServer.Model;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
namespace Dlzyff.BoardGameServer.Dao.Tools
{
    /// <summary>
    /// 数据库操作工具类
    /// </summary>
    public static class BroadgameDBTool
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public static string SQL_CONNECT_STR = string.Empty;
        /// <summary>
        /// 连接对象
        /// </summary>
        public static SqlConnection connection = null;
        /// <summary>
        /// 命令对象
        /// </summary>
        public static SqlCommand command = null;
        /// <summary>
        /// 数据读取器对象
        /// </summary>
        public static SqlDataReader reader = null;
        /// <summary>
        /// 数适配器对象
        /// </summary>
        public static SqlDataAdapter adapter = null;
        /// <summary>
        /// 数据库命令
        /// </summary>
        public static string sqlCommandText = string.Empty;
        /// <summary>
        /// 参数列表
        /// </summary>
        public static List<SqlParameter> parameters = null;

        static BroadgameDBTool()
        {
            connection = new SqlConnection(SQL_CONNECT_STR);
            parameters = new List<SqlParameter>();
        }

        /// <summary>
        /// 设置连接字符串
        /// </summary>
        /// <param name="connectStr">连接字符串</param>
        public static void SetConnectStr(string connectStr)
        {
            SQL_CONNECT_STR = connectStr;
        }

        /// <summary>
        /// 设置命令
        /// </summary>
        /// <param name="commandText"></param>
        public static void SetCommandText(string commandText) { }

        /// <summary>
        /// 设置参数列表
        /// </summary>
        /// <param name="parameters"></param>
        public static void SetParameterList(params SqlParameter[] parameters) { }

        /// <summary>
        ///  向指定数据表中插入数据(向数据库中的对应数据表插入一条新的数据的操作)
        /// </summary>
        /// <param name="dataTableName">指定插入数据的数据表名称</param>
        /// <param name="data">插入数据时附带的数据</param>
        public static void InsertData<ParameterDataType>(string dataTableName, ParameterDataType data)
        {
            switch (dataTableName)
            {
                case Tables.AccountInfo:    //处理账户数据表的数据插入(这里的插入操作 无非就是注册一个新的账户)
                    {
                        AccountInfo accountInfo = data as AccountInfo;
                        if (accountInfo != null)
                        {
                            //在插入数据之前 这可能还要判断数据表中是否已经存在相同的数据 如果存在 则不能进行插入
                            BroadgameDBTool.SetCommandText("Insert Into AccountInfo(Name,Password) (@Name,@Password)");//构建Sql语句
                                                                                                                       //设置参数列表
                            BroadgameDBTool.SetParameterList
                               (
                                   new SqlParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Size = 15, Value = accountInfo.AccountUsername },
                                   new SqlParameter() { ParameterName = "@Password", SqlDbType = SqlDbType.NVarChar, Size = 15, Value = accountInfo.AccountPassword }
                               );
                            BroadgameDBTool.NoramlExecute();//执行普通命令
                        }
                    }
                    break;
                case Tables.UserInfo:   //处理用户数据表的数据插入(这里的插入操作 无非就是根据账户数据创建一个新的用户)
                    {
                        UserInfo userInfo = data as UserInfo;
                        if (userInfo != null)
                        {
                            //在插入数据之前 这可能还要判断数据表中是否已经存在相同的数据 如果存在 则不能进行插入
                            BroadgameDBTool.SetCommandText("Insert Into UserInfo(Name,HeadImgName,Wincount,Losecount,Runcount,Expvalue,Money) (@Name,@HeadImgName,@Wincount,@Losecount,@Runcount,@Expvalue,@Money)");//构建Sql命令                
                            BroadgameDBTool.SetParameterList
                                (
                                   new SqlParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Size = 15, Value = userInfo.UserName },
                                   new SqlParameter() { ParameterName = "@HeadImgName", SqlDbType = SqlDbType.NVarChar, Size = 15, Value = userInfo.HeadImgName },
                                   new SqlParameter() { ParameterName = "@Wincount", SqlDbType = SqlDbType.Int, Size = 4, Value = userInfo.WinCount },
                                   new SqlParameter() { ParameterName = "@Losecount", SqlDbType = SqlDbType.Int, Size = 4, Value = userInfo.LoseCount },
                                   new SqlParameter() { ParameterName = "@Runcount", SqlDbType = SqlDbType.Int, Size = 4, Value = userInfo.RunCount },
                                   new SqlParameter() { ParameterName = "@Expvalue", SqlDbType = SqlDbType.Float, Size = 4, Value = userInfo.ExpValue },
                                   new SqlParameter() { ParameterName = "@Money", SqlDbType = SqlDbType.Int, Size = 10, Value = userInfo.Money }
                                );
                            BroadgameDBTool.NoramlExecute();//执行普通命令
                        }
                    }
                    break;
            }
        }

        /// <summary>
        ///  更新指定数据表中的数据(对数据库中的对应的数据表中已存在的数据做一个修改操作)
        /// </summary>
        /// <param name="dataTableName">指定更新数据的逻辑表名称</param>
        /// <param name="data">插入数据时附带的数据</param>
        public static void UpdateData<ParameterDataType>(string dataTableName, ParameterDataType data)
        {
            switch (dataTableName)
            {
                case Tables.AccountInfo:
                    BroadgameDBTool.SetCommandText("");
                    break;
                case Tables.UserInfo:
                    BroadgameDBTool.SetCommandText("");
                    break;
            }
        }

        /// <summary>
        /// 直接[物理]删除指定数据表中的数据(对数据库中的对应的数据表中已存在的数据做一个直接[物理]删除操作)
        /// </summary>
        /// <param name="dataTableName">指定直接[物理]删除的数据表名称</param>
        /// <param name="data">插入数据时附带的数据</param>
        public static void DeleteData<ParameterDataType>(string dataTableName, ParameterDataType data)
        {
            switch (dataTableName)
            {
                case Tables.AccountInfo:
                    BroadgameDBTool.SetCommandText("");
                    break;
                case Tables.UserInfo:
                    BroadgameDBTool.SetCommandText("");
                    break;
            }
        }

        /// <summary>
        /// 逻辑删除指定表中的数据(对数据库中的对应的数据表中已存在的数据做一个逻辑删除操作)
        /// </summary>
        /// <param name="dataTableName">指定逻辑删除的数据表名称</param>
        /// <param name="data">插入数据时附带的数据</param>
        public static void LogicDeleteData<ParameterDataType>(string dataTableName, ParameterDataType data)
        {
            switch (dataTableName)
            {
                case Tables.AccountInfo:
                    BroadgameDBTool.SetCommandText("");
                    break;
                case Tables.UserInfo:
                    BroadgameDBTool.SetCommandText("");
                    break;
            }
        }

        /// <summary>
        /// 执行普通数据库操作命令
        /// </summary>
        public static void NoramlExecute()
        {
            //1.构建数据库执行命令的对象
            using (command = new SqlCommand(sqlCommandText, connection))
            {
                //2.校验参数列表是否为空 且 包含的参数元素是否不为0
                if (parameters != null && parameters.Count > 0)//如果教程成功
                    command.Parameters.AddRange(parameters.ToArray());//将参数列表添加至命令对象的参数列表中
                //3.取得执行普通命令时的执行记录结果
                int recordResult = command.ExecuteNonQuery();
                LogMessage.Instance.SetLogMessage("执行完毕普通数据库操作命令所受影响的行数：" + recordResult.ToString() + " .");
                //4.执行完毕后 将参数列表清空
                parameters.Clear();
            }
        }

        /// <summary>
        /// 执行单查询操作(也就是相当于查询一条数据保存到一个object中,将保存的数据返回给上层进行处理)
        /// </summary>
        /// <returns></returns>
        public static object SingleQuery()
        {
            return new object();
        }

        /// <summary>
        /// 执行数据表查询操作(将查询完毕的数据保存到数据表中,将保存的数据返回给上层进行处理)
        /// </summary>
        /// <returns></returns>
        public static DataTable DataTableQuery()
        {
            return new DataTable();
        }

        /// <summary>
        /// 执行数据读取器查询操作(将查询完毕的数据保存到数据读取器中,将保存的数据返回给上层进行处理)
        /// </summary>
        public static void DataReaderQuery()
        {

        }

        /// <summary>
        /// 多条件查询
        /// </summary>
        public static void MuitlConditionalQuery()
        {

        }
    }
}
