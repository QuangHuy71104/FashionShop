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

        Panel sidebar, topbar, content;
        Button btnHome, btnProducts, btnCustomers, btnOrders, btnLogout;

        public FrmMain(Account acc)
        {
            current = acc;
            Text = "Fashion Shop";
            WindowState = FormWindowState.Maximized;
            Font = new Font("Segoe UI", 10);
            BackColor = Color.FromArgb(245, 246, 250);

            BuildLayout();
            Load += (s, e) => RefreshDashboard();
            Activated += (s, e) => RefreshDashboard();
        }

        private void BuildLayout()
        {
            // ===== SIDEBAR =====
            sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(44, 62, 80)
            };
            Controls.Add(sidebar);

            // logo / shop name
            var lblLogo = new Label
            {
                Text = "FashionShop",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = false,
                Height = 60,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
            };
            sidebar.Controls.Add(lblLogo);

            // user info panel
            var userPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(52, 73, 94)
            };
            sidebar.Controls.Add(userPanel);

            var lblUser = new Label
            {
                Text = current.EmployeeName,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11),
                Dock = DockStyle.Top,
                Height = 28
            };
            var lblRole = new Label
            {
                Text = current.Role,
                ForeColor = Color.LightGray,
                Dock = DockStyle.Top
            };
            userPanel.Controls.Add(lblRole);
            userPanel.Controls.Add(lblUser);

            // menu buttons
            btnHome = MakeSidebarButton("Home");
            btnProducts = MakeSidebarButton("Products");
            btnCustomers = MakeSidebarButton("Customers");
            btnOrders = MakeSidebarButton("Sales / Orders");
            btnLogout = MakeSidebarButton("Logout");

            sidebar.Controls.AddRange(new Control[]
            {
                btnLogout, btnOrders, btnCustomers, btnProducts, btnHome
            });

            // events
            btnProducts.Click += (s, e) =>
            {
                new FrmProducts(current).ShowDialog();
                RefreshDashboard();
            };
            btnCustomers.Click += (s, e) =>
            {
                new FrmCustomers().ShowDialog();
                RefreshDashboard();
            };
            btnOrders.Click += (s, e) =>
            {
                new FrmOrders(current).ShowDialog();
                RefreshDashboard();
            };
            btnLogout.Click += (s, e) => Close();

            // ===== TOPBAR =====
            topbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };
            Controls.Add(topbar);

            var lblTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Dock = DockStyle.Left,
                AutoSize = true
            };

            var lblHello = new Label
            {
                Text = $"Hello, {current.EmployeeName} ({current.Role})",
                ForeColor = Color.Gray,
                Dock = DockStyle.Right,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };

            topbar.Controls.Add(lblHello);
            topbar.Controls.Add(lblTitle);

            // ===== CONTENT AREA =====
            content = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                BackColor = Color.FromArgb(245, 246, 250)
            };
            Controls.Add(content);

            // Row 1 - cards
            var cardsRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 140,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true
            };
            content.Controls.Add(cardsRow);

            var cardProducts = MakeCard("Items", "0", Color.FromArgb(52, 152, 219), out lblProducts);
            var cardCustomers = MakeCard("Customers", "0", Color.FromArgb(241, 196, 15), out lblCustomers);
            var cardRevenue = MakeCard("Revenue", "0 đ", Color.FromArgb(46, 204, 113), out lblRevenue);

            cardsRow.Controls.AddRange(new Control[] { cardProducts, cardCustomers, cardRevenue });

            // Row 2 - quick links
            var quickWrap = new GroupBox
            {
                Text = "Quick Links",
                Dock = DockStyle.Top,
                Height = 170,
                Padding = new Padding(12),
                BackColor = Color.White
            };
            content.Controls.Add(quickWrap);

            var quickRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };
            quickWrap.Controls.Add(quickRow);

            var qProducts = MakeQuickButton("Open Products", Color.FromArgb(52, 152, 219));
            var qCustomers = MakeQuickButton("Open Customers", Color.FromArgb(155, 89, 182));
            var qOrders = MakeQuickButton("Open Sales", Color.FromArgb(230, 126, 34));

            qProducts.Click += (s, e) => btnProducts.PerformClick();
            qCustomers.Click += (s, e) => btnCustomers.PerformClick();
            qOrders.Click += (s, e) => btnOrders.PerformClick();

            quickRow.Controls.AddRange(new Control[] { qProducts, qCustomers, qOrders });

            // Row 3 - chart placeholder
            var chartWrap = new GroupBox
            {
                Text = "Item / Service (This Month)",
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                BackColor = Color.White
            };
            content.Controls.Add(chartWrap);

            var chartPlaceHolder = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke
            };
            chartWrap.Controls.Add(chartPlaceHolder);

            var lblChart = new Label
            {
                Text = "Chart placeholder (you can add chart later)",
                Dock = DockStyle.Fill,
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };
            chartPlaceHolder.Controls.Add(lblChart);
        }

        // ====== Refresh dashboard numbers ======
        private void RefreshDashboard()
        {
            try
            {
                int pCount = productService.GetAll().Rows.Count;
                int cCount = customerService.GetAll().Rows.Count;

                lblProducts.Text = pCount.ToString();
                lblCustomers.Text = cCount.ToString();
                lblRevenue.Text = "0 đ";
            }
            catch
            {
                lblProducts.Text = "0";
                lblCustomers.Text = "0";
                lblRevenue.Text = "0 đ";
            }
        }

        // ===== UI helpers =====

        private Button MakeSidebarButton(string text)
        {
            var b = new Button
            {
                Text = "   " + text,
                Dock = DockStyle.Top,
                Height = 48,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(44, 62, 80),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 11),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 73, 94);
            return b;
        }

        private Panel MakeCard(string title, string value, Color color, out Label lblValue)
        {
            var panel = new Panel
            {
                Width = 260,
                Height = 115,
                BackColor = color,
                Margin = new Padding(0, 0, 15, 0)
            };

            var lblT = new Label
            {
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 12),
                Location = new Point(12, 10),
                AutoSize = true
            };

            lblValue = new Label
            {
                Text = value,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                Location = new Point(12, 45),
                AutoSize = true
            };

            panel.Controls.Add(lblT);
            panel.Controls.Add(lblValue);

            return panel;
        }

        private Button MakeQuickButton(string text, Color color)
        {
            var b = new Button
            {
                Text = text,
                Width = 180,
                Height = 55,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 11),
                Margin = new Padding(10),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }
}
