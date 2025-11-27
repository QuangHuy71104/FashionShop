using FashionShop.DTO;
using MySql.Data.MySqlClient;
using System.Data;

namespace FashionShop.DAL
{
    public class CustomerRepository
    {
        public DataTable GetAll()
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var da = new MySqlDataAdapter(
                    "SELECT customer_id, customer_name, phone, email, address, points FROM customers", conn);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public DataTable Search(string kw)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT customer_id, customer_name, phone, email, address, points
                               FROM customers
                               WHERE customer_name LIKE @kw OR phone LIKE @kw";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@kw", "%" + kw + "%");
                var da = new MySqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public bool ExistsPhone(string phone)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT COUNT(*) FROM customers WHERE phone=@p", conn);
                cmd.Parameters.AddWithValue("@p", phone);
                return (long)cmd.ExecuteScalar() > 0;
            }
        }

        public int Insert(Customer c)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"INSERT INTO customers(customer_name,phone,email,address,points)
                      VALUES(@n,@p,@e,@a,@pt)", conn);
                cmd.Parameters.AddWithValue("@n", c.Name);
                cmd.Parameters.AddWithValue("@p", c.Phone);
                cmd.Parameters.AddWithValue("@e", c.Email);
                cmd.Parameters.AddWithValue("@a", c.Address);
                cmd.Parameters.AddWithValue("@pt", c.Points);
                return cmd.ExecuteNonQuery();
            }
        }

        public int Update(Customer c)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"UPDATE customers SET customer_name=@n, phone=@p, email=@e, address=@a, points=@pt
                      WHERE customer_id=@id", conn);
                cmd.Parameters.AddWithValue("@id", c.Id);
                cmd.Parameters.AddWithValue("@n", c.Name);
                cmd.Parameters.AddWithValue("@p", c.Phone);
                cmd.Parameters.AddWithValue("@e", c.Email);
                cmd.Parameters.AddWithValue("@a", c.Address);
                cmd.Parameters.AddWithValue("@pt", c.Points);
                return cmd.ExecuteNonQuery();
            }
        }

        public int Delete(int id)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("DELETE FROM customers WHERE customer_id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery();
            }
        }

        public DataTable GetForCombo()
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var da = new MySqlDataAdapter("SELECT customer_id, customer_name FROM customers", conn);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}
