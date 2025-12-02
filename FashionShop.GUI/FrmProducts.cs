using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using FashionShop.BLL;
using FashionShop.DTO;

namespace FashionShop.GUI
{
    public class FrmProducts : Form
    {
        ProductService service = new ProductService();
        private Account current;

        DataGridView dgv;
        TextBox txtCode, txtName, txtPrice, txtStock, txtSize, txtColor, txtSearch;
        ComboBox cboCategory, cboGender;

        Button btnAdd, btnUpd, btnDel, btnReload;

        // ===== ctor nhận Account =====
        public FrmProducts(Account acc)
        {
            current = acc;

            Text = "Products Management";
            Size = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterScreen;

            // ====== Left panel (input) ======
            var panel = new Panel { Dock = DockStyle.Left, Width = 320, Padding = new Padding(10) };

            int y = 20;
            panel.Controls.Add(MakeLabel("Code", 10, y));
            txtCode = MakeTextBox(110, y); panel.Controls.Add(txtCode); y += 40;

            panel.Controls.Add(MakeLabel("Name", 10, y));
            txtName = MakeTextBox(110, y); panel.Controls.Add(txtName); y += 40;

            panel.Controls.Add(MakeLabel("Category", 10, y));
            cboCategory = new ComboBox { Location = new Point(110, y), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            panel.Controls.Add(cboCategory); y += 40;

            panel.Controls.Add(MakeLabel("Gender", 10, y));
            cboGender = new ComboBox { Location = new Point(110, y), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cboGender.Items.AddRange(new object[] { "Men", "Women", "Unisex" });
            cboGender.SelectedIndex = 0;
            panel.Controls.Add(cboGender); y += 40;

            panel.Controls.Add(MakeLabel("Size", 10, y));
            txtSize = MakeTextBox(110, y); panel.Controls.Add(txtSize); y += 40;

            panel.Controls.Add(MakeLabel("Color", 10, y));
            txtColor = MakeTextBox(110, y); panel.Controls.Add(txtColor); y += 40;

            panel.Controls.Add(MakeLabel("Price", 10, y));
            txtPrice = MakeTextBox(110, y); panel.Controls.Add(txtPrice); y += 40;

            panel.Controls.Add(MakeLabel("Stock", 10, y));
            txtStock = MakeTextBox(110, y); panel.Controls.Add(txtStock); y += 50;

            // ====== buttons (DÙNG FIELD) ======
            btnAdd = MakeButton("Add", 10, y);
            btnUpd = MakeButton("Update", 110, y);
            btnDel = MakeButton("Delete", 210, y);
            y += 45;
            btnReload = MakeButton("Reload", 10, y);

            btnAdd.Click += BtnAdd_Click;
            btnUpd.Click += BtnUpd_Click;
            btnDel.Click += BtnDel_Click;
            btnReload.Click += (s, e) => LoadGrid();

            panel.Controls.AddRange(new Control[] { btnAdd, btnUpd, btnDel, btnReload });

            // ====== Right (grid + search) ======
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgv.CellClick += Dgv_CellClick;

            var topSearch = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8) };
            txtSearch = new TextBox { Width = 260, Location = new Point(8, 8) };
            var btnSearch = new Button { Text = "Search", Location = new Point(280, 6), Width = 80 };
            btnSearch.Click += (s, e) => dgv.DataSource = service.Search(txtSearch.Text.Trim());
            topSearch.Controls.AddRange(new Control[] { txtSearch, btnSearch });

            Controls.Add(dgv);
            Controls.Add(topSearch);
            Controls.Add(panel);

            Load += (s, e) =>
            {
                cboCategory.DataSource = service.GetCategories();
                cboCategory.DisplayMember = "category_name";
                cboCategory.ValueMember = "category_id";
                LoadGrid();

                ApplyRolePermission(); // <--- gọi sau khi UI tạo xong
            };
        }

        // ===== ctor mặc định (nếu mở form không truyền account) =====
        public FrmProducts() : this(null) { }

        // ====== khóa quyền staff ======
        private void ApplyRolePermission()
        {
            if (current == null) return;

            bool isStaff = current.Role != null &&
                           current.Role.Equals("Staff", StringComparison.OrdinalIgnoreCase);

            if (isStaff)
            {
                // không cho thêm/sửa/xóa
                btnAdd.Enabled = false;
                btnUpd.Enabled = false;
                btnDel.Enabled = false;

                // khóa luôn input để staff chỉ xem
                txtCode.ReadOnly = true;
                txtName.ReadOnly = true;
                txtSize.ReadOnly = true;
                txtColor.ReadOnly = true;
                txtPrice.ReadOnly = true;
                txtStock.ReadOnly = true;
                cboCategory.Enabled = false;
                cboGender.Enabled = false;

                Text += " (View only)";
            }
        }

        void LoadGrid() => dgv.DataSource = service.GetAll();

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
                Price = decimal.Parse(txtPrice.Text.Trim()),
                Stock = int.Parse(txtStock.Text.Trim())
            };
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var p = ReadForm();
                if (service.Add(p, out string err)) { MessageBox.Show("Added!"); LoadGrid(); }
                else MessageBox.Show(err);
            }
            catch (Exception ex) { MessageBox.Show("Invalid data: " + ex.Message); }
        }

        private void BtnUpd_Click(object sender, EventArgs e)
        {
            try
            {
                var p = ReadForm();
                if (service.Update(p, out string err)) { MessageBox.Show("Updated!"); LoadGrid(); }
                else MessageBox.Show(err);
            }
            catch (Exception ex) { MessageBox.Show("Invalid data: " + ex.Message); }
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text)) return;
            if (MessageBox.Show("Delete this product?", "Confirm", MessageBoxButtons.YesNo)
                == DialogResult.Yes)
            {
                service.Delete(txtCode.Text.Trim());
                LoadGrid();
            }
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var r = dgv.Rows[e.RowIndex];
            txtCode.Text = r.Cells["product_code"].Value.ToString();
            txtName.Text = r.Cells["product_name"].Value.ToString();
            txtSize.Text = r.Cells["size"].Value?.ToString();
            txtColor.Text = r.Cells["color"].Value?.ToString();
            cboGender.Text = r.Cells["gender"].Value?.ToString();
            txtPrice.Text = r.Cells["price"].Value.ToString();
            txtStock.Text = r.Cells["stock"].Value.ToString();
        }

        // UI helpers
        Label MakeLabel(string t, int x, int y) =>
            new Label { Text = t + ":", Location = new Point(x, y + 5), AutoSize = true };

        TextBox MakeTextBox(int x, int y) =>
            new TextBox { Location = new Point(x, y), Width = 180 };

        Button MakeButton(string t, int x, int y) =>
            new Button { Text = t, Location = new Point(x, y), Width = 90 };
    }
}
