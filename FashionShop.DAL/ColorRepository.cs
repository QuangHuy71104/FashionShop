using System.Data;
using MySql.Data.MySqlClient;

namespace FashionShop.DAL
{
    public class ColorRepository
    {
        public DataTable GetAll()
        {
            var dt = new DataTable();
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT color_id, color_name FROM colors ORDER BY color_name", conn);

                using (var da = new MySqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }
            return dt;
        }

        public bool ExistsName(string name)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM colors WHERE color_name=@name", conn);
                cmd.Parameters.AddWithValue("@name", name);

                var count = System.Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        public bool Insert(string name)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "INSERT INTO colors(color_name) VALUES(@name)", conn);
                cmd.Parameters.AddWithValue("@name", name);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Update(int id, string name)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "UPDATE colors SET color_name=@name WHERE color_id=@id", conn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@id", id);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int id)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "DELETE FROM colors WHERE color_id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
