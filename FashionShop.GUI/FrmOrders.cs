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
        Account current;
        ProductService productService = new ProductService();
        CustomerService customerService = new CustomerService();
        OrderService orderService = new OrderService();

        DataGridView dgvProducts, dgvCart;
        ComboBox cboCustomers;
        NumericUpDown nudQty;
        Label lblTotal;

        List<OrderDetail> cart = new List<OrderDetail>();
        DataTable productsTable;

        public FrmOrders(Account acc)
        {
            current = acc;
            Text = "Sales / Orders";
            Size = new Size(1100, 650);
            StartPosition = FormStartPosition.CenterScreen;

            // left products
            dgvProducts = new DataGridView
            {
                Location = new Point(10, 50),
                Size = new Size(520, 450),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // right cart
            dgvCart = new DataGridView
            {
                Location = new Point(560, 50),
                Size = new Size(520, 300),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var lblCus = new Label { Text = "Customer:", Location = new Point(560, 370), AutoSize = true };
            cboCustomers = new ComboBox { Location = new Point(640, 365), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblQty = new Label { Text = "Qty:", Location = new Point(10, 515), AutoSize = true };
            nudQty = new NumericUpDown { Location = new Point(55, 510), Width = 80, Minimum = 1, Maximum = 999, Value = 1 };

            var btnAdd = new Button { Text = "Add to Cart", Location = new Point(150, 507), Width = 120 };
            var btnRemove = new Button { Text = "Remove", Location = new Point(560, 510), Width = 90 };
            var btnCheckout = new Button { Text = "Checkout", Location = new Point(960, 510), Width = 120 };

            lblTotal = new Label
            {
                Text = "Total: 0 đ",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(560, 420),
                AutoSize = true
            };

            btnAdd.Click += AddToCart;
            btnRemove.Click += RemoveFromCart;
            btnCheckout.Click += Checkout;

            Controls.AddRange(new Control[]{
                new Label{Text="Products",Location=new Point(10,15),AutoSize=true,Font=new Font("Segoe UI",11,FontStyle.Bold)},
                new Label{Text="Cart",Location=new Point(560,15),AutoSize=true,Font=new Font("Segoe UI",11,FontStyle.Bold)},
                dgvProducts,dgvCart,lblCus,cboCustomers,lblQty,nudQty,
                btnAdd,btnRemove,btnCheckout,lblTotal
            });

            Load += (s, e) => {
                productsTable = productService.GetForSale();
                dgvProducts.DataSource = productsTable;

                cboCustomers.DataSource = customerService.GetForCombo();
                cboCustomers.DisplayMember = "customer_name";
                cboCustomers.ValueMember = "customer_id";
                cboCustomers.SelectedIndex = -1;

                RefreshCart();
            };
        }

        void AddToCart(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;
            int pid = Convert.ToInt32(dgvProducts.CurrentRow.Cells["product_id"].Value);
            string pname = dgvProducts.CurrentRow.Cells["product_name"].Value.ToString();
            decimal price = Convert.ToDecimal(dgvProducts.CurrentRow.Cells["price"].Value);
            int stock = Convert.ToInt32(dgvProducts.CurrentRow.Cells["stock"].Value);
            int qty = (int)nudQty.Value;
            if (qty > stock) { MessageBox.Show("Not enough stock"); return; }

            var ex = cart.FirstOrDefault(x => x.ProductId == pid);
            if (ex != null) { if (ex.Quantity + qty > stock) { MessageBox.Show("Over stock"); return; } ex.Quantity += qty; }
            else cart.Add(new OrderDetail { ProductId = pid, ProductName = pname, Quantity = qty, UnitPrice = price });

            RefreshCart();
        }

        void RemoveFromCart(object sender, EventArgs e)
        {
            if (dgvCart.CurrentRow == null) return;
            int pid = Convert.ToInt32(dgvCart.CurrentRow.Cells["ProductId"].Value);
            cart.RemoveAll(x => x.ProductId == pid);
            RefreshCart();
        }

        void RefreshCart()
        {
            dgvCart.DataSource = null;
            dgvCart.DataSource = cart.Select(x => new {
                x.ProductId,
                x.ProductName,
                x.Quantity,
                x.UnitPrice,
                x.SubTotal
            }).ToList();

            lblTotal.Text = "Total: " + cart.Sum(x => x.SubTotal).ToString("N0") + " đ";
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FrmOrders
            // 
            this.ClientSize = new System.Drawing.Size(274, 229);
            this.Name = "FrmOrders";
            this.Load += new System.EventHandler(this.FrmOrders_Load);
            this.ResumeLayout(false);

        }

        private void FrmOrders_Load(object sender, EventArgs e)
        {

        }

        void Checkout(object sender, EventArgs e)
        {
            int? cid = null;
            if (cboCustomers.SelectedIndex >= 0) cid = Convert.ToInt32(cboCustomers.SelectedValue);

            int orderId = orderService.Checkout(current.EmployeeId, cid, cart, out string err);
            if (orderId < 0) { MessageBox.Show(err); return; }

            MessageBox.Show("Checkout success! Order ID: " + orderId);
            cart.Clear();
            productsTable = productService.GetForSale();
            dgvProducts.DataSource = productsTable;
            RefreshCart();
        }
    }
}
