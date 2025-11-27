using System.Security.Cryptography;
using System.Text;

namespace FashionShop.BLL
{
    public static class HashHelper
    {
        public static string Sha256(string raw)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                var sb = new StringBuilder();
                foreach (var b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
