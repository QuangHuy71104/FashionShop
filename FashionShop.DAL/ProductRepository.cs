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
        // (KHÔNG load ảnh blob để grid nhẹ)
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
        p.price, p.stock
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
        // (KHÔNG load ảnh blob để grid nhẹ)
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
        p.price, p.stock
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
        // (KHÔNG load ảnh blob để grid nhẹ)
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
        p.price, p.stock
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
        // INSERT (color_id + image_blob)
        // =========================
        public int Insert(Product p)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"
INSERT INTO products
(product_code, product_name, category_id, supplier_id, size, color_id, gender, price, stock, image_blob, image_mime, status)
VALUES
(@code, @name, @catId, @supId, @size, @colorId, @gender, @price, @stock, @imgBlob, @imgMime, 1);";

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

                cmd.Parameters.AddWithValue("@imgBlob", (object)p.ImageBlob ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@imgMime", string.IsNullOrWhiteSpace(p.ImageMime) ? (object)DBNull.Value : p.ImageMime);

                return cmd.ExecuteNonQuery();
            }
        }

        // =========================
        // UPDATE (color_id + image_blob)
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
    image_blob = @imgBlob,
    image_mime = @imgMime
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

                cmd.Parameters.AddWithValue("@imgBlob", (object)p.ImageBlob ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@imgMime", string.IsNullOrWhiteSpace(p.ImageMime) ? (object)DBNull.Value : p.ImageMime);

                return cmd.ExecuteNonQuery();
            }
        }

        // =========================
        // GET IMAGE BY CODE (load blob khi click 1 dòng)
        // =========================
        public Tuple<byte[], string> GetImageByCode(string code)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"
SELECT image_blob, image_mime
FROM products
WHERE product_code = @code
LIMIT 1;";

                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@code", code);

                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read())
                        return Tuple.Create<byte[], string>(null, null);

                    if (rd.IsDBNull(0))
                        return Tuple.Create<byte[], string>(null, rd.IsDBNull(1) ? null : rd.GetString(1));

                    var blob = (byte[])rd["image_blob"];
                    var mime = rd.IsDBNull(1) ? null : rd.GetString(1);
                    return Tuple.Create(blob, mime);
                }
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
