using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FashionShop.BLL;
using FashionShop.DTO;

namespace FashionShop.GUI
{
    public partial class FrmColors : Form
    {
        private readonly ColorService service = new ColorService();
        private readonly Account current;

        private DataGridView dgv;
        private TextBox txtId, txtName, txtSearch;
        private Button btnAdd, btnUpd, btnDel, btnReload, btnClearSearch;

        private DataTable colorsTable;
        private DataView colorsView;
        private readonly string hintSearch = "Search color name...";

        public FrmColors(Account acc)
        {
            current = acc;

            // ===== Form base =====
            Text = "Manage Colors";
            Size = new Size(520, 420);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10f);
            MinimumSize = new Size(520, 420);

            BuildUI();
            ApplyRolePermission();
            LoadGrid();
        }

        private void BuildUI()
        {
            // ===== Root layout =====
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(14, 12, 14, 14),
                BackColor = Color.White
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));  // input
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // buttons
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));  // search
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // grid
            Controls.Add(root);

            // ================= INPUT AREA =================
            var pnlInput = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(0, 6, 0, 0)
            };
            pnlInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            pnlInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            pnlInput.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            pnlInput.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

            pnlInput.Controls.Add(new Label
            {
                Text = "Id",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            txtId = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };
            pnlInput.Controls.Add(txtId, 1, 0);

            pnlInput.Controls.Add(new Label
            {
                Text = "Color",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 1);

            txtName = new TextBox { Dock = DockStyle.Fill };
            pnlInput.Controls.Add(txtName, 1, 1);

            root.Controls.Add(pnlInput, 0, 0);

            // ================= BUTTONS AREA =================
            var pnlBtn = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0),          
                Margin = new Padding(0, 0, 0, 4)
            };

            btnAdd = MakeButton("Add", Color.FromArgb(33, 150, 243));
            btnUpd = MakeButton("Update", Color.FromArgb(76, 175, 80));
            btnDel = MakeButton("Delete", Color.FromArgb(244, 67, 54));
            btnReload = MakeButton("Reload", Color.FromArgb(96, 125, 139));

            pnlBtn.Controls.AddRange(new Control[] { btnAdd, btnUpd, btnDel, btnReload });
            root.Controls.Add(pnlBtn, 0, 1);

            btnAdd.Click += BtnAdd_Click;
            btnUpd.Click += BtnUpd_Click;
            btnDel.Click += BtnDel_Click;
            btnReload.Click += (s, e) =>
            {
                LoadGrid();
                ClearForm();
            };

            // ================= SEARCH AREA =================
            var pnlSearch = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(0),
                Margin = new Padding(0, 0, 0, 6)
            };
            pnlSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));

            txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10.5f),
                ForeColor = Color.Gray,
                Text = hintSearch
            };

            btnClearSearch = new Button
            {
                Dock = DockStyle.Fill,
                Text = "X",
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClearSearch.FlatAppearance.BorderSize = 0;

            pnlSearch.Controls.Add(txtSearch, 0, 0);
            pnlSearch.Controls.Add(btnClearSearch, 1, 0);
            root.Controls.Add(pnlSearch, 0, 2);

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

            txtSearch.TextChanged += (s, e) =>
            {
                if (txtSearch.Focused) ApplySearch();
            };

            btnClearSearch.Click += (s, e) =>
            {
                txtSearch.Text = hintSearch;
                txtSearch.ForeColor = Color.Gray;
                if (colorsView != null) colorsView.RowFilter = "";
            };

            // ================= GRID AREA =================
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 4, 0, 0),
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

            root.Controls.Add(dgv, 0, 3);
        }

        // ================= LOAD + SEARCH =================
        private void LoadGrid()
        {
            colorsTable = service.GetAll();
            colorsView = colorsTable.DefaultView;

            dgv.DataSource = colorsView;

            if (dgv.Columns.Contains("color_id"))
                dgv.Columns["color_id"].HeaderText = "Id";
            if (dgv.Columns.Contains("color_name"))
                dgv.Columns["color_name"].HeaderText = "Color";
        }

        private void ApplySearch()
        {
            if (colorsView == null) return;

            string key = txtSearch.Text.Trim();
            if (key == hintSearch) key = "";

            colorsView.RowFilter = "";

            if (!string.IsNullOrWhiteSpace(key))
            {
                key = key.Replace("'", "''");
                if (colorsTable.Columns.Contains("color_name"))
                {
                    colorsView.RowFilter = $"CONVERT([color_name], 'System.String') LIKE '%{key}%'";
                }
            }
        }

        // ================= PERMISSION =================
        private void ApplyRolePermission()
        {
            bool isAdmin = current != null &&
                           current.Role != null &&
                           current.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            btnAdd.Enabled = isAdmin;
            btnUpd.Enabled = isAdmin;
            btnDel.Enabled = isAdmin;
            txtName.ReadOnly = !isAdmin;

            if (!isAdmin)
                Text += " (View only)";
        }

        // ================= ACTIONS =================
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (service.Add(name, out string err))
            {
                MessageBox.Show("Added!");
                LoadGrid();
                ClearForm();
            }
            else MessageBox.Show(err);
        }

        private void FrmColors_Load(object sender, EventArgs e)
        {

        }

        private void BtnUpd_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtId.Text, out int id))
            {
                MessageBox.Show("Please choose a color first.");
                return;
            }

            string name = txtName.Text.Trim();
            if (service.Update(id, name, out string err))
            {
                MessageBox.Show("Updated!");
                LoadGrid();
            }
            else MessageBox.Show(err);
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtId.Text, out int id))
            {
                MessageBox.Show("Please choose a color first.");
                return;
            }

            if (MessageBox.Show("Delete this color?", "Confirm", MessageBoxButtons.YesNo)
                == DialogResult.Yes)
            {
                if (service.Delete(id, out string err))
                {
                    MessageBox.Show("Deleted!");
                    LoadGrid();
                    ClearForm();
                }
                else MessageBox.Show(err);
            }
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var r = dgv.Rows[e.RowIndex];

            txtId.Text = r.Cells["color_id"].Value?.ToString();
            txtName.Text = r.Cells["color_name"].Value?.ToString();
        }

        private void ClearForm()
        {
            txtId.Clear();
            txtName.Clear();
        }

        // ================= UI HELPERS =================
        private Button MakeButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                Width = 95,
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
    }
}
