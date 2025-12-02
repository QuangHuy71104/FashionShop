using System;
using System.Drawing;
using System.Windows.Forms;
using FashionShop.BLL;
using FashionShop.DTO;

namespace FashionShop.GUI
{
    public class FrmCustomers : Form
    {
        CustomerService service = new CustomerService();

        DataGridView dgv;
        TextBox txtId, txtName, txtPhone, txtEmail, txtAddress, txtPoints, txtSearch;
        Button btnAdd, btnUpd, btnDel, btnReload, btnSearch;

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FrmCustomers
            // 
            this.ClientSize = new System.Drawing.Size(274, 229);
            this.Name = "FrmCustomers";
            this.Load += new System.EventHandler(this.FrmCustomers_Load);
            this.ResumeLayout(false);

        }

        private void FrmCustomers_Load(object sender, EventArgs e)
        {

        }

        public FrmCustomers()
        {
            InitializeUI();
            Load += (s, e) => LoadGrid();
        }

        private void InitializeUI()
        {
            Text = "Customers Management";
            Size = new Size(1050, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);
            BackColor = Color.WhiteSmoke;

            // ===== Split layout =====
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 340,
                BackColor = Color.WhiteSmoke
            };
            Controls.Add(split);

            // ===== LEFT: Form panel =====
            var leftWrap = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                BackColor = Color.WhiteSmoke
            };
            split.Panel1.Controls.Add(leftWrap);

            var gbForm = new GroupBox
            {
                Text = "Customer Information",
                Dock = DockStyle.Top,
                Height = 310,
                Padding = new Padding(12),
                Font = new Font("Segoe UI Semibold", 10),
                BackColor = Color.White
            };
            leftWrap.Controls.Add(gbForm);

            var formTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                AutoSize = true
            };
            formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            for (int i = 0; i < 6; i++)
                formTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            gbForm.Controls.Add(formTable);

            // Inputs
            txtId = MakeTextBox(readOnly: true);
            txtName = MakeTextBox();
            txtPhone = MakeTextBox();
            txtEmail = MakeTextBox();
            txtAddress = MakeTextBox();
            txtPoints = MakeTextBox();

            formTable.Controls.Add(MakeLabel("Id"), 0, 0);
            formTable.Controls.Add(txtId, 1, 0);

            formTable.Controls.Add(MakeLabel("Name"), 0, 1);
            formTable.Controls.Add(txtName, 1, 1);

            formTable.Controls.Add(MakeLabel("Phone"), 0, 2);
            formTable.Controls.Add(txtPhone, 1, 2);

            formTable.Controls.Add(MakeLabel("Email"), 0, 3);
            formTable.Controls.Add(txtEmail, 1, 3);

            formTable.Controls.Add(MakeLabel("Address"), 0, 4);
            formTable.Controls.Add(txtAddress, 1, 4);

            formTable.Controls.Add(MakeLabel("Points"), 0, 5);
            formTable.Controls.Add(txtPoints, 1, 5);

            // Buttons area
            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 120,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 12, 0, 0),
                BackColor = Color.Transparent
            };
            leftWrap.Controls.Add(btnPanel);

            btnAdd = MakeButton("Add", Color.FromArgb(46, 204, 113));
            btnUpd = MakeButton("Update", Color.FromArgb(52, 152, 219));
            btnDel = MakeButton("Delete", Color.FromArgb(231, 76, 60));
            btnReload = MakeButton("Reload", Color.FromArgb(149, 165, 166));

            btnPanel.Controls.AddRange(new Control[] { btnAdd, btnUpd, btnDel, btnReload });

            // Events (giữ logic cũ)
            btnAdd.Click += (s, e) =>
            {
                var c = ReadForm();
                if (service.Add(c, out string err))
                {
                    MessageBox.Show("Added!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadGrid();
                    ClearForm();
                }
                else MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            btnUpd.Click += (s, e) =>
            {
                var c = ReadForm();
                if (service.Update(c, out string err))
                {
                    MessageBox.Show("Updated!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadGrid();
                }
                else MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            btnDel.Click += (s, e) =>
            {
                if (!int.TryParse(txtId.Text, out int id)) return;
                if (MessageBox.Show("Delete this customer?", "Confirm",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    service.Delete(id);
                    LoadGrid();
                    ClearForm();
                }
            };

            btnReload.Click += (s, e) => { LoadGrid(); ClearForm(); };

            // ===== RIGHT: Search + Grid =====
            var rightWrap = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                BackColor = Color.WhiteSmoke
            };
            split.Panel2.Controls.Add(rightWrap);

            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.White,
                Padding = new Padding(10),
            };
            rightWrap.Controls.Add(searchPanel);

            txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Width = 320,
                Location = new Point(10, 12)
            };
            SetPlaceholder(txtSearch, "Search by name / phone / email...");

            btnSearch = MakeButton("Search", Color.FromArgb(155, 89, 182));
            btnSearch.Location = new Point(340, 8);
            btnSearch.Height = 34;
            btnSearch.Width = 95;

            btnSearch.Click += (s, e) =>
            {
                var key = txtSearch.ForeColor == Color.Gray ? "" : txtSearch.Text.Trim();
                dgv.DataSource = service.Search(key);
            };

            searchPanel.Controls.AddRange(new Control[] { txtSearch, btnSearch });

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
            };
            rightWrap.Controls.Add(dgv);

            StyleGrid(dgv);

            dgv.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var r = dgv.Rows[e.RowIndex];
                txtId.Text = r.Cells["customer_id"].Value?.ToString();
                txtName.Text = r.Cells["customer_name"].Value?.ToString();
                txtPhone.Text = r.Cells["phone"].Value?.ToString();
                txtEmail.Text = r.Cells["email"].Value?.ToString();
                txtAddress.Text = r.Cells["address"].Value?.ToString();
                txtPoints.Text = r.Cells["points"].Value?.ToString();
            };
        }

        // ===== Placeholder custom (cho .NET Framework) =====
        void SetPlaceholder(TextBox tb, string hint)
        {
            tb.Text = hint;
            tb.ForeColor = Color.Gray;

            tb.GotFocus += (s, e) =>
            {
                if (tb.Text == hint)
                {
                    tb.Text = "";
                    tb.ForeColor = Color.Black;
                }
            };

            tb.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = hint;
                    tb.ForeColor = Color.Gray;
                }
            };
        }

        // ===== Helpers =====
        void LoadGrid() => dgv.DataSource = service.GetAll();

        Customer ReadForm()
        {
            int.TryParse(txtId.Text, out int id);
            int.TryParse(txtPoints.Text, out int pts);
            return new Customer
            {
                Id = id,
                Name = txtName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                Points = pts
            };
        }

        void ClearForm()
        {
            txtId.Clear();
            txtName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
            txtPoints.Clear();
        }

        Label MakeLabel(string text) => new Label
        {
            Text = text + ":",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(44, 62, 80)
        };

        TextBox MakeTextBox(bool readOnly = false) => new TextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = readOnly,
            Font = new Font("Segoe UI", 10),
            BackColor = readOnly ? Color.FromArgb(245, 246, 250) : Color.White
        };

        Button MakeButton(string text, Color backColor) => new Button
        {
            Text = text,
            AutoSize = false,
            Width = 95,
            Height = 38,
            Margin = new Padding(6),
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 10),
            Cursor = Cursors.Hand
        };

        void StyleGrid(DataGridView grid)
        {
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10);
            grid.ColumnHeadersHeight = 36;

            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(236, 240, 241);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;

            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(230, 230, 230);
        }
    }
}
