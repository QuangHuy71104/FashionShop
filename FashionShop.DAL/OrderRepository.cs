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
    }
}
