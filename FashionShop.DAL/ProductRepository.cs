using FashionShop.DTO;
using MySql.Data.MySqlClient;
using System.Data;

namespace FashionShop.DAL
{
    public class ProductRepository
    {
        public DataTable GetAll()
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT p.product_id, p.product_code, p.product_name,
                                      c.category_name, p.size, p.color, p.gender, p.price, p.stock
                               FROM products p
                               JOIN categories c ON p.category_id=c.category_id
                               ORDER BY p.product_id ASC";
                var da = new MySqlDataAdapter(sql, conn);
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
                string sql = @"SELECT p.product_id, p.product_code, p.product_name,
                                      c.category_name, p.size, p.color, p.gender, p.price, p.stock
                               FROM products p
                               JOIN categories c ON p.category_id=c.category_id
                               WHERE p.product_name LIKE @kw OR p.product_code LIKE @kw
                               ORDER BY p.product_id ASC";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@kw", "%" + kw + "%");
                var da = new MySqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public bool ExistsCode(string code)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT COUNT(*) FROM products WHERE product_code=@c", conn);
                cmd.Parameters.AddWithValue("@c", code);
                return (long)cmd.ExecuteScalar() > 0;
            }
        }

        public int Insert(Product p)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO products(product_code,product_name,category_id,price,stock,size,color,gender)
                               VALUES(@code,@name,@cat,@price,@stock,@size,@color,@gender)";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@code", p.Code);
                cmd.Parameters.AddWithValue("@name", p.Name);
                cmd.Parameters.AddWithValue("@cat", p.CategoryId);
                cmd.Parameters.AddWithValue("@price", p.Price);
                cmd.Parameters.AddWithValue("@stock", p.Stock);
                cmd.Parameters.AddWithValue("@size", p.Size);
                cmd.Parameters.AddWithValue("@color", p.Color);
                cmd.Parameters.AddWithValue("@gender", p.Gender);
                return cmd.ExecuteNonQuery();
            }
        }

        public int Update(Product p)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE products
                               SET product_name=@name, category_id=@cat, price=@price,
                                   stock=@stock, size=@size, color=@color, gender=@gender
                               WHERE product_code=@code";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@code", p.Code);
                cmd.Parameters.AddWithValue("@name", p.Name);
                cmd.Parameters.AddWithValue("@cat", p.CategoryId);
                cmd.Parameters.AddWithValue("@price", p.Price);
                cmd.Parameters.AddWithValue("@stock", p.Stock);
                cmd.Parameters.AddWithValue("@size", p.Size);
                cmd.Parameters.AddWithValue("@color", p.Color);
                cmd.Parameters.AddWithValue("@gender", p.Gender);
                return cmd.ExecuteNonQuery();
            }
        }

        public int Delete(string code)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("DELETE FROM products WHERE product_code=@c", conn);
                cmd.Parameters.AddWithValue("@c", code);
                return cmd.ExecuteNonQuery();
            }
        }

        public DataTable GetCategories()
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var da = new MySqlDataAdapter("SELECT category_id, category_name FROM categories", conn);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public DataTable GetProductsForSale()
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var da = new MySqlDataAdapter(
                    @"SELECT product_id, product_name, price, stock
              FROM products
              WHERE stock > 0
              ORDER BY product_id ASC", conn);  // ✅ bỏ p.
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

    }
}
