using FashionShop.BLL;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FashionShop.GUI
{
    public partial class FrmHistory : Form
    {
        private readonly OrderService orderService = new OrderService();

        private DataTable historyTable;   // raw: mỗi sản phẩm 1 dòng
        private DataView historyView;
        private readonly string hint = "Search by customer / product...";

        // các cột thuộc "hóa đơn" sẽ merge
        private readonly string[] orderCols =
        {
            "order_code",
            "order_date",
            "customer_name",
            "employee_name",
            "total_amount"
        };

        private Panel WrapDatePickerToMatchButton(DateTimePicker dtp, int targetHeight)
        {
            var pnl = new Panel
            {
                BackColor = Color.White,
                Height = targetHeight,
                MinimumSize = new Size(0, targetHeight),
                MaximumSize = new Size(0, targetHeight),
                Dock = DockStyle.Fill,
                Padding = new Padding(2),
                Margin = new Padding(0, 6, 6, 6)
            };

            pnl.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
                {
                    var r = pnl.ClientRectangle;
                    r.Width -= 1; r.Height -= 1;
                    e.Graphics.DrawRectangle(pen, r);
                }
            };

            dtp.Format = DateTimePickerFormat.Custom;
            dtp.CustomFormat = "dd/MM/yyyy";

            // căn giữa dọc
            dtp.Location = new Point(2, (targetHeight - dtp.Height) / 2);
            dtp.Width = pnl.Width - 4;
            dtp.Anchor = AnchorStyles.Left | AnchorStyles.Right;

            pnl.Resize += (_, __) =>
            {
                dtp.Location = new Point(2, (pnl.Height - dtp.Height) / 2);
                dtp.Width = pnl.Width - 4;
            };

            pnl.Controls.Add(dtp);
            return pnl;
        }


        private void MakeDateFramesSameHeightAsSearchButton()
        {
            int h = btnSearch.Height;  // lấy đúng chiều cao nút Search sau layout

            // ===== FROM =====
            var tlpFrom = dtFrom.Parent as TableLayoutPanel;
            if (tlpFrom != null && !(dtFrom.Parent is Panel))
            {
                int col = tlpFrom.GetColumn(dtFrom);
                int row = tlpFrom.GetRow(dtFrom);

                tlpFrom.Controls.Remove(dtFrom);
                var pnlFrom = WrapDatePickerToMatchButton(dtFrom, h);

                tlpFrom.Controls.Add(pnlFrom, col, row);
            }

            // ===== TO =====
            var tlpTo = dtTo.Parent as TableLayoutPanel;
            if (tlpTo != null && !(dtTo.Parent is Panel))
            {
                int col = tlpTo.GetColumn(dtTo);
                int row = tlpTo.GetRow(dtTo);

                tlpTo.Controls.Remove(dtTo);
                var pnlTo = WrapDatePickerToMatchButton(dtTo, h);

                tlpTo.Controls.Add(pnlTo, col, row);
            }
        }



        public FrmHistory()
        {
            InitializeComponent();

            Shown += (s, e) => MakeDateFramesSameHeightAsSearchButton();



            // ===== ép width nút Clear nhỏ lại nếu parent là TableLayoutPanel =====
            if (btnClear.Parent is TableLayoutPanel tlp)
            {
                int col = tlp.GetColumn(btnClear);
                tlp.ColumnStyles[col].SizeType = SizeType.Absolute;
                tlp.ColumnStyles[col].Width = 40;
                btnClear.Dock = DockStyle.Fill;
            }
            else
            {
                btnClear.AutoSize = false;
                btnClear.Width = 40;
                btnClear.Height = btnSearch.Height;
            }

            // ===== form base =====
            StartPosition = FormStartPosition.CenterScreen;
            var wa = Screen.PrimaryScreen.WorkingArea;
            MinimumSize = new Size(950, 550);

            Size = new Size(1400, 700);
            Padding = new Padding(10, 0, 10, 10);
            AutoScaleMode = AutoScaleMode.Font;

            dgvHistory.Dock = DockStyle.Fill;
            StyleGrid(dgvHistory);

            // ===== highlight header when changing selection =====
            dgvHistory.SelectionChanged += (s, e) => HighlightCurrentColumnHeader();
            dgvHistory.CellEnter += (s, e) => HighlightCurrentColumnHeader();
            dgvHistory.ColumnHeaderMouseClick += (s, e) => HighlightCurrentColumnHeader();


            // ===== placeholder search =====
            txtSearch.Text = hint;
            txtSearch.ForeColor = Color.Gray;

            txtSearch.GotFocus += (_, __) =>
            {
                if (txtSearch.Text == hint)
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = Color.Black;
                }
            };

            txtSearch.LostFocus += (_, __) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = hint;
                    txtSearch.ForeColor = Color.Gray;
                }
            };

            txtSearch.TextChanged += (_, __) =>
            {
                if (txtSearch.Focused) ApplySearch();
            };

            txtSearch.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ApplySearch();
                }
            };

            // ===== btnSearch lọc theo ngày =====
            btnSearch.Click += (_, __) => ReloadHistory();

            // ===== style btnClear icon đỏ =====
            btnClear.Text = "";
            btnClear.BackColor = Color.FromArgb(244, 67, 54);
            btnClear.ForeColor = Color.White;
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Cursor = Cursors.Hand;

            btnClear.Image = ResizeImage(Properties.Resources.delete, 18, 18);
            btnClear.ImageAlign = ContentAlignment.MiddleCenter;
            btnClear.TextImageRelation = TextImageRelation.Overlay;
            btnClear.Padding = new Padding(0);

            btnClear.Click += (_, __) =>
            {
                txtSearch.Text = hint;
                txtSearch.ForeColor = Color.Gray;
                if (historyView != null) historyView.RowFilter = "";
            };

            Load += FrmHistory_Load;
        }

        private void FrmHistory_Load(object sender, EventArgs e)
        {
            dtFrom.Value = DateTime.Today.AddMonths(-1);
            dtTo.Value = DateTime.Today;
            BeginInvoke(new Action(() =>
            {
                MakeDateFramesSameHeightAsSearchButton();
            }));
            ReloadHistory();
        }

        private void ReloadHistory()
        {
            DateTime? from = dtFrom.Checked ? dtFrom.Value.Date : (DateTime?)null;
            DateTime? to = dtTo.Checked ? dtTo.Value.Date : (DateTime?)null;

            historyTable = orderService.GetHistory("", "", from, to);
            historyTable.DefaultView.Sort = "order_code ASC, order_date ASC";

            historyView = historyTable.DefaultView;
            dgvHistory.DataSource = historyView;

            SetHistoryColumnOrder();
            SetHistoryColumnWidth();

            ApplySearch();
            EnableMergedOrderCells();
            HighlightCurrentColumnHeader();

        }

        private void SetHistoryColumnOrder()
        {
            if (dgvHistory.Columns.Contains("order_id"))
                dgvHistory.Columns["order_id"].Visible = false;

            int i = 0;
            void SetCol(string name, string header = null, string format = null)
            {
                if (!dgvHistory.Columns.Contains(name)) return;
                var c = dgvHistory.Columns[name];
                c.DisplayIndex = i++;
                if (header != null) c.HeaderText = header;
                if (format != null) c.DefaultCellStyle.Format = format;
            }

            // hóa đơn
            SetCol("order_code", "Order Code");
            SetCol("order_date", "Order Date", "dd/MM/yyyy HH:mm");
            SetCol("customer_name", "Customer");
            SetCol("employee_name", "Employee");

            // sản phẩm
            SetCol("product_id", "ProductId");
            SetCol("product_name", "ProductName");
            SetCol("size", "Size");
            SetCol("color", "Color");
            SetCol("stock", "Stock");

            // số lượng + tiền
            SetCol("quantity", "Qty");
            SetCol("unit_price", "Unit Price", "N0");
            SetCol("line_total", "SubTotal", "N0");
            SetCol("total_amount", "Total Amount", "N0");
        }

        private void SetHistoryColumnWidth()
        {
            dgvHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            void Fix(string name, int w)
            {
                if (!dgvHistory.Columns.Contains(name)) return;
                var c = dgvHistory.Columns[name];
                c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                c.Width = w;
            }

            void Fill(string name, float weight)
            {
                if (!dgvHistory.Columns.Contains(name)) return;
                var c = dgvHistory.Columns[name];
                c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                c.FillWeight = weight;
                c.MinimumWidth = 60;   // chống bị bóp quá nhỏ
            }

            // ==== 1) FIX cột hoá đơn (ít thay đổi, nên cố định) ====
            Fix("order_code", 170);
            Fix("order_date", 150);
            Fix("customer_name", 120);
            Fix("employee_name", 120);
            Fix("product_id", 100);
            Fix("total_amount", 110);

            // ==== 2) Fill các cột sản phẩm theo tỉ lệ ====
            Fill("product_id", 100);
            Fill("product_name", 220);
            Fill("size", 60);
            Fill("color", 80);
            Fill("stock", 60);
            Fill("quantity", 60);
            Fill("unit_price", 90);
            Fill("line_total", 90);

            // Nếu có cột thừa thì vẫn Fill nhẹ
            foreach (DataGridViewColumn col in dgvHistory.Columns)
            {
                if (col.AutoSizeMode == DataGridViewAutoSizeColumnMode.None) continue;
                if (col.FillWeight <= 0) col.FillWeight = 80;
            }
        }


        private void ApplySearch()
        {
            if (historyView == null) return;

            var key = txtSearch.Text.Trim();
            if (key == hint) key = "";

            historyView.RowFilter = "";

            if (!string.IsNullOrWhiteSpace(key))
            {
                key = key.Replace("'", "''");

                string[] cols =
                {
                    "order_code","customer_name","employee_name","product_name",
                    "size","color","quantity","unit_price","line_total",
                    "total_amount","order_date"
                };

                var realCols = cols
                    .Where(c => historyTable.Columns.Contains(c))
                    .Select(c => $"CONVERT([{c}], 'System.String') LIKE '%{key}%'");

                historyView.RowFilter = string.Join(" OR ", realCols);
            }
        }

        private void EnableMergedOrderCells()
        {
            dgvHistory.CellPainting -= DgvHistory_CellPainting_Merge;
            dgvHistory.CellPainting += DgvHistory_CellPainting_Merge;

            dgvHistory.CellFormatting -= DgvHistory_CellFormatting_Merge;
            dgvHistory.CellFormatting += DgvHistory_CellFormatting_Merge;
        }

        private void DgvHistory_CellFormatting_Merge(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 1) return;

            var colName = dgvHistory.Columns[e.ColumnIndex].Name;
            if (!orderCols.Contains(colName)) return;

            string curr = dgvHistory.Rows[e.RowIndex].Cells["order_code"].Value?.ToString();
            string prev = dgvHistory.Rows[e.RowIndex - 1].Cells["order_code"].Value?.ToString();

            if (curr == prev)
            {
                e.Value = "";
                e.FormattingApplied = true;
            }
        }

        private void DgvHistory_CellPainting_Merge(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var colName = dgvHistory.Columns[e.ColumnIndex].Name;
            if (!orderCols.Contains(colName)) return;

            int row = e.RowIndex;

            if (row > 0)
            {
                string curr = dgvHistory.Rows[row].Cells["order_code"].Value?.ToString();
                string prev = dgvHistory.Rows[row - 1].Cells["order_code"].Value?.ToString();
                if (curr == prev)
                {
                    e.Handled = true;
                    return;
                }
            }

            int groupHeight = dgvHistory.Rows[row].Height;
            int nextRow = row + 1;
            string first = dgvHistory.Rows[row].Cells["order_code"].Value?.ToString();

            while (nextRow < dgvHistory.Rows.Count)
            {
                string curr = dgvHistory.Rows[nextRow].Cells["order_code"].Value?.ToString();
                if (curr != first) break;
                groupHeight += dgvHistory.Rows[nextRow].Height;
                nextRow++;
            }

            Rectangle cellRect = e.CellBounds;
            cellRect.Height = groupHeight;

            using (Brush back = new SolidBrush(e.CellStyle.BackColor))
                e.Graphics.FillRectangle(back, cellRect);

            using (Pen p = new Pen(dgvHistory.GridColor))
                e.Graphics.DrawRectangle(p, cellRect.X, cellRect.Y, cellRect.Width - 1, cellRect.Height - 1);

            string text = dgvHistory.Rows[row].Cells[colName].FormattedValue?.ToString() ?? "";

            TextRenderer.DrawText(
                e.Graphics,
                text,
                e.CellStyle.Font,
                cellRect,
                e.CellStyle.ForeColor,
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.Left |
                TextFormatFlags.EndEllipsis
            );

            e.Handled = true;
        }

        private void StyleGrid(DataGridView g)
        {
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10f);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;

            g.AllowUserToResizeColumns = false;
            g.AllowUserToResizeRows = false;
            g.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            g.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            g.RowTemplate.Height = 32;
            g.ColumnHeadersHeight = 38;

            g.DefaultCellStyle.Font = new Font("Segoe UI", 10f);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(33, 150, 243);
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            g.ReadOnly = true;
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.RowHeadersVisible = false;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.BorderStyle = BorderStyle.FixedSingle;
            g.ScrollBars = ScrollBars.Both;
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

        private void FrmHistory_Load_1(object sender, EventArgs e) { }

        private void dgvHistory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void FrmHistory_Load_2(object sender, EventArgs e)
        {

        }

        // ===== Header highlight colors =====
        private readonly Color HeaderBackNormal = Color.FromArgb(245, 245, 245);
        private readonly Color HeaderForeNormal = Color.Black;

        private readonly Color HeaderBackActive = Color.FromArgb(33, 150, 243); // xanh primary
        private readonly Color HeaderForeActive = Color.White;

        private void HighlightCurrentColumnHeader()
        {
            if (dgvHistory == null || dgvHistory.Columns.Count == 0) return;

            // reset all columns header to normal
            foreach (DataGridViewColumn col in dgvHistory.Columns)
            {
                col.HeaderCell.Style.BackColor = HeaderBackNormal;
                col.HeaderCell.Style.ForeColor = HeaderForeNormal;
                col.HeaderCell.Style.Font = new Font("Segoe UI Semibold", 10f);
            }

            // active column = column of current cell
            var curCell = dgvHistory.CurrentCell;
            if (curCell == null) return;

            var activeCol = dgvHistory.Columns[curCell.ColumnIndex];
            activeCol.HeaderCell.Style.BackColor = HeaderBackActive;
            activeCol.HeaderCell.Style.ForeColor = HeaderForeActive;
        }

    }
}
