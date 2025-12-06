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
        private Label lblTotal;
        private TextBox txtSearch;
        private Button btnAdd, btnRemove, btnCheckout, btnSearch;

        private List<OrderDetail> cart = new List<OrderDetail>();
        private DataTable productsTable;     // bảng gốc
        private DataView productsView;       // view để search

        public FrmOrders(Account acc)
        {
            current = acc;

            // ===== Form base giống Products =====
            Text = "Sales / Orders";
            MinimumSize = new Size(1100, 650);
            Size = new Size(1250, 720);
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
                Height = 46,
                ColumnCount = 2
            };
            pnlSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
            pnlSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12f),
                Margin = new Padding(0, 6, 8, 6)
            };

            btnSearch = MakeButton("Search", Color.FromArgb(63, 81, 181));
            btnSearch.Dock = DockStyle.Fill;
            btnSearch.Margin = new Padding(0, 6, 0, 6);

            pnlSearch.Controls.Add(txtSearch, 0, 0);
            pnlSearch.Controls.Add(btnSearch, 1, 0);

            // Placeholder
            string hint = "Enter product code or name...";
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

            btnSearch.Click += (s, e) =>
            {
                if (productsView == null) return;

                var key = txtSearch.Text.Trim();
                if (key == hint) key = "";

                // reset filter trước
                productsView.RowFilter = "";

                if (!string.IsNullOrWhiteSpace(key))
                {
                    key = key.Replace("'", "''");
                    productsView.RowFilter =
                    $"[product_code] LIKE '%{key}%' OR [product_name] LIKE '%{key}%'";

                }

                dgvProducts.DataSource = productsView.ToTable();
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

            // Bottom qty + add
            var pnlBottomLeft = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(0, 8, 0, 0),
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

            // Customer + total area
            var pnlInfo = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(0, 10, 0, 0)
            };
            pnlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            pnlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            pnlInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            pnlInfo.Controls.Add(new Label
            {
                Text = "Customer",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            cboCustomers = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            pnlInfo.Controls.Add(cboCustomers, 1, 0);

            pnlInfo.Controls.Add(new Label
            {
                Text = "Total",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 1);

            lblTotal = new Label
            {
                Text = "Total: 0 đ",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(33, 33, 33)
            };
            pnlInfo.Controls.Add(lblTotal, 1, 1);

            // Buttons bottom right
            var btnGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
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
                split.Panel1MinSize = 420;
                split.Panel2MinSize = 520;

                int desiredLeft = 520;
                int maxLeft = split.Width - split.Panel2MinSize;
                split.SplitterDistance = Math.Max(split.Panel1MinSize,
                                          Math.Min(desiredLeft, maxLeft));
            };

            // ===== Events logic cũ =====
            btnAdd.Click += AddToCart;
            btnRemove.Click += RemoveFromCart;
            btnCheckout.Click += Checkout;

            // Load data
            Load += (s, e) =>
            {
                productsTable = productService.GetForSale();
                productsView = productsTable.DefaultView;

                dgvProducts.DataSource = productsTable;

                cboCustomers.DataSource = customerService.GetForCombo();
                cboCustomers.DisplayMember = "customer_name";
                cboCustomers.ValueMember = "customer_id";
                cboCustomers.SelectedIndex = -1;

                RefreshCart();
            };
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

        private void StyleGrid(DataGridView g)
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

        // ================= LOGIC GIỮ NGUYÊN =================
        private void AddToCart(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;

            int pid = Convert.ToInt32(dgvProducts.CurrentRow.Cells["product_id"].Value);
            string pname = dgvProducts.CurrentRow.Cells["product_name"].Value.ToString();
            decimal price = Convert.ToDecimal(dgvProducts.CurrentRow.Cells["price"].Value);
            int stock = Convert.ToInt32(dgvProducts.CurrentRow.Cells["stock"].Value);
            int qty = (int)nudQty.Value;

            if (qty > stock)
            {
                MessageBox.Show("Not enough stock");
                return;
            }

            var ex = cart.FirstOrDefault(x => x.ProductId == pid);
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
                    UnitPrice = price
                });
            }

            RefreshCart();
        }

        private void RemoveFromCart(object sender, EventArgs e)
        {
            if (dgvCart.CurrentRow == null) return;

            int pid = Convert.ToInt32(dgvCart.CurrentRow.Cells["ProductId"].Value);
            cart.RemoveAll(x => x.ProductId == pid);

            RefreshCart();
        }

        private void RefreshCart()
        {
            dgvCart.DataSource = null;
            dgvCart.DataSource = cart.Select(x => new
            {
                x.ProductId,
                x.ProductName,
                x.Quantity,
                x.UnitPrice,
                x.SubTotal
            }).ToList();

            lblTotal.Text = "Total: " + cart.Sum(x => x.SubTotal).ToString("N0") + " đ";
        }

        private void Checkout(object sender, EventArgs e)
        {
            int? cid = null;
            if (cboCustomers.SelectedIndex >= 0)
                cid = Convert.ToInt32(cboCustomers.SelectedValue);

            int orderId = orderService.Checkout(current.EmployeeId, cid, cart, out string err);
            if (orderId < 0)
            {
                MessageBox.Show(err);
                return;
            }

            MessageBox.Show("Checkout success! Order ID: " + orderId);

            cart.Clear();

            // reload stock mới
            productsTable = productService.GetForSale();
            productsView = productsTable.DefaultView;
            dgvProducts.DataSource = productsTable;

            RefreshCart();
        }
    }
}
