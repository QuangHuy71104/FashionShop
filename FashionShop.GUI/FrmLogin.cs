using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FashionShop.BLL;
using FashionShop.DTO;

namespace FashionShop.GUI
{
    public class FrmLogin : Form
    {
        // ====== controls ======
        private RoundedPanel card;
        private Label lblApp, lblTitle, lblUser, lblPass;
        private HintTextBox txtUser, txtPass;
        private GradientButton btnLogin, btnExit;
        //private Button btnExit;
        private LinkLabel lnkForgot, lnkSignup;
        private CheckBox chkShowPass;

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

        private AuthService auth = new AuthService();

        public FrmLogin()
        {
            Text = "Fashion Shop - Login";
            Size = new Size(420, 620);
            MinimumSize = new Size(420, 620);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(44, 62, 80);
            DoubleBuffered = true;

            BuildUI();

            this.Resize += (s, e) => ReLayout();
            ReLayout();
        }


        private void ReLayout()
        {
            lblApp.Left = (ClientSize.Width - lblApp.Width) / 2;

            card.Left = (ClientSize.Width - card.Width) / 2;
            card.Top = 120;
        }



        private void BuildUI()
        {
            // ===== app title (MEDDU style) =====
            lblApp = new Label
            {
                Text = "FASHION SHOP",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblApp.Left = (ClientSize.Width - lblApp.Width) / 2;
            Controls.Add(lblApp);

            // ===== white card =====
            card = new RoundedPanel
            {
                Size = new Size(340, 380),
                Radius = 24,
                BackColor = Color.White,
                Location = new Point((ClientSize.Width - 340) / 2, 140),
                Shadow = true
            };
            Controls.Add(card);

            lblTitle = new Label
            {
                Text = "LOGIN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 122, 199),
                AutoSize = true,
                Location = new Point(0, 24),
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblTitle.Left = (card.Width - lblTitle.Width) / 2;
            card.Controls.Add(lblTitle);

            // ===== username =====
            lblUser = new Label
            {
                Text = "Username",
                AutoSize = true,
                ForeColor = Color.Gray,
                Location = new Point(30, 90)
            };
            card.Controls.Add(lblUser);

            txtUser = new HintTextBox
            {
                Hint = "Username",
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.None,
                Width = 280,
                Location = new Point(30, 115)
            };
            card.Controls.Add(txtUser);

            var lineUser = new Panel
            {
                BackColor = Color.Silver,
                Size = new Size(280, 1),
                Location = new Point(30, 140)
            };
            card.Controls.Add(lineUser);

            // ===== password =====
            lblPass = new Label
            {
                Text = "Password",
                AutoSize = true,
                ForeColor = Color.Gray,
                Location = new Point(30, 170)
            };
            card.Controls.Add(lblPass);

            txtPass = new HintTextBox
            {
                Hint = "••••••••",
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.None,
                Width = 240,
                Location = new Point(30, 195),
                UseSystemPasswordChar = true
            };
            card.Controls.Add(txtPass);

            chkShowPass = new CheckBox
            {
                Text = "",
                AutoSize = false,
                Size = new Size(18, 18),
                ForeColor = Color.Gray
            };

            chkShowPass.CheckedChanged += (s, e) =>
            {
                txtPass.UseSystemPasswordChar = !chkShowPass.Checked;
            };
            card.Controls.Add(chkShowPass);

            var linePass = new Panel
            {
                BackColor = Color.Silver,
                Size = new Size(280, 1),
                Location = new Point(30, 220)
            };
            card.Controls.Add(linePass);

            // canh ngay trên thanh line
            chkShowPass.Left = linePass.Left + linePass.Width - chkShowPass.Width - 1 ;
            chkShowPass.Top = linePass.Top - chkShowPass.Height / 2 - 12;

            // ===== forgot password =====
            lnkForgot = new LinkLabel
            {
                Text = "Forget your password?",
                AutoSize = true,
                LinkColor = Color.FromArgb(60, 160, 220),
                ActiveLinkColor = Color.FromArgb(30, 122, 199),
                VisitedLinkColor = Color.FromArgb(60, 160, 220),
                Top = 235
            };

            // canh phải sau khi đã Add (để có Width chính xác)
            lnkForgot.Left = card.Width - lnkForgot.Width - 30; // 30 = padding phải

            lnkForgot.Click += (s, e) =>
            {
                MessageBox.Show("Login failed. Please contact the Admin.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            card.Controls.Add(lnkForgot);

            // ===== login gradient button =====
            btnLogin = new GradientButton
            {
                Text = "LOGIN",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(220, 52),
                Location = new Point((card.Width - 220) / 2, 260),
                Radius = 26
            };
            btnLogin.Click += BtnLogin_Click;
            card.Controls.Add(btnLogin);

            // ===== exit small button =====
            btnExit = new GradientButton
            {
                Text = "Exit",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.White,
                Size = new Size(80, 30),
                Location = new Point((card.Width - 80) / 2, 325),

                Radius = 12,                 // vẫn bo
                UseGradient = false,         // không dùng gradient
                StartColor = Color.Gray      // màu xám như cũ
            };
            btnExit.Click += (s, e) => Application.Exit();
            card.Controls.Add(btnExit);


            // ===== signup link bottom =====
            lnkSignup = new LinkLabel
            {
                //Text = "Don't have an account? Sign up",
                AutoSize = true,
                LinkColor = Color.FromArgb(60, 160, 220),
                ActiveLinkColor = Color.FromArgb(30, 122, 199),
                VisitedLinkColor = Color.FromArgb(60, 160, 220),
                Location = new Point(0, 365),
                TextAlign = ContentAlignment.MiddleCenter
            };
            lnkSignup.Left = (card.Width - lnkSignup.Width) / 2;
            lnkSignup.Click += (s, e) =>
            {
                MessageBox.Show("Chức năng đăng ký chưa implement.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            card.Controls.Add(lnkSignup);

            // enter/esc
            AcceptButton = btnLogin;
            CancelButton = btnExit;
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

    // =================== CUSTOM CONTROLS ===================

    // Panel bo góc + shadow
    public class RoundedPanel : Panel
    {
        public int Radius { get; set; } = 20;
        public bool Shadow { get; set; } = false;

        public RoundedPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = ClientRectangle;
            rect.Inflate(-1, -1);

            using (GraphicsPath path = GetRoundRect(rect, Radius))
            {
                if (Shadow)
                {
                    using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(25, 0, 0, 0)))
                    {
                        Rectangle shadowRect = rect;
                        shadowRect.Offset(0, 6);

                        using (GraphicsPath shadowPath = GetRoundRect(shadowRect, Radius))
                        {
                            e.Graphics.FillPath(shadowBrush, shadowPath);
                        }
                    }
                }

                using (SolidBrush brush = new SolidBrush(BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }
            }

            base.OnPaint(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (Radius <= 0) Region = null;
            else Region = new Region(GetRoundRect(ClientRectangle, Radius));
        }


        private GraphicsPath GetRoundRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();

            if (r.Width <= 0 || r.Height <= 0)
                return path;

            if (radius <= 0)
            {
                path.AddRectangle(r);
                path.CloseFigure();
                return path;
            }

            int maxRadius = Math.Min(r.Width, r.Height) / 2;
            if (radius > maxRadius) radius = maxRadius;

            int d = radius * 2;

            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

    }

    // TextBox có hint (placeholder)
    public class HintTextBox : TextBox
    {
        private string _hint = "";
        public string Hint
        {
            get => _hint;
            set
            {
                _hint = value;
                if (!Focused) SetHint();   // refresh lại hint khi đổi
            }
        }

        private bool isHint = true;
        private Color realFore;

        public HintTextBox()
        {
            realFore = ForeColor;

            this.Enter += (s, e) => RemoveHint();
            this.Leave += (s, e) => SetHint();
            this.KeyDown += (s, e) =>
            {
                if (isHint) RemoveHint();
            };

            SetHint();
        }

        private void SetHint()
        {
            if (string.IsNullOrWhiteSpace(base.Text))
            {
                isHint = true;
                base.Text = Hint;
                ForeColor = Color.Silver;

                // nếu là password hint thì tắt mask để thấy hint
                if (Hint.Contains("•") || Hint.Contains("*"))
                    UseSystemPasswordChar = false;
            }
        }

        private void RemoveHint()
        {
            if (isHint)
            {
                isHint = false;
                base.Text = "";
                ForeColor = realFore;

                // bật lại mask cho password khi user nhập
                if (Hint.Contains("•") || Hint.Contains("*"))
                    UseSystemPasswordChar = true;
            }
        }

        public override string Text
        {
            get => isHint ? "" : base.Text;
            set
            {
                isHint = false;
                base.Text = value;
                ForeColor = realFore;
            }
        }
    }



    // Button gradient bo tròn
    public class GradientButton : Button
    {
        public int Radius { get; set; } = 20;

        // NEW: cho phép đổi màu
        public bool UseGradient { get; set; } = true;
        public Color StartColor { get; set; } = Color.FromArgb(67, 132, 244);
        public Color EndColor { get; set; } = Color.FromArgb(86, 208, 253);

        public GradientButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            BackColor = Color.White;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = ClientRectangle;
            rect.Inflate(-1, -1);

            // đổ nền theo parent để không lòi viền đen
            Color parentBack = Parent?.BackColor ?? Color.White;
            using (SolidBrush bg = new SolidBrush(parentBack))
                e.Graphics.FillRectangle(bg, ClientRectangle);

            using (GraphicsPath path = GetRoundRect(rect, Radius))
            {
                if (UseGradient)
                {
                    using (LinearGradientBrush lg = new LinearGradientBrush(
                        rect, StartColor, EndColor, LinearGradientMode.Horizontal))
                    {
                        e.Graphics.FillPath(lg, path);
                    }
                }
                else
                {
                    using (SolidBrush sb = new SolidBrush(StartColor)) // dùng StartColor làm màu nền
                    {
                        e.Graphics.FillPath(sb, path);
                    }
                }
            }

            TextRenderer.DrawText(
                e.Graphics, Text, Font, rect, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent) { }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Region = Radius <= 0 ? null : new Region(GetRoundRect(ClientRectangle, Radius));
        }

        private GraphicsPath GetRoundRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }


}
