using System;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace FashionShop.DAL
{
    public static class DbContext
    {
        public static MySqlConnection GetConnection()
        {
            var setting = ConfigurationManager.ConnectionStrings["FashionShopDb"];
            if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
                throw new Exception("Không tìm thấy connection string 'FashionShopDb' trong App.config (project GUI).");

            return new MySqlConnection(setting.ConnectionString);
        }
    }
}
