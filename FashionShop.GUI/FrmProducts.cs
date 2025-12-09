using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FashionShop.BLL;
using FashionShop.DTO;

namespace FashionShop.GUI
{
    // Panel vẽ viền nét đứt giống khung mẫu
    public class DashedDropPanel : Panel
    {
        public DashedDropPanel()
        {
            DoubleBuffered = true;
            BackColor = Color.WhiteSmoke;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var rect = ClientRectangle;
            rect.Inflate(-2, -2);

            using (var pen = new Pen(Color.Gray, 1.5f))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(pen, rect);
            }
        }
    }

    public class FrmProducts : Form
    {
        ProductService service = new ProductService();
        private Account current;

        DataGridView dgv;

        TextBox txtCode, txtName, txtSize, txtColor, txtSearch;
        ComboBox cboCategory, cboGender;
        NumericUpDown nudPrice, nudStock;

        Button btnAdd, btnUpd, btnDel, btnReload, btnSearch;

        // ===== search helpers (giống FrmOrders) =====
        private DataTable productsTable;   // bảng gốc
        private DataView productsView;     // view để search
        private string colCode;
        private string colName;
        private readonly string hintSearch = "Type product code, name, category, color...";

        // Image Dropzone controls
        DashedDropPanel pnlImageDrop;
        PictureBox picPreview;
        Label lblDropHint;
        Button btnChooseImage;
        string selectedImagePath; // lưu path ảnh hiện tại

        public FrmProducts(Account acc)
        {
            current = acc;

            // ===== Form base =====
            Text = "Products Management";
            MinimumSize = new Size(1100, 750);
            Size = new Size(1400, 750);
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
                Text = "Product Information",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 10.5f),
                Padding = new Padding(12)
            };

            // TableLayout auto-size để không ăn khoảng trống
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
            txtCode = new TextBox();
            txtName = new TextBox();

            cboCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

            cboGender = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboGender.Items.AddRange(new object[] { "Men", "Women", "Unisex" });
            cboGender.SelectedIndex = 0;

            txtSize = new TextBox();
            txtColor = new TextBox();

            nudPrice = new NumericUpDown
            {
                Maximum = 1000000000,
                DecimalPlaces = 0,
                ThousandsSeparator = true
            };

            nudStock = new NumericUpDown
            {
                Maximum = 1000000,
                DecimalPlaces = 0,
                ThousandsSeparator = true
            };

            // add rows
            AddRow("Code", txtCode);
            AddRow("Name", txtName);
            AddRow("Category", cboCategory);
            AddRow("Gender", cboGender);
            AddRow("Size", txtSize);
            AddRow("Color", txtColor);
            AddRow("Price", nudPrice);
            AddRow("Stock", nudStock);

            // ===== Buttons grid =====
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

            btnAdd.Click += BtnAdd_Click;
            btnUpd.Click += BtnUpd_Click;
            btnDel.Click += BtnDel_Click;
            btnReload.Click += (s, e) =>
            {
                LoadGrid();
                ClearImageBox();
            };

            // Image Dropzone (khung upload ảnh)
            pnlImageDrop = new DashedDropPanel
            {
                Dock = DockStyle.Top,
                Height = 170,
                Margin = new Padding(6, 10, 6, 0),
                Padding = new Padding(6),
                AllowDrop = true
            };

            picPreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.WhiteSmoke
            };

            // overlay text + button ở giữa
            var overlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            overlay.Cursor = Cursors.Hand;
            overlay.Click += (s, e) => ChooseImage();


            lblDropHint = new Label
            {
                Text = "Drag and drop product image here",
                ForeColor = Color.Black,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f),
            };

            btnChooseImage = new Button
            {
                Text = "Choose image",
                AutoSize = true,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnChooseImage.FlatAppearance.BorderColor = Color.Silver;
            var overlayLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
            };

            overlayLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));   // spacer trên
            overlayLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // label
            overlayLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // button
            overlayLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));   // spacer dưới

            // khoảng cách nhẹ giữa chữ và nút
            lblDropHint.Margin = new Padding(0, 0, 0, 8);      // cách dưới label 8px
            btnChooseImage.Margin = new Padding(0, 4, 0, 0);   // cách trên nút 4px

            lblDropHint.Anchor = AnchorStyles.None;
            btnChooseImage.Anchor = AnchorStyles.None;

            overlayLayout.Controls.Add(new Panel(), 0, 0); // spacer trên
            overlayLayout.Controls.Add(lblDropHint, 0, 1);
            overlayLayout.Controls.Add(btnChooseImage, 0, 2);
            overlayLayout.Controls.Add(new Panel(), 0, 3); // spacer dưới


            lblDropHint.Anchor = AnchorStyles.None;
            btnChooseImage.Anchor = AnchorStyles.None;

            overlay.Controls.Add(overlayLayout);

            pnlImageDrop.Controls.Add(picPreview);
            pnlImageDrop.Controls.Add(overlay);

            btnChooseImage.Click += (s, e) => ChooseImage();

            pnlImageDrop.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Copy;
            };

            pnlImageDrop.DragDrop += (s, e) =>
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                    LoadImage(files[0]);
            };

            pnlImageDrop.Cursor = Cursors.Hand;
            picPreview.Cursor = Cursors.Hand;
            lblDropHint.Cursor = Cursors.Hand;

            pnlImageDrop.Click += (s, e) => ChooseImage();
            picPreview.Click += (s, e) => ChooseImage();
            lblDropHint.Click += (s, e) => ChooseImage();

            // ===== Left layout =====
            var leftLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(0),
            };

            leftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            leftLayout.Controls.Add(tbl, 0, 0);
            leftLayout.Controls.Add(pnlImageDrop, 0, 1); // thêm khung ở đây
            leftLayout.Controls.Add(btnGrid, 0, 2);

            gbInput.Controls.Clear();
            gbInput.Controls.Add(leftLayout);
            split.Panel1.Controls.Add(gbInput);

            // ================= RIGHT: Search + Grid =================
            split.Panel2.Padding = new Padding(12);

            // ===== Search bar giống FrmOrders =====
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

            btnSearch = MakeButton("", Color.FromArgb(244, 67, 54));
            btnSearch.Dock = DockStyle.Fill;
            btnSearch.Margin = new Padding(0, 6, 0, 6);

            pnlSearch.Controls.Add(txtSearch, 0, 0);
            pnlSearch.Controls.Add(btnSearch, 1, 0);

            // icon nút đỏ (nếu bạn không có Properties.Resources.delete thì comment 2 dòng này)
            btnSearch.Image = ResizeImage(Properties.Resources.delete, 18, 18);
            btnSearch.ImageAlign = ContentAlignment.MiddleCenter;
            btnSearch.TextImageRelation = TextImageRelation.Overlay;
            btnSearch.Padding = new Padding(0);

            // Placeholder
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

            // Click nút đỏ: clear + show full list
            btnSearch.Click += (s, e) =>
            {
                txtSearch.Text = hintSearch;
                txtSearch.ForeColor = Color.Gray;

                if (productsView != null)
                    productsView.RowFilter = "";
            };

            // live search khi gõ
            txtSearch.TextChanged += (s, e) =>
            {
                if (txtSearch.Focused) ApplySearch();
            };

            // Enter để search luôn
            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ApplySearch();
                }
            };

            // ===== Grid =====
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

            // ✅ set width trái phải hợp lý ngay lúc mở
            Shown += (s, e) =>
            {
                split.Panel1MinSize = 280;
                split.Panel2MinSize = 450;

                int desiredLeft = 330;
                int maxLeft = split.Width - split.Panel2MinSize;
                split.SplitterDistance = Math.Max(split.Panel1MinSize,
                                          Math.Min(desiredLeft, maxLeft));
            };

            // ✅ LOAD DATA NGAY TỪ ĐẦU
            Load += (s, e) =>
            {
                cboCategory.DataSource = service.GetCategories();
                cboCategory.DisplayMember = "category_name";
                cboCategory.ValueMember = "category_id";

                LoadGrid();             // hiển thị list ngay
                ApplyRolePermission();
                ClearImageBox();        // show hint upload lúc mở
            };
        }

        public FrmProducts() : this(null) { }

        // ================= SEARCH CORE =================

        private void ReloadProducts()
        {
            productsTable = service.GetAll();   // GetAll trả DataTable
            ResolveProductColumns();

            productsView = productsTable.DefaultView;
            dgv.DataSource = productsView;

            SetupAutoComplete();
            ApplySearch();
        }

        private void ResolveProductColumns()
        {
            if (productsTable == null) return;

            string[] codeCandidates = { "product_code", "ProductCode", "code", "productCode" };
            string[] nameCandidates = { "product_name", "ProductName", "name", "productName" };

            colCode = productsTable.Columns
                .Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .FirstOrDefault(n => codeCandidates.Contains(n));

            colName = productsTable.Columns
                .Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .FirstOrDefault(n => nameCandidates.Contains(n));
        }

        private void ApplySearch()
        {
            if (productsView == null) return;

            var key = txtSearch.Text.Trim();
            if (key == hintSearch) key = "";
            productsView.RowFilter = "";

            if (!string.IsNullOrWhiteSpace(key))
            {
                key = key.Replace("'", "''");

                // các cột muốn search
                string[] cols = {
                    "product_code",
                    "product_name",
                    "category_name",
                    "size",
                    "color",
                    "gender"
                };

                // chỉ lấy những cột thật sự tồn tại trong DataTable
                var realCols = cols
                    .Where(c => productsTable.Columns.Contains(c))
                    .Select(c => $"CONVERT([{c}], 'System.String') LIKE '%{key}%'");

                productsView.RowFilter = string.Join(" OR ", realCols);
            }
        }

        private void SetupAutoComplete()
        {
            if (productsTable == null) return;

            var src = new AutoCompleteStringCollection();

            if (!string.IsNullOrEmpty(colCode))
            {
                var codes = productsTable.AsEnumerable()
                    .Select(r => r[colCode]?.ToString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToArray();
                src.AddRange(codes);
            }

            if (!string.IsNullOrEmpty(colName))
            {
                var names = productsTable.AsEnumerable()
                    .Select(r => r[colName]?.ToString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToArray();
                src.AddRange(names);
            }

            txtSearch.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtSearch.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtSearch.AutoCompleteCustomSource = src;
        }

        // ================= IMAGE DROPZONE HELPERS =================

        private void ChooseImage()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                ofd.Title = "Choose product image";

                if (ofd.ShowDialog() == DialogResult.OK)
                    LoadImage(ofd.FileName);
            }
        }

        private void LoadImage(string path)
        {
            try
            {
                selectedImagePath = path;

                // tránh lock file
                using (var tmp = Image.FromFile(path))
                {
                    picPreview.Image = new Bitmap(tmp);
                }

                // ẩn hint
                picPreview.Visible = true;
                lblDropHint.Visible = false;
                btnChooseImage.Visible = false;
            }
            catch
            {
                MessageBox.Show("Cannot load this image!");
            }
        }

        private void ClearImageBox()
        {
            picPreview.Image = null;
            selectedImagePath = null;

            picPreview.Visible = false;
            lblDropHint.Visible = true;
            btnChooseImage.Visible = true;
        }

        // ================= UI STYLE HELPERS =================

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

            // ===== FIX CỨNG Ô =====
            g.AllowUserToResizeColumns = false;
            g.AllowUserToResizeRows = false;
            g.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            g.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            g.RowTemplate.Height = 32;     // chiều cao dòng cố định
            g.ColumnHeadersHeight = 38;    // header cố định

            g.DefaultCellStyle.Font = new Font("Segoe UI", 10f);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(33, 150, 243);
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
        }

        // ================= PERMISSION =================

        private void ApplyRolePermission()
        {
            if (current == null) return;

            bool isStaff = current.Role != null &&
                           current.Role.Equals("Staff", StringComparison.OrdinalIgnoreCase);

            if (isStaff)
            {
                btnAdd.Enabled = false;
                btnUpd.Enabled = false;
                btnDel.Enabled = false;

                txtCode.ReadOnly = true;
                txtName.ReadOnly = true;
                txtSize.ReadOnly = true;
                txtColor.ReadOnly = true;
                nudPrice.Enabled = false;
                nudStock.Enabled = false;
                cboCategory.Enabled = false;
                cboGender.Enabled = false;

                Text += " (View only)";
            }
        }

        // LoadGrid giờ gọi ReloadProducts
        void LoadGrid() => ReloadProducts();

        Product ReadForm()
        {
            return new Product
            {
                Code = txtCode.Text.Trim(),
                Name = txtName.Text.Trim(),
                CategoryId = Convert.ToInt32(cboCategory.SelectedValue),
                Gender = cboGender.Text,
                Size = txtSize.Text.Trim(),
                Color = txtColor.Text.Trim(),
                Price = nudPrice.Value,
                Stock = (int)nudStock.Value,
                 ImagePath = selectedImagePath
            };
        }

        // ================= ACTIONS =================

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var p = ReadForm();
                if (service.Add(p, out string err))
                {
                    MessageBox.Show("Added!");
                    LoadGrid();
                    ClearImageBox();
                }
                else MessageBox.Show(err);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid data: " + ex.Message);
            }
        }

        private void BtnUpd_Click(object sender, EventArgs e)
        {
            try
            {
                var p = ReadForm();
                if (service.Update(p, out string err))
                {
                    MessageBox.Show("Updated!");
                    LoadGrid();
                }
                else MessageBox.Show(err);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid data: " + ex.Message);
            }
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text)) return;
            if (MessageBox.Show("Delete this product?", "Confirm", MessageBoxButtons.YesNo)
                == DialogResult.Yes)
            {
                service.Delete(txtCode.Text.Trim());
                LoadGrid();
                ClearImageBox();
            }
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var r = dgv.Rows[e.RowIndex];

            txtCode.Text = r.Cells["product_code"].Value?.ToString();
            txtName.Text = r.Cells["product_name"].Value?.ToString();
            txtSize.Text = r.Cells["size"].Value?.ToString();
            txtColor.Text = r.Cells["color"].Value?.ToString();
            cboGender.Text = r.Cells["gender"].Value?.ToString();

            if (decimal.TryParse(r.Cells["price"].Value?.ToString(), out var price))
                nudPrice.Value = Math.Min(nudPrice.Maximum, price);

            if (int.TryParse(r.Cells["stock"].Value?.ToString(), out var stock))
                nudStock.Value = Math.Min(nudStock.Maximum, stock);

            // RESET khung ảnh trước
            ClearImageBox();

            // nếu có cột image_path thì load ảnh theo sản phẩm
            if (dgv.Columns.Contains("image_path"))
            {
                var path = r.Cells["image_path"].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path))
                {
                    LoadImage(path);
                }
            }
        }

    }
}
