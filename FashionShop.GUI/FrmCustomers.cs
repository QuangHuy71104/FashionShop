using System;
using System.Drawing;
using System.Windows.Forms;
using FashionShop.BLL;
using FashionShop.DTO;

namespace FashionShop.GUI
{
    public class FrmCustomers : Form
    {
        private readonly CustomerService service = new CustomerService();

        DataGridView dgv;

        TextBox txtId, txtName, txtPhone, txtEmail, txtAddress, txtPoints, txtSearch;
        Button btnAdd, btnUpd, btnDel, btnReload, btnSearch;

        public FrmCustomers()
        {
            // ===== Form base (giống Products) =====
            Text = "Customers Management";
            MinimumSize = new Size(1050, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10f);

            // ===== Root SplitContainer =====
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
                Text = "Customer Information",
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
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
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

            // === controls ===
            txtId = new TextBox { ReadOnly = true, BackColor = Color.FromArgb(245, 246, 250) };
            txtName = new TextBox();
            txtPhone = new TextBox();
            txtEmail = new TextBox();
            txtAddress = new TextBox();
            txtPoints = new TextBox();

            AddRow("Id", txtId);
            AddRow("Name", txtName);
            AddRow("Phone", txtPhone);
            AddRow("Email", txtEmail);
            AddRow("Address", txtAddress);
            AddRow("Points", txtPoints);

            // ===== Buttons grid (giống Products) =====
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

            btnAdd = MakeButton("Add", Color.FromArgb(33, 150, 243));
            btnUpd = MakeButton("Update", Color.FromArgb(76, 175, 80));
            btnDel = MakeButton("Delete", Color.FromArgb(244, 67, 54));
            btnReload = MakeButton("Reload", Color.FromArgb(96, 125, 139));

            btnAdd.Dock = DockStyle.Fill;
            btnUpd.Dock = DockStyle.Fill;
            btnDel.Dock = DockStyle.Fill;
            btnReload.Dock = DockStyle.Fill;

            btnAdd.Margin = new Padding(6);
            btnUpd.Margin = new Padding(6);
            btnDel.Margin = new Padding(6);
            btnReload.Margin = new Padding(6);

            btnGrid.Controls.Add(btnAdd, 0, 0);
            btnGrid.Controls.Add(btnUpd, 1, 0);
            btnGrid.Controls.Add(btnDel, 0, 1);
            btnGrid.Controls.Add(btnReload, 1, 1);

            // ===== LEFT layout stack =====
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
                Height = 50,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0),
            };
            pnlSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
            pnlSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = false,
                Height = 50,
                Margin = new Padding(0, 6, 8, 6),
                Font = new Font("Segoe UI", 12f)
            };

            btnSearch = MakeButton("Search", Color.FromArgb(63, 81, 181));
            btnSearch.Dock = DockStyle.Fill;
            btnSearch.Margin = new Padding(0, 6, 0, 6);

            pnlSearch.Layout += (s, e) =>
            {
                int hTxt = txtSearch.PreferredHeight;
                int pad = Math.Max(0, (pnlSearch.Height - hTxt) / 2);
                txtSearch.Margin = new Padding(0, pad, 8, pad);
                btnSearch.Margin = new Padding(0, pad, 0, pad);
            };

            pnlSearch.Controls.Add(txtSearch, 0, 0);
            pnlSearch.Controls.Add(btnSearch, 1, 0);

            // placeholder giả
            string hint = "Search by name / phone / email...";
            txtSearch.Text = hint;
            txtSearch.ForeColor = Color.Gray;

            txtSearch.GotFocus += (s, e) =>
            {
                if (txtSearch.Text == hint)
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = Color.Black;
                }
            };
            txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = hint;
                    txtSearch.ForeColor = Color.Gray;
                }
            };

            // Grid
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
            dgv.CellClick += Dgv_CellClick;
            StyleGrid(dgv);

            split.Panel2.Controls.Add(dgv);
            split.Panel2.Controls.Add(pnlSearch);

            // set width trái phải lúc mở
            Shown += (s, e) =>
            {
                split.Panel1MinSize = 280;
                split.Panel2MinSize = 450;

                int desiredLeft = 330;
                int maxLeft = split.Width - split.Panel2MinSize;
                split.SplitterDistance = Math.Max(split.Panel1MinSize,
                                          Math.Min(desiredLeft, maxLeft));
            };

            // ===== Events giữ logic cũ =====
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

            btnSearch.Click += (s, e) =>
            {
                var key = txtSearch.Text.Trim();
                if (key == hint) key = "";
                dgv.DataSource = service.Search(key);
            };

            // load data
            Load += (s, e) => LoadGrid();
        }

        // ================= UI STYLE HELPERS =================
        Button MakeButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10f),
                Cursor = Cursors.Hand,
                Margin = new Padding(6),
                MinimumSize = new Size(0, 30)
            };
        }

        void StyleGrid(DataGridView g)
        {
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10f);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            g.ColumnHeadersHeight = 38;

            g.DefaultCellStyle.Font = new Font("Segoe UI", 10f);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(33, 150, 243);
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
        }

        // ================= DATA/FORM =================
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

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var r = dgv.Rows[e.RowIndex];

            txtId.Text = r.Cells["customer_id"].Value?.ToString();
            txtName.Text = r.Cells["customer_name"].Value?.ToString();
            txtPhone.Text = r.Cells["phone"].Value?.ToString();
            txtEmail.Text = r.Cells["email"].Value?.ToString();
            txtAddress.Text = r.Cells["address"].Value?.ToString();
            txtPoints.Text = r.Cells["points"].Value?.ToString();
        }
    }
}
