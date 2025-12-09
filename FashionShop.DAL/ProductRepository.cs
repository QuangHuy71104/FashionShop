using System;
using System.Data;
using MySql.Data.MySqlClient;
using FashionShop.DTO;

namespace FashionShop.DAL
{
    public class ProductRepository
    {
        // =========================
        // GET ALL (join categories + colors)
        // =========================
        public DataTable GetAll()
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"
SELECT  p.product_id, p.product_code, p.product_name,
        c.category_name,
        p.size,
        co.color_name,
        p.gender,
        p.price, p.stock, p.image_path
FROM products p
JOIN categories c ON p.category_id = c.category_id
LEFT JOIN colors co ON p.color_id = co.color_id
ORDER BY p.product_id ASC;";

                var da = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================
        // SEARCH (lọc theo code/name/category/color)
        // =========================
        public DataTable Search(string keyword)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"
SELECT  p.product_id, p.product_code, p.product_name,
        c.category_name,
        p.size,
        co.color_name,
        p.gender,
        p.price, p.stock, p.image_path
FROM products p
JOIN categories c ON p.category_id = c.category_id
LEFT JOIN colors co ON p.color_id = co.color_id
WHERE p.product_code LIKE @kw
   OR p.product_name LIKE @kw
   OR c.category_name LIKE @kw
   OR co.color_name LIKE @kw
ORDER BY p.product_id ASC;";

                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");

                var da = new MySqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================
        // GET FOR SALE (stock > 0)
        // =========================
        public DataTable GetProductsForSale()
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"
SELECT  p.product_id, p.product_code, p.product_name,
        c.category_name,
        p.size,
        co.color_name,
        p.gender,
        p.price, p.stock, p.image_path
FROM products p
JOIN categories c ON p.category_id = c.category_id
LEFT JOIN colors co ON p.color_id = co.color_id
WHERE p.stock > 0 AND p.status = 1
ORDER BY p.product_id ASC;";

                var da = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================
        // EXISTS CODE
        // =========================
        public bool ExistsCode(string code)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM products WHERE product_code = @code;";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@code", code);

                var count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        // =========================
        // INSERT (color_id)
        // =========================
        public int Insert(Product p)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"
INSERT INTO products
(product_code, product_name, category_id, supplier_id, size, color_id, gender, price, stock, image_path, status)
VALUES
(@code, @name, @catId, @supId, @size, @colorId, @gender, @price, @stock, @img, 1);";

                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@code", p.Code);
                cmd.Parameters.AddWithValue("@name", p.Name);
                cmd.Parameters.AddWithValue("@catId", p.CategoryId);
                cmd.Parameters.AddWithValue("@supId", p.SupplierId == 0 ? (object)DBNull.Value : p.SupplierId);
                cmd.Parameters.AddWithValue("@size", string.IsNullOrWhiteSpace(p.Size) ? (object)DBNull.Value : p.Size);
                cmd.Parameters.AddWithValue("@colorId", p.ColorId <= 0 ? (object)DBNull.Value : p.ColorId);
                cmd.Parameters.AddWithValue("@gender", p.Gender);
                cmd.Parameters.AddWithValue("@price", p.Price);
                cmd.Parameters.AddWithValue("@stock", p.Stock);
                cmd.Parameters.AddWithValue("@img", string.IsNullOrWhiteSpace(p.ImagePath) ? (object)DBNull.Value : p.ImagePath);

                return cmd.ExecuteNonQuery();
            }
        }

        // =========================
        // UPDATE (color_id)
        // =========================
        public int Update(Product p)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"
UPDATE products
SET product_name = @name,
    category_id = @catId,
    supplier_id = @supId,
    size = @size,
    color_id = @colorId,
    gender = @gender,
    price = @price,
    stock = @stock,
    image_path = @img
WHERE product_code = @code;";

                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@code", p.Code);
                cmd.Parameters.AddWithValue("@name", p.Name);
                cmd.Parameters.AddWithValue("@catId", p.CategoryId);
                cmd.Parameters.AddWithValue("@supId", p.SupplierId == 0 ? (object)DBNull.Value : p.SupplierId);
                cmd.Parameters.AddWithValue("@size", string.IsNullOrWhiteSpace(p.Size) ? (object)DBNull.Value : p.Size);
                cmd.Parameters.AddWithValue("@colorId", p.ColorId <= 0 ? (object)DBNull.Value : p.ColorId);
                cmd.Parameters.AddWithValue("@gender", p.Gender);
                cmd.Parameters.AddWithValue("@price", p.Price);
                cmd.Parameters.AddWithValue("@stock", p.Stock);
                cmd.Parameters.AddWithValue("@img", string.IsNullOrWhiteSpace(p.ImagePath) ? (object)DBNull.Value : p.ImagePath);

                return cmd.ExecuteNonQuery();
            }
        }

        // =========================
        // DELETE
        // =========================
        public int Delete(string code)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM products WHERE product_code = @code;";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@code", code);
                return cmd.ExecuteNonQuery();
            }
        }

        // =========================
        // GET CATEGORIES FOR COMBO
        // =========================
        public DataTable GetCategories()
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = "SELECT category_id, category_name FROM categories ORDER BY category_name;";
                var da = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================
        // GET COLORS FOR COMBO
        // =========================
        public DataTable GetColors()
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = "SELECT color_id, color_name FROM colors ORDER BY color_name;";
                var da = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}
