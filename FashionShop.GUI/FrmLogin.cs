using System;
using System.Drawing;
using System.Windows.Forms;
using FashionShop.BLL;
using FashionShop.DTO;

namespace FashionShop.GUI
{
    public class FrmLogin : Form
    {
        TextBox txtUser, txtPass;
        Button btnLogin, btnExit;
        AuthService auth = new AuthService();

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FrmLogin
            // 
            this.ClientSize = new System.Drawing.Size(274, 229);
            this.Name = "FrmLogin";
            this.Load += new System.EventHandler(this.FrmLogin_Load);
            this.ResumeLayout(false);

        }

        private void FrmLogin_Load(object sender, EventArgs e)
        {

        }

        public FrmLogin()
        {
            Text = "Fashion Shop - Login";
            Size = new Size(420, 260);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var lblTitle = new Label
            {
                Text = "LOGIN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(160, 15)
            };

            var lblUser = new Label { Text = "Username:", AutoSize = true, Location = new Point(40, 70) };
            var lblPass = new Label { Text = "Password:", AutoSize = true, Location = new Point(40, 110) };

            txtUser = new TextBox { Location = new Point(140, 65), Width = 220 };
            txtPass = new TextBox { Location = new Point(140, 105), Width = 220, PasswordChar = '*' };

            btnLogin = new Button { Text = "Login", Location = new Point(140, 155), Width = 100 };
            btnExit = new Button { Text = "Exit", Location = new Point(260, 155), Width = 100 };

            btnLogin.Click += BtnLogin_Click;
            btnExit.Click += (s, e) => Application.Exit();

            Controls.AddRange(new Control[] { lblTitle, lblUser, lblPass, txtUser, txtPass, btnLogin, btnExit });
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            Account acc = auth.Login(txtUser.Text.Trim(), txtPass.Text, out string err);
            if (acc == null)
            {
                MessageBox.Show(err, "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Hide();
            var main = new FrmMain(acc);
            main.FormClosed += (s, ev) => Close();
            main.Show();
        }
    }
}
