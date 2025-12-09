using System.Data;
using FashionShop.DTO;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FashionShop.DAL
{
    public class OrderRepository
    {
        public int InsertOrder(int employeeId, int? customerId, decimal total, List<OrderDetail> details)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        string code = "HD" + DateTime.Now.ToString("yyyyMMddHHmmss");

                        var cmdO = new MySqlCommand(
                            @"INSERT INTO orders(order_code, order_date, customer_id, employee_id, total_amount, payment_method)
                              VALUES(@code, NOW(), @cus, @emp, @total, 'Cash')", conn, tran);
                        cmdO.Parameters.AddWithValue("@code", code);
                        cmdO.Parameters.AddWithValue("@cus", customerId);
                        cmdO.Parameters.AddWithValue("@emp", employeeId);
                        cmdO.Parameters.AddWithValue("@total", total);
                        cmdO.ExecuteNonQuery();

                        int orderId = (int)cmdO.LastInsertedId;

                        foreach (var d in details)
                        {
                            var cmdD = new MySqlCommand(
                                @"INSERT INTO order_details(order_id, product_id, quantity, unit_price)
                                  VALUES(@oid,@pid,@q,@price)", conn, tran);
                            cmdD.Parameters.AddWithValue("@oid", orderId);
                            cmdD.Parameters.AddWithValue("@pid", d.ProductId);
                            cmdD.Parameters.AddWithValue("@q", d.Quantity);
                            cmdD.Parameters.AddWithValue("@price", d.UnitPrice);
                            cmdD.ExecuteNonQuery();

                            var cmdS = new MySqlCommand(
                                "UPDATE products SET stock = stock - @q WHERE product_id=@pid", conn, tran);
                            cmdS.Parameters.AddWithValue("@q", d.Quantity);
                            cmdS.Parameters.AddWithValue("@pid", d.ProductId);
                            cmdS.ExecuteNonQuery();
                        }

                        tran.Commit();
                        return orderId;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        // ✅ THÊM HÀM NÀY
        public decimal GetRevenue()
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT IFNULL(SUM(quantity * unit_price), 0)
                      FROM order_details", conn);

                object result = cmd.ExecuteScalar();
                return Convert.ToDecimal(result);
            }
        }

        // 1) Doanh thu theo ngày trong tháng hiện tại
        public DataTable GetRevenueByDayInMonth()
        {
            var dt = new DataTable();
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"
            SELECT DAY(o.order_date) AS day,
                   IFNULL(SUM(od.quantity * od.unit_price), 0) AS revenue
            FROM orders o
            JOIN order_details od ON o.order_id = od.order_id
            WHERE MONTH(o.order_date) = MONTH(CURDATE())
              AND YEAR(o.order_date) = YEAR(CURDATE())
            GROUP BY DAY(o.order_date)
            ORDER BY day;";

                using (var da = new MySqlDataAdapter(sql, conn))
                {
                    da.Fill(dt);
                }
            }
            return dt;
        }

        // 2) Top sản phẩm bán chạy trong tháng
        public DataTable GetTopProductsInMonth(int top)
        {
            var dt = new DataTable();
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"
            SELECT p.product_name,
                   IFNULL(SUM(od.quantity), 0) AS qty
            FROM order_details od
            JOIN orders o ON o.order_id = od.order_id
            JOIN products p ON p.product_id = od.product_id
            WHERE MONTH(o.order_date) = MONTH(CURDATE())
              AND YEAR(o.order_date) = YEAR(CURDATE())
            GROUP BY p.product_id, p.product_name
            ORDER BY qty DESC
            LIMIT @top;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@top", top);
                    using (var da = new MySqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        // 3) Doanh thu theo category trong tháng
        public DataTable GetRevenueByCategoryInMonth()
        {
            var dt = new DataTable();
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"
            SELECT c.category_name,
                   IFNULL(SUM(od.quantity * od.unit_price), 0) AS revenue
            FROM order_details od
            JOIN orders o ON o.order_id = od.order_id
            JOIN products p ON p.product_id = od.product_id
            JOIN categories c ON c.category_id = p.category_id
            WHERE MONTH(o.order_date) = MONTH(CURDATE())
              AND YEAR(o.order_date) = YEAR(CURDATE())
            GROUP BY c.category_id, c.category_name
            ORDER BY revenue DESC;";

                using (var da = new MySqlDataAdapter(sql, conn))
                {
                    da.Fill(dt);
                }
            }
            return dt;
        }

        // 4) History: lọc theo tên khách, tên sản phẩm, và khoảng thời gian
        public DataTable GetHistory(string kwCustomer, string kwProduct, DateTime? from, DateTime? to)
        {
            var dt = new DataTable();
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();

                string sql = @"
            SELECT 
                o.order_id,
                o.order_code,
                o.order_date,
                IFNULL(c.customer_name, 'Walk-in') AS customer_name,
                e.employee_name,
                o.total_amount,

                -- ✅ thêm các cột giống Cart
                p.product_id,
                p.product_name,
                p.size,
                p.color,
                p.stock,

                od.quantity,
                od.unit_price,
                (od.quantity * od.unit_price) AS line_total
            FROM orders o
            JOIN order_details od ON o.order_id = od.order_id
            JOIN products p ON p.product_id = od.product_id
            LEFT JOIN customers c ON c.customer_id = o.customer_id
            LEFT JOIN employees e ON e.employee_id = o.employee_id
            WHERE 1=1
                AND (@kwCus IS NULL OR c.customer_name LIKE @kwCus)
                AND (@kwPro IS NULL OR p.product_name LIKE @kwPro)
                AND (@fromDate IS NULL OR DATE(o.order_date) >= DATE(@fromDate))
                AND (@toDate IS NULL OR DATE(o.order_date) <= DATE(@toDate))
            ORDER BY o.order_date DESC, o.order_id DESC;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@kwCus",
                        string.IsNullOrWhiteSpace(kwCustomer) ? (object)DBNull.Value : "%" + kwCustomer + "%");

                    cmd.Parameters.AddWithValue("@kwPro",
                        string.IsNullOrWhiteSpace(kwProduct) ? (object)DBNull.Value : "%" + kwProduct + "%");

                    cmd.Parameters.AddWithValue("@fromDate",
                        from.HasValue ? (object)from.Value.Date : DBNull.Value);

                    cmd.Parameters.AddWithValue("@toDate",
                        to.HasValue ? (object)to.Value.Date : DBNull.Value);

                    using (var da = new MySqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }
    
    }
}
