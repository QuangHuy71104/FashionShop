using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FashionShop.BLL;
using FashionShop.DTO;

namespace FashionShop.GUI
{
    public class FrmOrders : Form
    {
        private Account current;
        private ProductService productService = new ProductService();
        private CustomerService customerService = new CustomerService();
        private OrderService orderService = new OrderService();

        private DataGridView dgvProducts, dgvCart;
        private ComboBox cboCustomers;
        private NumericUpDown nudQty;
        private Label lblTotal, lblPoints;
        private TextBox txtSearch;
        private Button btnAdd, btnRemove, btnCheckout, btnSearch;
        private Button btnNewCustomer;


        private int? selectedProductId = null;   // sản phẩm đang chọn
        private bool isUpdateMode = false;       // đang ở chế độ update hay add


        // ===== Payment/Points UI labels =====
        private Label lblRawTotal, lblDiscount, lblDiscountHint, lblFinalTotal;
        private Label lblOldPoints, lblEarnPoints, lblAfterPoints;
        private int oldPoints = 0;
        private decimal discountRate = 0m;

        private List<OrderDetail> cart = new List<OrderDetail>();
        private DataTable productsTable;     // bảng gốc
        private DataView productsView;       // view để search

        // ===== search helpers =====
        private string colCode;
        private string colName;
        private readonly string hint = "Enter product name...";

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

        public FrmOrders(Account acc)
        {
            current = acc;

            // ===== Form base giống Products =====
            Text = "Sales / Orders";
            MinimumSize = new Size(1300, 650);
            Size = new Size(1500, 720);
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

            // ================= LEFT: PRODUCTS =================
            split.Panel1.Padding = new Padding(12);

            var gbProducts = new GroupBox
            {
                Text = "Products",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 10.5f),
                Padding = new Padding(10)
            };
            split.Panel1.Controls.Add(gbProducts);


            // Search bar
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

                // GIẢM margin phải để cân với nút đỏ
                Margin = new Padding(0, 6, 4, 6)
            };

            btnSearch = MakeButton("", Color.FromArgb(244, 67, 54));
            btnSearch.Dock = DockStyle.Fill;
            btnSearch.Margin = new Padding(0, 6, 0, 6);

            // add vào layout
            pnlSearch.Controls.Add(txtSearch, 0, 0);
            pnlSearch.Controls.Add(btnSearch, 1, 0);

            // icon trong nút đỏ: CANH GIỮA cho cân dọc
            btnSearch.Image = ResizeImage(Properties.Resources.delete, 18, 18);
            btnSearch.ImageAlign = ContentAlignment.MiddleCenter;
            btnSearch.TextImageRelation = TextImageRelation.Overlay;
            btnSearch.Padding = new Padding(0);


            // Placeholder
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

            // ===== Events search mới =====
            btnSearch.Click += (s, e) => {
                // xóa text, trả về hint
                txtSearch.Text = hint;
                txtSearch.ForeColor = Color.Gray;

                // reset filter & hiện full list
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

            // Grid products
            dgvProducts = new DataGridView
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
            StyleGrid(dgvProducts);
            dgvProducts.CellClick += DgvProducts_CellClick;


            // Bottom qty + add
            var pnlBottomLeft = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(0, 20, 0, 0),
                BackColor = Color.Transparent
            };

            var lblQty = new Label
            {
                Text = "Qty:",
                AutoSize = true,
                Location = new Point(0, 12)
            };

