using System;
using System.Drawing;
using System.Windows.Forms;
using FashionShop.BLL;
using FashionShop.DTO;

namespace FashionShop.GUI
{
    public class FrmCustomers : Form
    {
        CustomerService service = new CustomerService();
        DataGridView dgv;
        TextBox txtId, txtName, txtPhone, txtEmail, txtAddress, txtPoints, txtSearch;

        public FrmCustomers()
        {
            Text = "Customers Management";
            Size = new Size(950, 550);
            StartPosition = FormStartPosition.CenterScreen;

            var panel = new Panel { Dock = DockStyle.Left, Width = 300, Padding = new Padding(10) };

            int y = 20;
            panel.Controls.Add(L("Id", 10, y)); txtId = TB(110, y); txtId.ReadOnly = true; panel.Controls.Add(txtId); y += 38;
            panel.Controls.Add(L("Name", 10, y)); txtName = TB(110, y); panel.Controls.Add(txtName); y += 38;
            panel.Controls.Add(L("Phone", 10, y)); txtPhone = TB(110, y); panel.Controls.Add(txtPhone); y += 38;
            panel.Controls.Add(L("Email", 10, y)); txtEmail = TB(110, y); panel.Controls.Add(txtEmail); y += 38;
            panel.Controls.Add(L("Address", 10, y)); txtAddress = TB(110, y); panel.Controls.Add(txtAddress); y += 38;
            panel.Controls.Add(L("Points", 10, y)); txtPoints = TB(110, y); panel.Controls.Add(txtPoints); y += 48;

            var btnAdd = Btn("Add", 10, y); var btnUpd = Btn("Update", 105, y); var btnDel = Btn("Delete", 200, y);
            y += 45; var btnReload = Btn("Reload", 10, y);

            btnAdd.Click += (s, e) => {
                var c = ReadForm();
                if (service.Add(c, out string err)) { MessageBox.Show("Added!"); LoadGrid(); }
                else MessageBox.Show(err);
            };
            btnUpd.Click += (s, e) => {
                var c = ReadForm();
                if (service.Update(c, out string err)) { MessageBox.Show("Updated!"); LoadGrid(); }
                else MessageBox.Show(err);
            };
            btnDel.Click += (s, e) => {
                if (!int.TryParse(txtId.Text, out int id)) return;
                if (MessageBox.Show("Delete?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    service.Delete(id); LoadGrid();
                }
            };
            btnReload.Click += (s, e) => LoadGrid();

            panel.Controls.AddRange(new Control[] { btnAdd, btnUpd, btnDel, btnReload });

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgv.CellClick += (s, e) => {
                if (e.RowIndex < 0) return;
                var r = dgv.Rows[e.RowIndex];
                txtId.Text = r.Cells["customer_id"].Value.ToString();
                txtName.Text = r.Cells["customer_name"].Value.ToString();
                txtPhone.Text = r.Cells["phone"].Value?.ToString();
                txtEmail.Text = r.Cells["email"].Value?.ToString();
                txtAddress.Text = r.Cells["address"].Value?.ToString();
                txtPoints.Text = r.Cells["points"].Value.ToString();
            };

            var top = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8) };
            txtSearch = new TextBox { Width = 250, Location = new Point(8, 8) };
            var btnSearch = new Button { Text = "Search", Location = new Point(270, 6), Width = 80 };
            btnSearch.Click += (s, e) => dgv.DataSource = service.Search(txtSearch.Text.Trim());
            top.Controls.AddRange(new Control[] { txtSearch, btnSearch });

            Controls.Add(dgv); Controls.Add(top); Controls.Add(panel);
            Load += (s, e) => LoadGrid();
        }

        void LoadGrid() => dgv.DataSource = service.GetAll();

        Customer ReadForm()
        {
            int.TryParse(txtId.Text, out int id);
            int.TryParse(txtPoints.Text, out int pts);
            return new Customer
            {
                Id = id,
                Name = txtName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                Points = pts
            };
        }

        Label L(string t, int x, int y) => new Label { Text = t + ":", Location = new Point(x, y + 5), AutoSize = true };
        TextBox TB(int x, int y) => new TextBox { Location = new Point(x, y), Width = 170 };
        Button Btn(string t, int x, int y) => new Button { Text = t, Location = new Point(x, y), Width = 85 };
    }
}
