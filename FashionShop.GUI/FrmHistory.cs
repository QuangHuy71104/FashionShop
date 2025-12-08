using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using FashionShop.BLL;

namespace FashionShop.GUI
{
    public partial class FrmHistory : Form
    {
        private readonly OrderService orderService = new OrderService();
        private readonly string hint = "Search by customer / product...";

        public FrmHistory()
        {
            InitializeComponent();

            // Placeholder thủ công
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

            // events
            btnSearch.Click += (s, e) => LoadHistory();
            btnClear.Click += (s, e) => ClearFilters();

            // Enter để search
            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    LoadHistory();
                }
            };

            Load += FrmHistory_Load;
        }

        private void FrmHistory_Load(object sender, EventArgs e)
        {
            // mặc định xem 1 tháng gần nhất
            dtFrom.Value = DateTime.Today.AddMonths(-1);
            dtTo.Value = DateTime.Today;

            StyleGrid(dgvHistory);
            LoadHistory();
        }

        private void LoadHistory()
        {
            string key = txtSearch.Text.Trim();
            if (key == hint) key = "";

            DateTime? from = dtFrom.Checked ? dtFrom.Value.Date : (DateTime?)null;
            DateTime? to = dtTo.Checked ? dtTo.Value.Date : (DateTime?)null;

            // Vì gộp 1 ô => truyền chung keyword cho cả customer & product
            DataTable dt = orderService.GetHistory(key, key, from, to);

            dgvHistory.DataSource = dt;

            dgvHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHistory.BackgroundColor = Color.White;
            dgvHistory.DefaultCellStyle.BackColor = Color.White;
        }

        private void ClearFilters()
        {
            txtSearch.Text = hint;
            txtSearch.ForeColor = Color.Gray;

            dtFrom.Value = DateTime.Today.AddMonths(-1);
            dtTo.Value = DateTime.Today;

            LoadHistory();
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
        }
    }
}
