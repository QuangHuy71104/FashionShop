using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FashionShop.BLL;
using FashionShop.DTO;

namespace FashionShop.GUI
{
    public partial class FrmAccounts : Form
    {
        private readonly AccountService service = new AccountService();
        private readonly Account current;

        private DataGridView dgv;
        private TextBox txtUser, txtPass, txtSearch;
        private ComboBox cboStaff;
        private Button btnAdd, btnDel, btnReload, btnClearSearch;
        private Label lblCount;

        private DataTable accountsTable;
        private DataView accountsView;
        private readonly string hintSearch = "Search staff username...";

        public FrmAccounts(Account acc)
        {
            current = acc;

            Text = "Manage Staff Accounts";
            Size = new Size(720, 520);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10f);
            MinimumSize = new Size(720, 520);

            // Designer có thể vẫn tồn tại nhưng không bắt buộc gọi,
            // gọi cũng không sao vì chúng ta không tự viết InitializeComponent ở đây.
            // Nếu designer file bạn vẫn có, dòng này OK.
            InitializeComponent();

            BuildUI();
            ApplyRolePermission();
            Load += FrmAccounts_Load;
        }

        // HÀM LOAD ĐÚNG TÊN để Designer không báo CS1061
        private void FrmAccounts_Load(object sender, EventArgs e)
        {
            LoadStaffCombo();
            LoadGrid();
        }


        // ===== Header highlight colors =====
        private readonly Color HeaderBackNormal = Color.FromArgb(245, 245, 245);
        private readonly Color HeaderForeNormal = Color.Black;

        private readonly Color HeaderBackActive = Color.FromArgb(33, 150, 243);
        private readonly Color HeaderForeActive = Color.White;

        // highlight header theo cột đang đứng
        private void HighlightCurrentColumnHeader(DataGridView grid)
        {
            if (grid == null || grid.Columns.Count == 0) return;

            // reset all headers
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.HeaderCell.Style.BackColor = HeaderBackNormal;
                col.HeaderCell.Style.ForeColor = HeaderForeNormal;
                col.HeaderCell.Style.Font = new Font("Segoe UI Semibold", 10f);
            }

            var cur = grid.CurrentCell;
            if (cur == null) return;

