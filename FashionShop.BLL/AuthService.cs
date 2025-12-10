using FashionShop.DAL;
using FashionShop.DTO;

namespace FashionShop.BLL
{
    public class AuthService
    {
        private AuthRepository repo = new AuthRepository();

        public Account Login(string username, string password, out string err)
        {
            err = "";

            if (string.IsNullOrWhiteSpace(username))
            {
                err = "Please enter your username.";
                return null;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                err = "Please enter your password.";
                return null;
            }

            string passHash = HashHelper.Sha256(password);
            var acc = repo.Login(username, passHash);

            if (acc == null)
            {
                err = "Login failed. Please contact the Admin.";
                return null;
            }

            return acc;
        }

    }
}
