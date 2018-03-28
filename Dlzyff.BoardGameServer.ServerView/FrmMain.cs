using Dlzyff.BoardGame.BottomServer.Applications;
using Dlzyff.BoardGame.BottomServer.Peers;
using Dlzyff.BoardGameServer.Dao.Tools;
using Dlzyff.BoardGameServer.Log;
using Dlzyff.BoardGameServer.ServerView.Centers;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace Dlzyff.BoardGameServer.ServerView
{
    /// <summary>
    /// 服务端前端主窗体
    /// </summary>
    public partial class FrmMain : Form
    {
        /// <summary>
        /// 添加消息委托对象
        /// </summary>
        private Action addMessageAction = null;
        /// <summary>
        /// 服务端对象
        /// </summary>
        private ServerPeer serverPeer = new ServerPeer();
        /// <summary>
        /// 网络消息转发中心对象
        /// </summary>
        private IApplicationBase netMsgCenterApp = new NetMessageCenter();

        /// <summary>
        /// 构造方法
        /// </summary>
        public FrmMain()
        {
            this.InitializeComponent();
            LogMessage.Instance.AddMessageEvent += this.AddLogMessageEvent;
            this.btnCloseServer.Enabled = false;
        }

        /// <summary>
        /// 添加日志消息的回调方法
        /// </summary>
        /// <param name="message"></param>
        private void AddLogMessageEvent(string message)
        {
            this.addMessageAction = () =>
            {
                this.logMessageList.Items.Add(message);
            };
            this.Invoke(this.addMessageAction);
        }

        /// <summary>
        /// 开启服务的按钮点击事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnStartServer_Click(object sender, EventArgs e)
        {
            this.StartServer();
            this.btnCloseServer.Enabled = true;
        }

        /// <summary>
        /// 开启服务端程序(也就是启动服务端程序)
        /// </summary>
        private void StartServer()
        {
            this.serverPeer.SetApplication(this.netMsgCenterApp);
            this.serverPeer.StartServer(6666, 10);
            BroadgameDBTool.SetConnectStr("这儿填写数据库连接字符串");//可以从配置文件中读取数据库连接字符串
            this.logMessageList.Items.Add("服务器启动成功~");
            this.logMessageList.Items.Add("等待客户端对象的连接~");
            this.btnStartServer.Enabled = false;
        }

        /// <summary>
        /// 关闭服务端程序(附加功能:重启服务端程序)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCloseServer_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("要重新启动嘛？", "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.serverPeer.Close();
                Application.Exit();
                Process.Start(Assembly.GetExecutingAssembly().Location);
                this.StartPosition = FormStartPosition.WindowsDefaultLocation;

            }
        }
    }
}