            var activeCol = grid.Columns[cur.ColumnIndex];
            activeCol.HeaderCell.Style.BackColor = HeaderBackActive;
            activeCol.HeaderCell.Style.ForeColor = HeaderForeActive;
        }


        private void BuildUI()
        {
            Controls.Clear();

            // ===== Root SplitContainer giống Customers =====
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                FixedPanel = FixedPanel.None,
                IsSplitterFixed = false,
                BackColor = Color.White
            };
            Controls.Add(split);

            // ================= LEFT: Input =================
            split.Panel1.Padding = new Padding(12);

            var gbInput = new GroupBox
            {
                Text = "Account Information",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 10.5f),
                Padding = new Padding(12)
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 0,
                Padding = new Padding(6),
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int _row = 0;
            void AddRow(string label, Control control)
            {
                tbl.RowCount = _row + 1;
                tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

                var lb = new Label
                {
                    Text = label,
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                control.Dock = DockStyle.Fill;
                control.Margin = new Padding(0, 2, 0, 2);

                tbl.Controls.Add(lb, 0, _row);
                tbl.Controls.Add(control, 1, _row);
                _row++;
            }

            // === input controls ===
            cboStaff = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            txtUser = new TextBox();
            txtPass = new TextBox { UseSystemPasswordChar = true };

            AddRow("Staff employee", cboStaff);
            AddRow("Username", txtUser);
            AddRow("Password", txtPass);

            // ===== Buttons grid giống Customers =====
            var btnGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 105,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(6),
                Margin = new Padding(0, 12, 0, 0)
            };
            btnGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            btnGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            btnGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            btnGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            btnAdd = MakeButton("Create Staff", Color.FromArgb(33, 150, 243));
            btnDel = MakeButton("Delete Staff", Color.FromArgb(244, 67, 54));
            btnReload = MakeButton("Reload", Color.FromArgb(96, 125, 139));

            btnAdd.Dock = DockStyle.Fill;
            btnDel.Dock = DockStyle.Fill;
            btnReload.Dock = DockStyle.Fill;

            btnAdd.Margin = new Padding(6);
            btnDel.Margin = new Padding(6);
            btnReload.Margin = new Padding(6);

            // bố trí 3 nút (ô còn lại để trống cho cân)
            btnGrid.Controls.Add(btnAdd, 0, 0);
            btnGrid.Controls.Add(btnDel, 1, 0);
            btnGrid.Controls.Add(btnReload, 0, 1);

            // stack trái
            var leftLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0),
            };
            leftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            leftLayout.Controls.Add(tbl, 0, 0);
            leftLayout.Controls.Add(btnGrid, 0, 1);

            gbInput.Controls.Clear();
            gbInput.Controls.Add(leftLayout);
            split.Panel1.Controls.Add(gbInput);

            // ================= RIGHT: Search + Grid =================
            split.Panel2.Padding = new Padding(12);

            var pnlSearch = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 43,
                ColumnCount = 2,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            pnlSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 46));

            txtSearch = new TextBox
            {
                Height = 45,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12f),
                Margin = new Padding(0, 6, 4, 6)
            };

            btnClearSearch = MakeButton("", Color.FromArgb(244, 67, 54));
            btnClearSearch.Dock = DockStyle.Fill;
            btnClearSearch.Margin = new Padding(0, 6, 0, 6);

            pnlSearch.Controls.Add(txtSearch, 0, 0);
            pnlSearch.Controls.Add(btnClearSearch, 1, 0);

            // icon đỏ giống các form kia
            btnClearSearch.Image = ResizeImage(Properties.Resources.delete, 18, 18);
            btnClearSearch.ImageAlign = ContentAlignment.MiddleCenter;
            btnClearSearch.TextImageRelation = TextImageRelation.Overlay;
            btnClearSearch.Padding = new Padding(0);

            // placeholder
            txtSearch.Text = hintSearch;
            txtSearch.ForeColor = Color.Gray;

            txtSearch.GotFocus += (s, e) =>
            {
                if (txtSearch.Text == hintSearch)
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = Color.Black;
                }
            };
            txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = hintSearch;
                    txtSearch.ForeColor = Color.Gray;
                }
            };

            // live search
            txtSearch.TextChanged += (s, e) =>
            {
                if (txtSearch.Focused) ApplySearch();
            };

            // clear search
            btnClearSearch.Click += (s, e) =>
            {
                txtSearch.Text = hintSearch;
                txtSearch.ForeColor = Color.Gray;
                if (accountsView != null) accountsView.RowFilter = "";
            };

            // grid
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };
            StyleGrid(dgv);
            dgv.SelectionChanged += (s, e) => HighlightCurrentColumnHeader(dgv);
            dgv.CellEnter += (s, e) => HighlightCurrentColumnHeader(dgv);
            dgv.ColumnHeaderMouseClick += (s, e) => HighlightCurrentColumnHeader(dgv);

            // khi bind data xong cũng highlight luôn
            dgv.DataBindingComplete += (s, e) => HighlightCurrentColumnHeader(dgv);


            split.Panel2.Controls.Add(dgv);
            split.Panel2.Controls.Add(pnlSearch);

            // set tỉ lệ trái/phải giống Customers
            Shown += (s, e) =>
            {
                split.Panel1MinSize = 280;
                split.Panel2MinSize = 450;

                int desiredLeft = 330;
                int maxLeft = split.Width - split.Panel2MinSize;
                split.SplitterDistance = Math.Max(split.Panel1MinSize,
                                          Math.Min(desiredLeft, maxLeft));
            };

            // ======= gắn lại event logic cũ =======
            btnAdd.Click += BtnAdd_Click;
            btnDel.Click += BtnDel_Click;
            btnReload.Click += (s, e) =>
            {
                LoadStaffCombo();
                LoadGrid();
                ClearForm();
            };
        }


        private void LoadStaffCombo()
        {
            var dt = service.GetStaffEmployeesForCombo();
            cboStaff.DataSource = dt;
            cboStaff.DisplayMember = "employee_name";
            cboStaff.ValueMember = "employee_id";
            cboStaff.SelectedIndex = dt.Rows.Count > 0 ? 0 : -1;
        }

        private void LoadGrid()
        {
            accountsTable = service.GetStaffAccounts();
            accountsView = accountsTable.DefaultView;
            dgv.DataSource = accountsView;

            ApplySearch();
            HighlightCurrentColumnHeader(dgv);

        }

        private void ApplySearch()
        {
            if (accountsView == null) return;

            string key = txtSearch.Text.Trim();
            if (key == hintSearch) key = "";
            accountsView.RowFilter = "";

            if (!string.IsNullOrWhiteSpace(key))
            {
                key = key.Replace("'", "''");
                accountsView.RowFilter =
                    $"CONVERT([username], 'System.String') LIKE '%{key}%'";
            }
        }

        private void ApplyRolePermission()
        {
            bool isAdmin = current != null &&
                           current.Role != null &&
                           current.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            btnAdd.Enabled = isAdmin;
            btnDel.Enabled = isAdmin;
            txtUser.ReadOnly = !isAdmin;
            txtPass.ReadOnly = !isAdmin;
            cboStaff.Enabled = isAdmin;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPass.Text.Trim();
            int employeeId = cboStaff.SelectedIndex >= 0
                ? Convert.ToInt32(cboStaff.SelectedValue)
                : 0;

            if (service.CreateStaffAccount(username, password, employeeId, out string err))
            {
                MessageBox.Show("Created staff account!");
                LoadGrid();
                ClearForm();
            }
            else MessageBox.Show(err);
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null)
            {
                MessageBox.Show("Please choose a staff account first.");
                return;
            }

            int accountId = Convert.ToInt32(dgv.CurrentRow.Cells["account_id"].Value);
            string username = dgv.CurrentRow.Cells["username"].Value?.ToString();

            var confirm = MessageBox.Show(
                $"Delete staff account '{username}'?",
                "Confirm delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm == DialogResult.Yes)
            {
                if (service.DeleteStaffAccount(accountId, out string err))
                {
                    MessageBox.Show("Deleted!");
                    LoadGrid();
                }
                else MessageBox.Show(err);
            }
        }

        private void ClearForm()
        {
            txtUser.Clear();
            txtPass.Clear();
        }

        private Button MakeButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                Width = 135,
                Height = 32,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(6)
            };
        }

        private void StyleGrid(DataGridView g)
        {
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10f);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;

            g.AllowUserToResizeColumns = false;
            g.AllowUserToResizeRows = false;
            g.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            g.RowTemplate.Height = 30;
            g.ColumnHeadersHeight = 36;

            g.DefaultCellStyle.Font = new Font("Segoe UI", 10f);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(33, 150, 243);
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
        }

        private Image ResizeImage(Image img, int w, int h)
        {
            var bmp = new Bitmap(w, h);
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.DrawImage(img, 0, 0, w, h);
            }
            return bmp;
        }

    }
}