            nudQty = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 999,
                Value = 1,
                Width = 90,
                Location = new Point(45, 8)
            };

            nudQty.ValueChanged += (s, e) =>
            {
                if (nudQty.Value < 1) nudQty.Value = 1;
            };


            btnAdd = MakeButton("Add to Cart", Color.FromArgb(33, 150, 243));
            btnAdd.Width = 150;
            btnAdd.Height = 32;
            btnAdd.Location = new Point(150, 6);

            pnlBottomLeft.Controls.AddRange(new Control[] { lblQty, nudQty, btnAdd });

            // Thứ tự add: Top -> Fill -> Bottom (để dock chuẩn)
            gbProducts.Controls.Add(dgvProducts);
            gbProducts.Controls.Add(pnlBottomLeft);
            gbProducts.Controls.Add(pnlSearch);

            // ================= RIGHT: CART / CHECKOUT =================
            split.Panel2.Padding = new Padding(12);

            var gbCart = new GroupBox
            {
                Text = "Cart",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 10.5f),
                Padding = new Padding(10)
            };
            split.Panel2.Controls.Add(gbCart);

            // Grid cart
            dgvCart = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 320,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };
            StyleGrid(dgvCart);
            dgvCart.CellClick += DgvCart_CellClick;


            // ================= Payment Summary =================
            var pnlInfo = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(0, 10, 0, 0)
            };
            pnlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            pnlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // rows
            pnlInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 38)); // Customer
            pnlInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Sub total
            pnlInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Discount
            pnlInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 24)); // Discount hint
            pnlInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 42)); // Final total
            pnlInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); // Old points
            pnlInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); // Earn points
            pnlInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // After points

            // --- Row 0: Customer ---
            pnlInfo.Controls.Add(new Label
            {
                Text = "Customer",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            // panel chứa combo + nút New
            var pnlCustomer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Margin = new Padding(0)
            };
            pnlCustomer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlCustomer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // rộng nút

            cboCustomers = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            btnNewCustomer = MakeButton("New Customer", Color.FromArgb(33, 150, 243));

            // KHÔNG fill nữa để khỏi bị cao
            btnNewCustomer.Dock = DockStyle.None;
            btnNewCustomer.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            // set size gọn lại
            btnNewCustomer.Width = 110;
            btnNewCustomer.Height = cboCustomers.Height - 2;   // hoặc bạn set cứng 28
            btnNewCustomer.Margin = new Padding(6, 4, 0, 4);
            btnNewCustomer.Font = new Font("Segoe UI Semibold", 9.5f);

            cboCustomers = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 6, 0, 6)  // canh giữa dọc giống nút
            };


            pnlCustomer.Controls.Add(cboCustomers, 0, 0);
            pnlCustomer.Controls.Add(btnNewCustomer, 1, 0);

            pnlInfo.Controls.Add(pnlCustomer, 1, 0);


            // --- Row 1: Sub total ---
            pnlInfo.Controls.Add(new Label
            {
                Text = "Sub total",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 1);

            lblRawTotal = new Label
            {
                Text = "0 đ",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlInfo.Controls.Add(lblRawTotal, 1, 1);

            // --- Row 2: Discount ---
            pnlInfo.Controls.Add(new Label
            {
                Text = "Discount",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 2);

            lblDiscount = new Label
            {
                Text = "0%",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(244, 67, 54)
            };
            pnlInfo.Controls.Add(lblDiscount, 1, 2);

            // --- Row 3: Discount hint (span 2 cols) ---
            lblDiscountHint = new Label
            {
                Text = "Need 100 points to get 10% off",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                Padding = new Padding(0, 0, 0, 6)
            };
            pnlInfo.Controls.Add(lblDiscountHint, 0, 3);
            pnlInfo.SetColumnSpan(lblDiscountHint, 2);

            // --- Row 4: Final total ---
            pnlInfo.Controls.Add(new Label
            {
                Text = "Final total",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 4);

            lblFinalTotal = new Label
            {
                Text = "0 đ",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(33, 33, 33)
            };
            pnlInfo.Controls.Add(lblFinalTotal, 1, 4);

            // --- Row 5: Old points ---
            pnlInfo.Controls.Add(new Label
            {
                Text = "Old points",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 5);

            lblOldPoints = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlInfo.Controls.Add(lblOldPoints, 1, 5);

            // --- Row 6: Earn points ---
            pnlInfo.Controls.Add(new Label
            {
                Text = "Earned points",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 6);

            lblEarnPoints = new Label
            {
                Text = "+0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlInfo.Controls.Add(lblEarnPoints, 1, 6);

            // --- Row 7: After points ---
            pnlInfo.Controls.Add(new Label
            {
                Text = "After checkout",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 7);

            lblAfterPoints = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            pnlInfo.Controls.Add(lblAfterPoints, 1, 7);


            // Buttons bottom right
            var btnGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 10, 0, 0)
            };
            btnGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            btnGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            btnRemove = MakeButton("Remove", Color.FromArgb(244, 67, 54));
            btnCheckout = MakeButton("Checkout", Color.FromArgb(76, 175, 80));
            btnRemove.Dock = DockStyle.Fill;
            btnCheckout.Dock = DockStyle.Fill;

            btnGrid.Controls.Add(btnRemove, 0, 0);
            btnGrid.Controls.Add(btnCheckout, 1, 0);

            gbCart.Controls.Add(btnGrid);
            gbCart.Controls.Add(pnlInfo);
            gbCart.Controls.Add(dgvCart);

            // ===== Split width đẹp lúc mở =====
            Shown += (s, e) =>
            {
                split.Panel1MinSize = 500;
                split.Panel2MinSize = 500;

                //chia 50/50
                int half = split.Width / 2;

                // đảm bảo không nhỏ hơn min size
                int maxLeft = split.Width - split.Panel2MinSize;
                split.SplitterDistance = Math.Max(split.Panel1MinSize,
                                          Math.Min(half, maxLeft));
            };


            // ===== Events logic cũ =====
            btnAdd.Click += AddToCart;
            btnRemove.Click += RemoveFromCart;
            btnCheckout.Click += Checkout;
            btnNewCustomer.Click += (s, e) =>
            {
                // mở form Customers để thêm mới
                using (var f = new FrmCustomers())
                {
                    f.ShowDialog();
                }


                // reload list khách sau khi đóng form
                var dt = customerService.GetForCombo();
                cboCustomers.DataSource = dt;
                cboCustomers.DisplayMember = "customer_name";
                cboCustomers.ValueMember = "customer_id";

                // auto chọn khách mới nhất (dòng cuối)
                if (dt.Rows.Count > 0)
                    cboCustomers.SelectedIndex = dt.Rows.Count - 1;
                else
                    cboCustomers.SelectedIndex = -1;

                RefreshCart();
            };


            // Load data
            Load += (s, e) =>
            {
                ReloadProducts();

                cboCustomers.DataSource = customerService.GetForCombo();
                cboCustomers.DisplayMember = "customer_name";
                cboCustomers.ValueMember = "customer_id";
                cboCustomers.SelectedIndex = -1;

                // ===== GẮN EVENT Ở ĐÂY =====
                cboCustomers.SelectedIndexChanged += (s2, e2) =>
                {
                    if (cboCustomers.SelectedIndex >= 0)
                    {
                        int cid2 = Convert.ToInt32(cboCustomers.SelectedValue);
                        oldPoints = customerService.GetPoints(cid2);
                    }
                    else oldPoints = 0;

                    RefreshCart(); // tính lại total/points/discount theo khách mới
                };

                RefreshCart();
            };
        }

        // ================= SEARCH CORE =================

        private void ReloadProducts()
        {
            productsTable = productService.GetForSale();
            ResolveProductColumns();

            productsView = productsTable.DefaultView;
            dgvProducts.DataSource = productsView;

            SetupAutoComplete();
            ApplySearch(); // nếu đang có chữ trong ô search thì lọc lại
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
            if (key == hint) key = "";

            productsView.RowFilter = "";

            if (!string.IsNullOrWhiteSpace(key))
            {
                key = key.Replace("'", "''");

                if (!string.IsNullOrEmpty(colCode) && !string.IsNullOrEmpty(colName))
                {
                    productsView.RowFilter =
                        $"[{colCode}] LIKE '%{key}%' OR [{colName}] LIKE '%{key}%'";
                }
                else if (!string.IsNullOrEmpty(colName))
                {
                    productsView.RowFilter = $"[{colName}] LIKE '%{key}%'";
                }
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

        // ================= UI HELPERS =================
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

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FrmOrders
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "FrmOrders";
            this.Load += new System.EventHandler(this.FrmOrders_Load);
            this.ResumeLayout(false);

        }

        private void FrmOrders_Load(object sender, EventArgs e)
        {

        }

        // ================= BUSINESS LOGIC =================
        private void AddToCart(object sender, EventArgs e)
        {
            // ưu tiên sp đang chọn (từ products hoặc cart)
            int pid;

            if (selectedProductId.HasValue)
                pid = selectedProductId.Value;
            else
            {
                if (dgvProducts.CurrentRow == null) return;
                pid = Convert.ToInt32(dgvProducts.CurrentRow.Cells["product_id"].Value);
            }

            // lấy row sản phẩm từ productsTable để chắc đúng dữ liệu
            var prodRow = productsTable.AsEnumerable()
                .FirstOrDefault(x => x.Field<int>("product_id") == pid);

            if (prodRow == null) return;

            string pname = prodRow["product_name"].ToString();
            decimal price = Convert.ToDecimal(prodRow["price"]);
            string size = prodRow.Table.Columns.Contains("size") ? prodRow["size"].ToString() : "";
            string color = prodRow.Table.Columns.Contains("color") ? prodRow["color"].ToString() : "";
            int stock = Convert.ToInt32(prodRow["stock"]);
            int qty = (int)nudQty.Value;

            if (qty > stock)
            {
                MessageBox.Show("Not enough stock");
                return;
            }

            var ex = cart.FirstOrDefault(x => x.ProductId == pid);

            if (isUpdateMode && ex != null)
            {
                ex.Quantity = qty; // update qty
            }
            else
            {
                if (ex != null)
                {
                    if (ex.Quantity + qty > stock)
                    {
                        MessageBox.Show("Over stock");
                        return;
                    }
                    ex.Quantity += qty;
                }
                else
                {
                    cart.Add(new OrderDetail
                    {
                        ProductId = pid,
                        ProductName = pname,
                        Quantity = qty,
                        UnitPrice = price,
                        Size = size,
                        Color = color,
                        Stock = stock
                    });
                }
            }

            RefreshCart();

            // reset mode
            nudQty.Value = 1;
            isUpdateMode = false;
            selectedProductId = null;
            btnAdd.Text = "Add to Cart";
            btnAdd.BackColor = Color.FromArgb(33, 150, 243);
            // Enter = Add to Cart
            this.AcceptButton = btnAdd;

        }


        private void RemoveFromCart(object sender, EventArgs e)
        {
            if (dgvCart.CurrentRow == null) return;

            int pid = Convert.ToInt32(dgvCart.CurrentRow.Cells["ProductId"].Value);
            cart.RemoveAll(x => x.ProductId == pid);

            RefreshCart();

            isUpdateMode = false;
            selectedProductId = null;
            btnAdd.Text = "Add to Cart";
            btnAdd.BackColor = Color.FromArgb(33, 150, 243);
            nudQty.Value = 1;

        }

        private void RefreshCart()
        {
            dgvCart.DataSource = null;

            dgvCart.DataSource = cart.Select(x => new
            {
                x.ProductId,
                x.ProductName,

                // thêm 3 cột giống Products
                x.Size,
                x.Color,
                x.Stock,

                x.Quantity,
                x.UnitPrice,
                x.SubTotal
            }).ToList();

            // (tuỳ chọn) rename header cho đẹp
            if (dgvCart.Columns["ProductId"] != null) dgvCart.Columns["ProductId"].HeaderText = "ProductId";
            if (dgvCart.Columns["ProductName"] != null) dgvCart.Columns["ProductName"].HeaderText = "ProductName";
            if (dgvCart.Columns["UnitPrice"] != null) dgvCart.Columns["UnitPrice"].HeaderText = "UnitPrice";
            if (dgvCart.Columns["SubTotal"] != null) dgvCart.Columns["SubTotal"].HeaderText = "SubTotal";

            // ===== tính tiền/points như cũ =====
            decimal rawTotal = cart.Sum(x => x.SubTotal);

            int newPoints = (int)(rawTotal / 50000m); // 1 point = 50,000đ
            int afterPoints = oldPoints + newPoints;

            discountRate = afterPoints >= 100 ? 0.10m : 0m;
            decimal finalTotal = rawTotal * (1 - discountRate);

            lblRawTotal.Text = $"{rawTotal:N0} đ";

            lblDiscount.Text = discountRate > 0 ? "10%" : "0%";
            lblDiscount.ForeColor = discountRate > 0
                ? Color.FromArgb(76, 175, 80)
                : Color.FromArgb(244, 67, 54);

            lblDiscountHint.Text = afterPoints >= 100
                ? "Eligible for 10% off"
                : "Need 100 points to get 10% off";
            lblDiscountHint.ForeColor = afterPoints >= 100
                ? Color.FromArgb(76, 175, 80)
                : Color.Gray;

            lblFinalTotal.Text = $"{finalTotal:N0} đ";

            lblOldPoints.Text = oldPoints.ToString("N0");
            lblEarnPoints.Text = "+" + newPoints.ToString("N0");
            lblAfterPoints.Text = afterPoints.ToString("N0");
        }


        private void Checkout(object sender, EventArgs e)
        {
            int? cid = null;
            if (cboCustomers.SelectedIndex >= 0)
                cid = Convert.ToInt32(cboCustomers.SelectedValue);

            if (cart.Count == 0)
            {
                MessageBox.Show("Cart is empty!");
                return;
            }

            // ===== raw total =====
            decimal rawTotal = cart.Sum(x => x.SubTotal);

            // ===== points mới theo rule =====
            int newPoints = (int)(rawTotal / 50000m);

            // ===== tổng points sau mua =====
            int totalPointsAfter = oldPoints + newPoints;

            // ===== discount nếu đủ 100 points =====
            decimal discountRate = totalPointsAfter >= 100 ? 0.10m : 0m;
            decimal finalTotal = rawTotal * (1 - discountRate);

            // ===== checkout order =====
            int orderId = orderService.Checkout(current.EmployeeId, cid, cart, out string err);
            if (orderId < 0)
            {
                MessageBox.Show(err);
                return;
            }

            // ===== cộng points mới cho customer =====
            if (cid.HasValue)
            {
                int used = totalPointsAfter >= 100 ? 100 : 0;

                int finalPoints = oldPoints + newPoints - used;
                if (finalPoints < 0) finalPoints = 0;

                if (!customerService.SetPoints(cid.Value, finalPoints, out string perr))
                    MessageBox.Show("Checkout OK but update points failed: " + perr);
            }

            MessageBox.Show(
                $"Checkout success! Order ID: {orderId}\n" +
                $"Raw total: {rawTotal:N0} đ\n" +
                $"Discount: {(discountRate * 100):0}%\n" +
                $"Final total: {finalTotal:N0} đ\n" +
                $"Old points: {oldPoints}\n" +
                $"Earned: +{newPoints}\n" +
                $"After: {totalPointsAfter}"
            );

            cart.Clear();
            ReloadProducts();

            // reset lại oldPoints nếu bạn muốn về 0 khi bỏ chọn khách
            // oldPoints = 0;

            isUpdateMode = false;
            selectedProductId = null;
            btnAdd.Text = "Add to Cart";
            btnAdd.BackColor = Color.FromArgb(33, 150, 243);
            nudQty.Value = 1;


            RefreshCart();
        }


        // Click 1 dòng sản phẩm: nudQty = qty trong cart nếu có, không có thì =1
        private void DgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var r = dgvProducts.Rows[e.RowIndex];

            int pid = Convert.ToInt32(r.Cells["product_id"].Value);
            int stock = Convert.ToInt32(r.Cells["stock"].Value);

            selectedProductId = pid;

            nudQty.Minimum = 1;
            nudQty.Maximum = Math.Max(1, stock);

            var ex = cart.FirstOrDefault(x => x.ProductId == pid);

            if (ex != null)
            {
                // đã có trong cart → đồng bộ qty + chuyển mode Update
                nudQty.Value = Math.Min(ex.Quantity, nudQty.Maximum);

                isUpdateMode = true;
                btnAdd.Text = "Update Qty";
                btnAdd.BackColor = Color.FromArgb(255, 152, 0); // cam cho dễ phân biệt
            }
            else
            {
                //chưa có → mode Add
                nudQty.Value = 1;

                isUpdateMode = false;
                btnAdd.Text = "Add to Cart";
                btnAdd.BackColor = Color.FromArgb(33, 150, 243); // xanh như cũ
            }
        }


        // Click 1 dòng trong cart: nudQty nhảy theo qty hiện tại + chuyển sang update mode
        private void DgvCart_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int pid = Convert.ToInt32(dgvCart.Rows[e.RowIndex].Cells["ProductId"].Value);

            var ex = cart.FirstOrDefault(x => x.ProductId == pid);
            if (ex == null)
            {
                nudQty.Value = 1;
                return;
            }

            selectedProductId = pid; // lưu lại sp đang chọn

            // lấy stock từ productsTable để set Maximum
            int stock = 999;
            var prodRow = productsTable?.AsEnumerable()
                .FirstOrDefault(x => x.Field<int>("product_id") == pid);

            if (prodRow != null)
                stock = prodRow.Field<int>("stock");

            nudQty.Minimum = 1;
            nudQty.Maximum = Math.Max(1, stock);
            nudQty.Value = Math.Min(ex.Quantity, nudQty.Maximum);

            // chuyển sang update mode giống click product
            isUpdateMode = true;
            btnAdd.Text = "Update Qty";
            btnAdd.BackColor = Color.FromArgb(255, 152, 0);
        }

    }
}
