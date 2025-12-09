using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

using FashionShop.DTO;
using FashionShop.BLL;

namespace FashionShop.GUI
{
    public class FrmMain : Form
    {
        private Account current;
        private ProductService productService = new ProductService();
        private CustomerService customerService = new CustomerService();
        private OrderService orderService = new OrderService();

        private SplitContainer splitOuter;

        // ✅ 1 biến ratio duy nhất để scale theo cửa sổ
        private double _sidebarRatio = 0.20; // 20% (muốn nhỏ hơn thì giảm, vd 0.18)

        Label lblProducts, lblCustomers, lblRevenue;

        Panel sidebar, topbar, content;
        Button btnHome, btnProducts, btnCustomers, btnOrders, btnHistory, btnLogout; // ✅ thêm btnHistory

        // ===== Charts =====
        private Panel chartWrap;
        private Panel pnlMode1, pnlMode2;     // ✅ 2 mode panel
        private Chart chartRevenue, chartTopProducts, chartCategory;
        private TableLayoutPanel mode2Layout;
        private Button btnToggleCharts;
        private bool showMode1 = false;        // false = Mode 2 mặc định trước

        public FrmMain(Account acc)
        {
            current = acc;
            Text = "Fashion Shop";
            WindowState = FormWindowState.Maximized;
            Font = new Font("Segoe UI", 10);
            BackColor = Color.FromArgb(245, 246, 250);

            BuildLayout();

            // ✅ init splitter sau khi handle + layout xong
            Shown += (s, e) =>
            {
                splitOuter.Panel1MinSize = 140;  // ✅ cho sidebar nhỏ được (tự chỉnh)
                splitOuter.Panel2MinSize = 500;

                SetSplitterRatio(_sidebarRatio);
            };

            // ✅ scale theo cửa sổ: resize giữ đúng ratio
            splitOuter.SizeChanged += (s, e) =>
            {
                if (splitOuter.Width > splitOuter.Panel1MinSize + splitOuter.Panel2MinSize)
                    SetSplitterRatio(_sidebarRatio);
            };

            Load += (s, e) => RefreshDashboard();
            Activated += (s, e) => RefreshDashboard();
        }

        private void SetSplitterRatio(double ratio)
        {
            int desired = (int)(splitOuter.Width * ratio);
            int min = splitOuter.Panel1MinSize;
            int max = splitOuter.Width - splitOuter.Panel2MinSize;
            if (max < min) max = min;

            splitOuter.SplitterDistance = Math.Max(min, Math.Min(desired, max));
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

        private void BuildLayout()
        {
            // ===== OUTER SPLIT =====
            splitOuter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                IsSplitterFixed = true,       // không cho kéo
                FixedPanel = FixedPanel.None,
                BackColor = Color.FromArgb(245, 246, 250)
            };
            Controls.Add(splitOuter);

            // ===== SIDEBAR (Panel1) =====
            sidebar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(44, 62, 80)
            };
            splitOuter.Panel1.Controls.Add(sidebar);

            // ===== TOPBAR (Panel2 - Top) =====
            topbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };
            splitOuter.Panel2.Controls.Add(topbar);

            var lblTitle = new Label
            {
                Text = "Fashion Shop",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Dock = DockStyle.Left,
                AutoSize = true
            };
            topbar.Controls.Add(lblTitle);

