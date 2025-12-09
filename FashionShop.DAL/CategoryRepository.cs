using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace FashionShop.DAL
{
    public class CategoryRepository
    {
        public DataTable GetAll()
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = "SELECT category_id, category_name FROM categories ORDER BY category_name";

                using (var da = new MySqlDataAdapter(sql, conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public bool ExistsName(string name)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM categories WHERE LOWER(category_name)=LOWER(@name)";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name.Trim());
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public bool Insert(string name)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = "INSERT INTO categories(category_name) VALUES(@name)";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name.Trim());
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Update(int id, string name)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE categories SET category_name=@name WHERE category_id=@id";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name.Trim());
                    cmd.Parameters.AddWithValue("@id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Delete(int id)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM categories WHERE category_id=@id";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
