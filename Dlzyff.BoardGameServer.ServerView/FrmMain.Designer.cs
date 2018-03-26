namespace Dlzyff.BoardGameServer.ServerView
{
    partial class FrmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnStartServer = new System.Windows.Forms.Button();
            this.btnCloseServer = new System.Windows.Forms.Button();
            this.btnBroadcastMessage = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.logMessageList = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnStartServer
            // 
            this.btnStartServer.Font = new System.Drawing.Font("微软雅黑", 14F);
            this.btnStartServer.Location = new System.Drawing.Point(906, 12);
            this.btnStartServer.Name = "btnStartServer";
            this.btnStartServer.Size = new System.Drawing.Size(202, 43);
            this.btnStartServer.TabIndex = 0;
            this.btnStartServer.Text = "开启服务";
            this.btnStartServer.UseVisualStyleBackColor = true;
            this.btnStartServer.Click += new System.EventHandler(this.BtnStartServer_Click);
            // 
            // btnCloseServer
            // 
            this.btnCloseServer.Font = new System.Drawing.Font("微软雅黑", 14F);
            this.btnCloseServer.Location = new System.Drawing.Point(906, 77);
            this.btnCloseServer.Name = "btnCloseServer";
            this.btnCloseServer.Size = new System.Drawing.Size(202, 43);
            this.btnCloseServer.TabIndex = 1;
            this.btnCloseServer.Text = "关闭服务";
            this.btnCloseServer.UseVisualStyleBackColor = true;
            this.btnCloseServer.Click += new System.EventHandler(this.BtnCloseServer_Click);
            // 
            // btnBroadcastMessage
            // 
            this.btnBroadcastMessage.Font = new System.Drawing.Font("微软雅黑", 14F);
            this.btnBroadcastMessage.Location = new System.Drawing.Point(906, 631);
            this.btnBroadcastMessage.Name = "btnBroadcastMessage";
            this.btnBroadcastMessage.Size = new System.Drawing.Size(202, 38);
            this.btnBroadcastMessage.TabIndex = 2;
            this.btnBroadcastMessage.Text = "广播消息";
            this.btnBroadcastMessage.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(4, 631);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(896, 38);
            this.textBox1.TabIndex = 3;
            // 
            // logMessageList
            // 
            this.logMessageList.Font = new System.Drawing.Font("楷体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.logMessageList.FormattingEnabled = true;
            this.logMessageList.ItemHeight = 19;
            this.logMessageList.Location = new System.Drawing.Point(4, 125);
            this.logMessageList.Name = "logMessageList";
            this.logMessageList.Size = new System.Drawing.Size(1104, 498);
            this.logMessageList.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 15F);
            this.label1.Location = new System.Drawing.Point(424, 93);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 27);
            this.label1.TabIndex = 5;
            this.label1.Text = "日志消息列表";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.GhostWhite;
            this.ClientSize = new System.Drawing.Size(1120, 681);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.logMessageList);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnBroadcastMessage);
            this.Controls.Add(this.btnCloseServer);
            this.Controls.Add(this.btnStartServer);
            this.Font = new System.Drawing.Font("微软雅黑", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "卓越非凡棋牌服务端";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStartServer;
        private System.Windows.Forms.Button btnCloseServer;
        private System.Windows.Forms.Button btnBroadcastMessage;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ListBox logMessageList;
        private System.Windows.Forms.Label label1;
    }
}