using System;
using System.Windows.Forms;

namespace MediaPlayer
{
    /// <summary>
    /// 简单输入对话框 - 用于获取用户输入的URL地址
    /// </summary>
    public partial class InputDialog : Form
    {
        private readonly TextBox txtInput;
        private readonly Button btnOk;
        private readonly Button btnCancel;

        public string InputText => txtInput.Text;

        public InputDialog(string title, string prompt, string defaultValue = "")
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new System.Drawing.Size(450, 150);

            var lblPrompt = new Label
            {
                Text = prompt,
                Location = new System.Drawing.Point(12, 15),
                Size = new System.Drawing.Size(420, 30)
            };

            txtInput = new TextBox
            {
                Text = defaultValue,
                Location = new System.Drawing.Point(12, 50),
                Size = new System.Drawing.Size(420, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnOk = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(270, 85),
                Size = new System.Drawing.Size(75, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(355, 85),
                Size = new System.Drawing.Size(75, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            Controls.AddRange(new Control[] { lblPrompt, txtInput, btnOk, btnCancel });
            AcceptButton = btnOk;
            CancelButton = btnCancel;

            txtInput.Select();
            txtInput.SelectAll();
        }

        private void InitializeComponent()
        {
            // 设计器自动生成的方法 - 此处留空，已在构造函数中完成初始化
        }
    }
}