            // ===== CONTENT (Panel2 - Fill) =====
            content = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                BackColor = Color.FromArgb(245, 246, 250)
            };
            splitOuter.Panel2.Controls.Add(content);
            content.BringToFront();

            // ✅ MAIN LAYOUT 2 ROWS: CARDS + CHARTS
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = content.BackColor
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));  // cards
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // charts
            content.Controls.Add(mainLayout);

            // ===== USER INFO TOP =====
            var userPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                Padding = new Padding(12, 14, 12, 10),
                BackColor = Color.FromArgb(52, 73, 94)
            };
            sidebar.Controls.Add(userPanel);

            var lblUser = new Label
            {
                Text = current.EmployeeName,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 14),
                Dock = DockStyle.Top,
                Height = 28
            };
            var lblRole = new Label
            {
                Text = current.Role,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Top,
                Height = 20
            };

            var sep = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = Color.White
            };

            userPanel.Controls.Add(sep);
            userPanel.Controls.Add(lblRole);
            userPanel.Controls.Add(lblUser);

            // ===== MENU WRAP =====
            var menuWrap = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(44, 62, 80)
            };
            sidebar.Controls.Add(menuWrap);
            menuWrap.BringToFront();

            // menu buttons
            btnHome = MakeSidebarButton("Home");
            btnProducts = MakeSidebarButton("Products");
            btnCustomers = MakeSidebarButton("Customers");
            btnOrders = MakeSidebarButton("Sales / Orders");
            btnHistory = MakeSidebarButton("History"); // ✅ thêm nút History

            btnHome.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnProducts.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnCustomers.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnOrders.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnHistory.Font = new Font("Segoe UI", 12, FontStyle.Bold);

            // add theo DockTop (để Home nằm trên)
            // DockTop add theo thứ tự ngược: add trước => nằm dưới
            menuWrap.Controls.Add(btnHistory);  // ✅ History nằm dưới Orders
            menuWrap.Controls.Add(btnOrders);
            menuWrap.Controls.Add(btnCustomers);
            menuWrap.Controls.Add(btnProducts);
            menuWrap.Controls.Add(btnHome);

            btnHome.Visible = false; // tắt nút home


            // ===== LOGOUT bottom =====
            btnLogout = MakeSidebarButton("Logout");
            btnLogout.Dock = DockStyle.Bottom;
            btnLogout.Height = 55;
            btnLogout.Text = "  Logout";
            btnLogout.Font = new Font("Segoe UI", 12, FontStyle.Bold);

            btnLogout.Image = ResizeImage(Properties.Resources.logout, 18, 18);
            btnLogout.ImageAlign = ContentAlignment.MiddleLeft;
            btnLogout.TextAlign = ContentAlignment.MiddleLeft;
            btnLogout.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnLogout.Padding = new Padding(8, 0, 0, 0);

            sidebar.Controls.Add(btnLogout);

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
            btnHistory.Click += (s, e) =>
            {
                new FrmHistory().ShowDialog(); // ✅ mở form lịch sử
            };
            btnLogout.Click += (s, e) => Close();

            // ===== Row 1 - cards =====
            var cardsWrap = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = content.BackColor
            };
            mainLayout.Controls.Add(cardsWrap, 0, 0);

            var cardsRow = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            cardsWrap.Controls.Add(cardsRow);

            var cardItems = MakeCard("Items", "0", Color.FromArgb(76, 133, 220), out lblProducts);
            var cardCustomers = MakeCard("Customers", "0", Color.FromArgb(244, 186, 63), out lblCustomers);
            var cardRevenue = MakeCard("Revenue", "0 đ", Color.FromArgb(95, 209, 120), out lblRevenue);
            cardsRow.Controls.AddRange(new Control[] { cardItems, cardCustomers, cardRevenue });

            void CenterCards()
            {
                cardsRow.Left = (cardsWrap.Width - cardsRow.Width) / 2;
                cardsRow.Top = (cardsWrap.Height - cardsRow.Height) / 2;
            }
            cardsWrap.Resize += (s, e) => CenterCards();
            CenterCards();

            // ===== Row 2 - charts area =====
            chartWrap = new Panel
            {
            };
                // ===== Row 3 - charts area =====
                chartWrap = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 500,
                    Padding = new Padding(12),
                    BackColor = Color.White
                };
                content.Controls.Add(chartWrap);

                // một panel filler phía dưới để chiếm chỗ còn lại cho đẹp
                var filler = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(12),
                    BackColor = Color.White,
                    Margin = new Padding(0, 10, 0, 0) // hở nhẹ với cards
                };
                mainLayout.Controls.Add(chartWrap, 0, 1);

                BuildChartsArea();
            }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FrmMain
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "FrmMain";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.ResumeLayout(false);
        }

        private void FrmMain_Load(object sender, EventArgs e) { }

        // ================= CHARTS UI =================

        private void BuildChartsArea()
        {
            // ===== Toggle Button =====
            btnToggleCharts = new Button
            {
                Text = "Show mode 1 (revenue chart)",
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnToggleCharts.FlatAppearance.BorderSize = 0;
            btnToggleCharts.Click += (s, e) => ToggleChartsMode();
            chartWrap.Controls.Add(btnToggleCharts);

            // ===== Panel MODE 1 =====
            pnlMode1 = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = false // mặc định ẩn
            };
            chartWrap.Controls.Add(pnlMode1);

            chartRevenue = MakeChart("Revenue by day (this month)", SeriesChartType.Line);
            pnlMode1.Controls.Add(chartRevenue);

            // ===== Panel MODE 2 =====
            pnlMode2 = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = true // mặc định hiện mode 2 trước
            };
            chartWrap.Controls.Add(pnlMode2);
            pnlMode2.BringToFront();

            mode2Layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White
            };
            mode2Layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            mode2Layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            pnlMode2.Controls.Add(mode2Layout);

            chartTopProducts = MakeChart("Top products (this month)", SeriesChartType.Bar);
            mode2Layout.Controls.Add(chartTopProducts, 0, 0);

            chartCategory = MakeChart("Revenue by category (this month)", SeriesChartType.Doughnut);
            mode2Layout.Controls.Add(chartCategory, 1, 0);

            btnToggleCharts.BringToFront();
        }

        private void ToggleChartsMode()
        {
            showMode1 = !showMode1;

            if (showMode1)
            {
                pnlMode1.Visible = true;
                pnlMode2.Visible = false;
                pnlMode1.BringToFront();
                btnToggleCharts.Text = "Show mode 2 (bottom charts)";
            }
            else
            {
                pnlMode1.Visible = false;
                pnlMode2.Visible = true;
                pnlMode2.BringToFront();
                btnToggleCharts.Text = "Show mode 1 (revenue chart)";
            }
        }

        private Chart MakeChart(string title, SeriesChartType type)
        {
            var ch = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderlineColor = Color.Gainsboro,
                BorderlineDashStyle = ChartDashStyle.Solid,
                BorderlineWidth = 1
            };

            var area = new ChartArea("area");
            area.BackColor = Color.White;

            area.AxisX.MajorGrid.LineColor = Color.Gainsboro;
            area.AxisY.MajorGrid.LineColor = Color.Gainsboro;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 9f);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 9f);

            ch.ChartAreas.Add(area);

            ch.Titles.Add(new Title
            {
                Text = title,
                Font = new Font("Segoe UI Semibold", 12f),
                ForeColor = Color.Black,
                Alignment = ContentAlignment.TopLeft
            });

            var s = new Series("series")
            {
                ChartType = type,
                BorderWidth = 3,
                IsValueShownAsLabel = true,
                Font = new Font("Segoe UI", 9f)
            };

            if (type == SeriesChartType.Line)
            {
                s.MarkerStyle = MarkerStyle.Circle;
                s.MarkerSize = 6;
            }

            ch.Series.Add(s);

            var legend = new Legend
            {
                Docking = Docking.Bottom,
                Font = new Font("Segoe UI", 9f)
            };
            ch.Legends.Add(legend);

            return ch;
        }

        // ================= CHARTS DATA =================

        private void LoadCharts()
        {
            try
            {
                var dtRevenue = orderService.GetRevenueByDayInMonth();
                LoadRevenueChart(dtRevenue);

                var dtTop = orderService.GetTopProductsInMonth(5);
                LoadTopProductsChart(dtTop);

                var dtCate = orderService.GetRevenueByCategoryInMonth();
                LoadCategoryChart(dtCate);
            }
            catch
            {
                chartRevenue.Series[0].Points.Clear();
                chartTopProducts.Series[0].Points.Clear();
                chartCategory.Series[0].Points.Clear();
            }
        }

        private void LoadRevenueChart(DataTable dt)
        {
            var s = chartRevenue.Series[0];
            s.Points.Clear();
            s.LegendText = "Revenue";

            foreach (DataRow r in dt.Rows)
            {
                int day = Convert.ToInt32(r["day"]);
                decimal rev = Convert.ToDecimal(r["revenue"]);
                s.Points.AddXY(day, rev);
            }
        }

        private void LoadTopProductsChart(DataTable dt)
        {
            var s = chartTopProducts.Series[0];
            s.Points.Clear();
            s.LegendText = "Qty sold";

            foreach (DataRow r in dt.Rows)
            {
                string name = r["product_name"].ToString();
                int qty = Convert.ToInt32(r["qty"]);
                s.Points.AddXY(name, qty);
            }
        }

        private void LoadCategoryChart(DataTable dt)
        {
            var s = chartCategory.Series[0];
            s.Points.Clear();

            foreach (DataRow r in dt.Rows)
            {
                string cate = r["category_name"].ToString();
                decimal rev = Convert.ToDecimal(r["revenue"]);

                int idx = s.Points.AddXY(cate, rev);
                var p = s.Points[idx];

                p.LegendText = cate;
                p.Label = rev.ToString("N0");
                // p.Label = "#PERCENT{P0}"; // nếu muốn hiện %
            }

            s.LegendText = "";
        }

        // ====== Refresh dashboard numbers ======
        private void RefreshDashboard()
        {
            try
            {
                int pCount = productService.GetAll().Rows.Count;
                int cCount = customerService.GetAll().Rows.Count;

                decimal revenue = orderService.GetRevenue();

                lblProducts.Text = pCount.ToString();
                lblCustomers.Text = cCount.ToString();
                lblRevenue.Text = revenue.ToString("N0") + " đ";

                LoadCharts();
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

        private Panel MakeCard(string title, string value, Color headerColor, out Label lblValue)
        {
            var card = new Panel
            {
                Width = 300,
                Height = 200,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 25, 0),
                Padding = new Padding(0),
            };

            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(230, 230, 230), 1))
                {
                    g.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = headerColor
            };
            card.Controls.Add(header);

            var lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(30, 30, 30),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            header.Controls.Add(lblTitle);

            var body = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            card.Controls.Add(body);

            var bodyTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            bodyTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            bodyTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            body.Controls.Add(bodyTable);

            lblValue = new Label
            {
                Text = value,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(30, 30, 30),
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Margin = new Padding(0, 60, 0, 0)
            };

            bodyTable.Controls.Add(lblValue, 0, 0);

            return card;
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
