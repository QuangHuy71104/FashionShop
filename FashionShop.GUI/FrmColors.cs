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

            Text = "Manage Colors";
            Size = new Size(780, 520);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10f);
            MinimumSize = new Size(780, 520);

            BuildUI();
            ApplyRolePermission();

            Load += (s, e) => LoadGrid();
        }

        public FrmColors() : this(null) { }

        // ================= UI (LIKE FrmCategories) =================
        private void BuildUI()
        {
            Controls.Clear();

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
                Text = "Color Information",
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

            int row = 0;
            void AddRow(string label, Control control)
            {
                tbl.RowCount = row + 1;
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

                tbl.Controls.Add(lb, 0, row);
                tbl.Controls.Add(control, 1, row);
                row++;
            }

            txtId = new TextBox { ReadOnly = true };
            txtName = new TextBox();

            AddRow("Id", txtId);
            AddRow("Color", txtName);

            // ===== Buttons grid (2x2) =====
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

            foreach (var b in new[] { btnAdd, btnUpd, btnDel, btnReload })
            {
                b.Dock = DockStyle.Fill;
                b.Margin = new Padding(6);
            }

            btnGrid.Controls.Add(btnAdd, 0, 0);
            btnGrid.Controls.Add(btnUpd, 1, 0);
            btnGrid.Controls.Add(btnDel, 0, 1);
            btnGrid.Controls.Add(btnReload, 1, 1);

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

            // icon delete
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

            txtSearch.TextChanged += (s, e) =>
            {
                if (txtSearch.Focused) ApplySearch();
            };

            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ApplySearch();
                }
            };

            btnClearSearch.Click += (s, e) =>
            {
                txtSearch.Text = hintSearch;
                txtSearch.ForeColor = Color.Gray;
                if (colorsView != null) colorsView.RowFilter = "";
            };

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

            // tỉ lệ trái/phải giống Categories
            Shown += (s, e) =>
            {
                split.Panel1MinSize = 260;
                split.Panel2MinSize = 420;

                int desiredLeft = 320;
                int maxLeft = split.Width - split.Panel2MinSize;
                split.SplitterDistance = Math.Max(split.Panel1MinSize,
                                          Math.Min(desiredLeft, maxLeft));
            };

            // ===== events =====
            btnAdd.Click += BtnAdd_Click;
            btnUpd.Click += BtnUpd_Click;
            btnDel.Click += BtnDel_Click;
            btnReload.Click += (s, e) =>
            {
                LoadGrid();
                ClearForm();
            };
        }

        // ================= LOAD + SEARCH =================
        private void LoadGrid()
        {
            colorsTable = service.GetAll();
            colorsView = colorsTable.DefaultView;

            dgv.DataSource = colorsView;

            // highlight header khi chọn ô
            dgv.SelectionChanged += (s, e) => HighlightCurrentColumnHeader();
            dgv.CellEnter += (s, e) => HighlightCurrentColumnHeader();
            dgv.ColumnHeaderMouseClick += (s, e) => HighlightCurrentColumnHeader();

            // cho bảng co giãn theo form
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;

            dgv.RowTemplate.Height = 30;
            dgv.ColumnHeadersHeight = 38;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;

            // ===== STT (No) =====
            if (!dgv.Columns.Contains("stt"))
            {
                var sttCol = new DataGridViewTextBoxColumn
                {
                    Name = "stt",
                    HeaderText = "No",
                    ReadOnly = true
                };
                dgv.Columns.Insert(0, sttCol);
            }

            var stt = dgv.Columns["stt"];
            stt.DisplayIndex = 0;
            stt.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // không cho Fill
            stt.Width = 65;
            stt.MinimumWidth = 65;
            stt.Resizable = DataGridViewTriState.False;

            // ===== ID nhỏ =====
            if (dgv.Columns.Contains("color_id"))
            {
                var id = dgv.Columns["color_id"];
                id.HeaderText = "ID";
                id.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                id.Width = 65;
                id.MinimumWidth = 65;
                id.Resizable = DataGridViewTriState.False;
            }

            // ===== Name co giãn =====
            if (dgv.Columns.Contains("color_name"))
            {
                var name = dgv.Columns["color_name"];
                name.HeaderText = "Color";
                name.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                name.FillWeight = 100;
                name.MinimumWidth = 200;
            }

            // đổ số thứ tự theo filter/search
            dgv.RowPostPaint -= Dgv_RowPostPaint;
            dgv.RowPostPaint += Dgv_RowPostPaint;

            ApplySearch();
            HighlightCurrentColumnHeader();
        }

        private void Dgv_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            dgv.Rows[e.RowIndex].Cells["stt"].Value = (e.RowIndex + 1).ToString();
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
                    colorsView.RowFilter =
                        $"CONVERT([color_name], 'System.String') LIKE '%{key}%'";
                }
            }
        }

        // ================= PERMISSION =================
        private void ApplyRolePermission()
        {
            bool isAdmin = current != null &&
                           current.Role != null &&
                           current.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            if (btnAdd != null) btnAdd.Enabled = isAdmin;
            if (btnUpd != null) btnUpd.Enabled = isAdmin;
            if (btnDel != null) btnDel.Enabled = isAdmin;
            if (txtName != null) txtName.ReadOnly = !isAdmin;

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

            if (MessageBox.Show("Delete this color?", "Confirm",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
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

        // ===== header highlight giống Categories =====
        private readonly Color HeaderBackNormal = Color.FromArgb(245, 245, 245);
        private readonly Color HeaderForeNormal = Color.Black;

        private readonly Color HeaderBackActive = Color.FromArgb(33, 150, 243);
        private readonly Color HeaderForeActive = Color.White;

        private void HighlightCurrentColumnHeader()
        {
            if (dgv == null || dgv.Columns.Count == 0) return;

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                col.HeaderCell.Style.BackColor = HeaderBackNormal;
                col.HeaderCell.Style.ForeColor = HeaderForeNormal;
                col.HeaderCell.Style.Font = new Font("Segoe UI Semibold", 10f);
            }

            if (dgv.CurrentCell == null) return;

            var activeCol = dgv.Columns[dgv.CurrentCell.ColumnIndex];
            activeCol.HeaderCell.Style.BackColor = HeaderBackActive;
            activeCol.HeaderCell.Style.ForeColor = HeaderForeActive;
        }


        private void FrmColors_Load(object sender, EventArgs e)
        {
            
        }
    }
    }
