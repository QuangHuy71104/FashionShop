using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FashionShop.BLL;
using FashionShop.DTO;
using System.Globalization;
using System.IO; // ✅ NEW for blob

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
        private readonly ProductService service = new ProductService();
        private readonly Account current;

        private DataGridView dgv;

        private TextBox txtCode, txtName, txtSearch;
        private ComboBox cboCategory, cboGender, cboSize, cboColor;
        private NumericUpDown nudPrice, nudStock;

        private Button btnAdd, btnUpd, btnDel, btnReload, btnSearch;

        // ===== search helpers =====
        private DataTable productsTable;
        private DataView productsView;
        private string colCode;
        private string colName;
        private readonly string hintSearch = "Type product code, name, category, color...";

        // Image Dropzone controls
        private DashedDropPanel pnlImageDrop;
        private PictureBox picPreview;
        private Label lblDropHint;
        private Button btnChooseImage;

        // ✅ CHANGED: store image as BLOB instead of path
        private byte[] selectedImageBlob;
        private string selectedImageMime;

        private readonly ColorService colorService = new ColorService();

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
                FixedPanel = FixedPanel.Panel1,   // cố định panel trái (tùy bạn muốn cố định bên nào)
                IsSplitterFixed = true,           // khóa không cho kéo
                BackColor = Color.White,
                SplitterWidth = 2                 // optional: cho đường giữa mảnh lại
            };

            // optional: để cursor không hiện dạng kéo
            split.SplitterMoved += (s, e) => split.SplitterDistance = split.SplitterDistance;
            split.Cursor = Cursors.Default;
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

            // Category combobox + manage button in a panel
            cboCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

            cboGender = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboGender.Items.AddRange(new object[] { "Men", "Women", "Unisex" });
            cboGender.SelectedIndex = 0;

            // Size + Color dropdowns (wrapped to avoid overflow)
            cboSize = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboColor = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

            // icon 3 chấm/bánh răng bạn chuẩn bị sẵn (xem mục 4)
            Image icoManage = Properties.Resources.dots; // ví dụ tên dots

            var pnlCategory = WrapComboWithIcon(
                cboCategory,
                icoManage,
                (s, e) =>
                {
                    if (!IsAdmin()) return;
                    using (var f = new FrmCategories(current))
                        f.ShowDialog();
                    LoadCategories(); //reload lại category sau CRUD
                },
                comboWidth: 200,
                gap: 6
                );

            var pnlColor = WrapComboWithIcon(
                cboColor,
                icoManage,
                (s, e) =>
                {
                    if (!IsAdmin()) return;
                    using (var f = new FrmColors(current)) f.ShowDialog();
                    LoadColors();
                },
                comboWidth: 200,
                gap: 6
            );

            nudPrice = new NumericUpDown
            {
                Maximum = 1000000000,
                DecimalPlaces = 0,
                ThousandsSeparator = false,
                Dock = DockStyle.Fill
            };

            // label "000" gợi ý
            var lblZeros = new Label
            {
                Text = " ,000",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10f, FontStyle.Italic),
                AutoSize = true,
                Dock = DockStyle.Right,
                Padding = new Padding(0, 4, 6, 0),  // canh cho đẹp
                TextAlign = ContentAlignment.MiddleRight
            };
            // panel bọc lại để nhìn như suffix nằm trong ô
            var priceWrap = new Panel
            {
                Height = nudPrice.Height,
                Dock = DockStyle.Top,
                Padding = new Padding(0),
                BackColor = Color.White
            };

            priceWrap.Controls.Add(nudPrice);
            priceWrap.Controls.Add(lblZeros);

            // Price nhập theo đơn vị nghìn: Enter bỏ 000, Leave tự thêm 000
            //nudPrice.Enter += (s, e) =>
            //{
            //    var v = nudPrice.Value;
            //    if (v >= 1000 && v % 1000 == 0)
            //        nudPrice.Value = v / 1000; // show phần nghìn để dễ sửa
            //};

            //nudPrice.Leave += (s, e) =>
            //{
            //    nudPrice.Value = nudPrice.Value * 1000; // tự thêm 000 khi rời ô
            //};

            nudStock = new NumericUpDown
            {
                Maximum = 1000000,
                DecimalPlaces = 0,
                ThousandsSeparator = true
            };



            // add rows
            AddRow("Code", txtCode);
            AddRow("Name", txtName);
            AddRow("Category", pnlCategory);
            AddRow("Color", pnlColor);
            AddRow("Gender", cboGender);
            AddRow("Price", priceWrap);
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

            overlayLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            overlayLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            overlayLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            overlayLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            lblDropHint.Margin = new Padding(0, 0, 0, 8);
            btnChooseImage.Margin = new Padding(0, 4, 0, 0);

            lblDropHint.Anchor = AnchorStyles.None;
            btnChooseImage.Anchor = AnchorStyles.None;

            overlayLayout.Controls.Add(new Panel(), 0, 0);
            overlayLayout.Controls.Add(lblDropHint, 0, 1);
            overlayLayout.Controls.Add(btnChooseImage, 0, 2);
            overlayLayout.Controls.Add(new Panel(), 0, 3);

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
                    LoadImage(files[0]); // ✅ still load from file to preview & capture blob
            };

            pnlImageDrop.Cursor = Cursors.Hand;
            picPreview.Cursor = Cursors.Hand;
            lblDropHint.Cursor = Cursors.Hand;

            pnlImageDrop.Click += (s, e) => ChooseImage();
            picPreview.Click += (s, e) => ChooseImage();
            lblDropHint.Click += (s, e) => ChooseImage();

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
            leftLayout.Controls.Add(pnlImageDrop, 0, 1);
            leftLayout.Controls.Add(btnGrid, 0, 2);

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

            btnSearch.Click += (s, e) =>
            {
                txtSearch.Text = hintSearch;
                txtSearch.ForeColor = Color.Gray;

                if (productsView != null)
                    productsView.RowFilter = "";
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

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None, // cho phép rộng hơn
                ScrollBars = ScrollBars.Both,                               // bật cả 2 scroll
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };
            dgv.CellClick += Dgv_CellClick;
            // mặc định KHÔNG xuống dòng cho toàn bộ bảng
            dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgv.RowTemplate.Height = 28;   // gọn
            dgv.SelectionChanged += (s, e) => HighlightCurrentColumnHeader();
            dgv.CellEnter += (s, e) => HighlightCurrentColumnHeader();
            dgv.ColumnHeaderMouseClick += (s, e) => HighlightCurrentColumnHeader();

            StyleGrid(dgv);

            split.Panel2.Controls.Add(dgv);
            split.Panel2.Controls.Add(pnlSearch);

            Shown += (s, e) =>
            {
                split.Panel1MinSize = 280;
                split.Panel2MinSize = 450;

                int desiredLeft = 400;
                int maxLeft = split.Width - split.Panel2MinSize;
                split.SplitterDistance = Math.Max(split.Panel1MinSize,
                                          Math.Min(desiredLeft, maxLeft));
            };

            Load += (s, e) =>
            {
                LoadCategories();
                LoadColors();

                // Size chỉ sổ xuống, không manage
                cboSize.Items.Clear();
                cboSize.Items.AddRange(new object[] { "S", "M", "L", "XL", "XXL" });
                cboSize.SelectedIndex = 0;

                LoadGrid();
                ApplyRolePermission();
                ClearImageBox();
            };
        }

        public FrmProducts() : this(null) { }

        // ================= SEARCH CORE =================

        private void ReloadProducts()
        {
            productsTable = service.GetAll();
            ResolveProductColumns();

            productsView = productsTable.DefaultView;
            dgv.DataSource = productsView;
            // tắt autosize để width có hiệu lực
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }

            // set width gọn gàng hơn
            SetColWidth("product_id", 100);
            SetColWidth("product_code", 120);
            SetColWidth("product_name", 200);
            SetColWidth("category_name", 150);
            SetColWidth("size", 70);
            SetColWidth("color", 70);
            SetColWidth("gender", 80);
            SetColWidth("price", 100);
            SetColWidth("stock", 80);

            // chỉ product_name được xuống dòng
            if (dgv.Columns.Contains("product_name"))
            {
                var col = dgv.Columns["product_name"];
                col.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }

            // cho phép row tự cao khi product_name dài
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            // (optional) canh chữ lên trên cho đẹp khi ô cao
            dgv.Columns["product_name"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;

            // helper local function
            void SetColWidth(string name, int w)
            {
                if (dgv.Columns.Contains(name))
                    dgv.Columns[name].Width = w;
            }

            // ===== format price: có dấu phẩy, không .00 =====
            if (dgv.Columns.Contains("price"))
            {
                var colPrice = dgv.Columns["price"];
                colPrice.DefaultCellStyle.Format = "#,##0"; // 12000 -> 12,000
                colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }

            SetupAutoComplete();
            ApplySearch();
            HighlightCurrentColumnHeader();

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
            if (productsView == null || productsTable == null) return;

            var key = txtSearch.Text.Trim();
            if (key == hintSearch) key = "";

            productsView.RowFilter = "";

            if (string.IsNullOrWhiteSpace(key)) return;

            key = key.Replace("'", "''");

            // các cột text muốn search
            string[] textCols =
            {
        "product_code",
        "product_name",
        "category_name",
        "size",
        "color_name",   // ✅ đúng tên cột
        "gender"
    };

            var filters = textCols
                .Where(c => productsTable.Columns.Contains(c))
                .Select(c => $"CONVERT([{c}], 'System.String') LIKE '%{key}%'")
                .ToList();

            // ✅ search thêm price: cho phép gõ 280 hoặc 280000 hoặc 280,000
            if (productsTable.Columns.Contains("price"))
            {
                var digits = new string(key.Where(char.IsDigit).ToArray());
                if (!string.IsNullOrEmpty(digits))
                {
                    filters.Add($"CONVERT([price], 'System.String') LIKE '%{digits}%'");
                }
            }

            productsView.RowFilter = string.Join(" OR ", filters);
        }

        private void SetupAutoComplete()
        {
            if (productsTable == null) return;

            var src = new AutoCompleteStringCollection();

            // add code
            if (!string.IsNullOrEmpty(colCode) && productsTable.Columns.Contains(colCode))
            {
                var codes = productsTable.AsEnumerable()
                    .Select(r => r[colCode]?.ToString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToArray();
                src.AddRange(codes);
            }

            // add name
            if (!string.IsNullOrEmpty(colName) && productsTable.Columns.Contains(colName))
            {
                var names = productsTable.AsEnumerable()
                    .Select(r => r[colName]?.ToString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToArray();
                src.AddRange(names);
            }

            // add other columns (đúng tên cột trong DataTable)
            AddCol("category_name");
            AddCol("size");
            AddCol("color_name"); // chú ý: DB đang trả color_name
            AddCol("gender");

            txtSearch.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtSearch.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtSearch.AutoCompleteCustomSource = src;

            // ===== local helper =====
            void AddCol(string col)
            {
                if (!productsTable.Columns.Contains(col)) return;

                var vals = productsTable.AsEnumerable()
                    .Select(r => r[col]?.ToString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToArray();

                src.AddRange(vals);
            }
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

        // ✅ CHANGED: load from file -> store blob + mime + preview
        private void LoadImage(string path)
        {
            try
            {
                selectedImageBlob = File.ReadAllBytes(path);
                selectedImageMime = GetMimeFromExtension(Path.GetExtension(path));

                using (var tmp = Image.FromFile(path))
                {
                    picPreview.Image = new Bitmap(tmp);
                }

                picPreview.Visible = true;
                lblDropHint.Visible = false;
                btnChooseImage.Visible = false;
            }
            catch
            {
                MessageBox.Show("Cannot load this image!");
            }
        }

        // ✅ NEW: load from blob -> preview only
        private void LoadImageFromBlob(byte[] blob)
        {
            try
            {
                if (blob == null || blob.Length == 0)
                {
                    ClearImageBox();
                    return;
                }

                using (var ms = new MemoryStream(blob))
                using (var img = Image.FromStream(ms))
                {
                    picPreview.Image = new Bitmap(img);
                }

                picPreview.Visible = true;
                lblDropHint.Visible = false;
                btnChooseImage.Visible = false;
            }
            catch
            {
                ClearImageBox();
            }
        }

        // ✅ NEW: basic mime mapping
        private string GetMimeFromExtension(string ext)
        {
            ext = (ext ?? "").ToLowerInvariant();
            if (ext == ".png") return "image/png";
            if (ext == ".gif") return "image/gif";
            if (ext == ".bmp") return "image/bmp";
            return "image/jpeg";
        }

        // ✅ CHANGED: clear blob state
        private void ClearImageBox()
        {
            picPreview.Image = null;

            selectedImageBlob = null;
            selectedImageMime = null;

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

        private Button MakeButton(string text, Color backColor)
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

                cboSize.Enabled = false;
                cboColor.Enabled = false;
                nudPrice.Enabled = false;
                nudStock.Enabled = false;
                cboCategory.Enabled = false;
                cboGender.Enabled = false;

                Text += " (View only)";
            }
        }

        private void LoadGrid() => ReloadProducts();

        private Product ReadForm()
        {
            decimal price = nudPrice.Value;

            // nếu user bấm Add/Update khi đang focus ở nudPrice (lúc Enter đã chia 1000)
            if (nudPrice.Focused)
                price *= 1000;

            return new Product
            {
                Code = txtCode.Text.Trim(),
                Name = txtName.Text.Trim(),
                CategoryId = cboCategory.SelectedValue == null ? 0 : Convert.ToInt32(cboCategory.SelectedValue),
                ColorId = cboColor.SelectedValue == null ? 0 : Convert.ToInt32(cboColor.SelectedValue),
                Gender = cboGender.Text,
                Size = cboSize.Text,
                //Price = price,
                Stock = (int)nudStock.Value,
                Price = nudPrice.Value * 1000m,

                // ✅ CHANGED: store blob + mime
                ImageBlob = selectedImageBlob,
                ImageMime = selectedImageMime
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
            cboSize.Text = r.Cells["size"].Value?.ToString();
            cboColor.Text = r.Cells["color_name"].Value?.ToString();
            cboGender.Text = r.Cells["gender"].Value?.ToString();
            cboCategory.Text = r.Cells["category_name"].Value?.ToString();

            var priceStr = r.Cells["price"].Value?.ToString();
            if (decimal.TryParse(priceStr, NumberStyles.Number, CultureInfo.CurrentCulture, out var price))
            {
                var display = price / 1000m;      // 261,000 -> 261
                nudPrice.Value = Math.Min(nudPrice.Maximum, display);
            }


            if (int.TryParse(r.Cells["stock"].Value?.ToString(), out var stock))
                nudStock.Value = Math.Min(nudStock.Maximum, stock);

            // ✅ CHANGED: load image from DB (BLOB) not from image_path
            ClearImageBox();

            var code = txtCode.Text.Trim();
            if (!string.IsNullOrWhiteSpace(code))
            {
                var img = service.GetImageByCode(code); // Tuple<byte[], string>
                if (img != null && img.Item1 != null && img.Item1.Length > 0)
                {
                    LoadImageFromBlob(img.Item1);

                    // giữ blob để update không làm mất ảnh nếu user không chọn ảnh mới
                    selectedImageBlob = img.Item1;
                    selectedImageMime = img.Item2;
                }
            }
        }

        // tạo nút "..." nhỏ gọn
        // Tạo icon nhỏ (PictureBox) thay cho nút "..."
        private PictureBox CreateManageIcon(Image icon, EventHandler onClick)
        {
            var pic = new PictureBox
            {
                Image = icon,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 16,          // <-- CHỈNH độ to icon ở đây
                Height = 16,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            pic.Click += onClick;
            return pic;
        }

        // Bọc ComboBox ngắn + khoảng trắng + icon nhỏ bên phải
        private Panel WrapComboWithIcon(ComboBox cbo, Image icon, EventHandler onClick, int comboWidth = 190, int gap = 6)
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = Padding.Empty
            };

            // ComboBox ngắn lại (không Fill)
            cbo.DropDownStyle = ComboBoxStyle.DropDownList;
            cbo.Dock = DockStyle.Left;
            cbo.Width = comboWidth;   // <-- CHỈNH độ ngắn combo ở đây
            cbo.IntegralHeight = false;
            cbo.Height = 28;

            // khoảng trắng nhỏ giữa combo và icon
            var spacer = new Panel
            {
                Dock = DockStyle.Left,
                Width = gap           // <-- CHỈNH khoảng trắng ở đây
            };

            var manageIcon = CreateManageIcon(icon, onClick);

            pnl.Controls.Add(manageIcon);
            pnl.Controls.Add(spacer);
            pnl.Controls.Add(cbo);

            return pnl;
        }

        private void LoadColors()
        {
            var dt = service.GetColors();      // lấy từ ProductService
            cboColor.DataSource = dt;
            cboColor.DisplayMember = "color_name";
            cboColor.ValueMember = "color_id";
            cboColor.SelectedIndex = dt.Rows.Count > 0 ? 0 : -1;
        }

        private bool IsAdmin()
        {
            return current != null &&
                   current.Role != null &&
                   current.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        private void LoadCategories()
        {
            var dt = service.GetCategories();
            cboCategory.DataSource = dt;
            cboCategory.DisplayMember = "category_name";
            cboCategory.ValueMember = "category_id";
            cboCategory.SelectedIndex = dt.Rows.Count > 0 ? 0 : -1;
        }

        // Màu header mặc định (đang dùng trong StyleGrid)
        private readonly Color HeaderBackNormal = Color.FromArgb(245, 245, 245);
        private readonly Color HeaderForeNormal = Color.Black;

        // Màu header khi active
        private readonly Color HeaderBackActive = Color.FromArgb(33, 150, 243); // xanh primary
        private readonly Color HeaderForeActive = Color.White;

        private void HighlightCurrentColumnHeader()
        {
            if (dgv == null || dgv.Columns.Count == 0) return;

            // reset tất cả header về normal
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                col.HeaderCell.Style.BackColor = HeaderBackNormal;
                col.HeaderCell.Style.ForeColor = HeaderForeNormal;
                col.HeaderCell.Style.Font = new Font("Segoe UI Semibold", 10f);
            }

            // lấy cột đang được focus
            var curCell = dgv.CurrentCell;
            if (curCell == null) return;

            var activeCol = dgv.Columns[curCell.ColumnIndex];
            activeCol.HeaderCell.Style.BackColor = HeaderBackActive;
            activeCol.HeaderCell.Style.ForeColor = HeaderForeActive;
        }
    }
}
