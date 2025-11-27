using System;
using System.Drawing;
using System.Windows.Forms;
using FashionShop.DTO;
using FashionShop.BLL;

namespace FashionShop.GUI
{
    public class FrmMain : Form
    {
        private Account current;
        private ProductService productService = new ProductService();
        private CustomerService customerService = new CustomerService();

        Label lblProducts, lblCustomers, lblRevenue;

        public FrmMain(Account acc)
        {
            current = acc;
            Text = $"Fashion Shop - {acc.EmployeeName} ({acc.Role})";
            WindowState = FormWindowState.Maximized;

            // ===== Menu =====
            var menu = new MenuStrip();
            var mProducts = new ToolStripMenuItem("Products");
            var mCustomers = new ToolStripMenuItem("Customers");
            var mOrders = new ToolStripMenuItem("Sales / Orders");
            var mLogout = new ToolStripMenuItem("Logout");

            // Sau khi form con đóng -> refresh dashboard
            mProducts.Click += (s, e) =>
            {
                new FrmProducts().ShowDialog();
                RefreshDashboard();
            };
            mCustomers.Click += (s, e) =>
            {
                new FrmCustomers().ShowDialog();
                RefreshDashboard();
            };
            mOrders.Click += (s, e) =>
            {
                new FrmOrders(current).ShowDialog();
                RefreshDashboard();
            };
            mLogout.Click += (s, e) => Close();

            menu.Items.AddRange(new ToolStripItem[] { mProducts, mCustomers, mOrders, mLogout });
            MainMenuStrip = menu;
            Controls.Add(menu);

            // ===== Title =====
            var lblTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(30, 60)
            };
            Controls.Add(lblTitle);

            // ===== Cards =====
            lblProducts = MakeCard("Total Products", "0", 30, 130);
            lblCustomers = MakeCard("Total Customers", "0", 280, 130);
            lblRevenue = MakeCard("Revenue (demo)", "0 đ", 530, 130);

            // ===== Quick buttons =====
            var btnP = new Button { Text = "Open Products", Location = new Point(30, 260), Width = 180, Height = 45 };
            var btnC = new Button { Text = "Open Customers", Location = new Point(280, 260), Width = 180, Height = 45 };
            var btnO = new Button { Text = "Open Sales", Location = new Point(530, 260), Width = 180, Height = 45 };

            btnP.Click += (s, e) =>
            {
                new FrmProducts().ShowDialog();
                RefreshDashboard();
            };
            btnC.Click += (s, e) =>
            {
                new FrmCustomers().ShowDialog();
                RefreshDashboard();
            };
            btnO.Click += (s, e) =>
            {
                new FrmOrders(current).ShowDialog();
                RefreshDashboard();
            };

            Controls.AddRange(new Control[] { btnP, btnC, btnO });

            Load += (s, e) => RefreshDashboard();

            // Mỗi lần quay lại FrmMain -> auto refresh
            Activated += (s, e) => RefreshDashboard();
        }

        private void RefreshDashboard()
        {
            try
            {
                int pCount = productService.GetAll().Rows.Count;
                int cCount = customerService.GetAll().Rows.Count;

                lblProducts.Text = pCount.ToString();
                lblCustomers.Text = cCount.ToString();

                // doanh thu hiện để demo 0 (muốn tính thật thì mình viết tiếp)
                lblRevenue.Text = "0 đ";
            }
            catch
            {
                lblProducts.Text = "0";
                lblCustomers.Text = "0";
                lblRevenue.Text = "0 đ";
            }
        }

        private Label MakeCard(string title, string value, int x, int y)
        {
            var panel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(220, 110),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblT = new Label
            {
                Text = title,
                Location = new Point(12, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            var lblV = new Label
            {
                Text = value,
                Location = new Point(12, 45),
                AutoSize = true,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            panel.Controls.Add(lblT);
            panel.Controls.Add(lblV);
            Controls.Add(panel);

            return lblV;
        }
    }
}
