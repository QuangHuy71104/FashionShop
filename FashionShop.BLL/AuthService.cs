using FashionShop.DAL;
using FashionShop.DTO;

namespace FashionShop.BLL
{
    public class AuthService
    {
        private AuthRepository repo = new AuthRepository();

        public Account Login(string user, string pass, out string err)
        {
            err = "";
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            { err = "Nhập đủ username & password"; return null; }

            var acc = repo.Login(user, HashHelper.Sha256(pass));
            if (acc == null) err = "Sai tài khoản hoặc mật khẩu";
            return acc;
        }
    }
}
