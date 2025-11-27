using FashionShop.DTO;
using MySql.Data.MySqlClient;

namespace FashionShop.DAL
{
    public class AuthRepository
    {
        public Account Login(string user, string passHash)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT a.username, e.employee_id, e.employee_name, e.role
                               FROM accounts a
                               JOIN employees e ON a.employee_id=e.employee_id
                               WHERE a.username=@u AND a.password_hash=@p AND a.is_active=1";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@u", user);
                cmd.Parameters.AddWithValue("@p", passHash);

                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;

                    return new Account
                    {
                        Username = rd.GetString("username"),
                        EmployeeId = rd.GetInt32("employee_id"),
                        EmployeeName = rd.GetString("employee_name"),
                        Role = rd.GetString("role")
                    };
                }
            }
        }
    }
}
