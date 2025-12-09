using System.Data;
using FashionShop.DAL;

namespace FashionShop.BLL
{
    public class ColorService
    {
        private readonly ColorRepository repo = new ColorRepository();

        public DataTable GetAll()
        {
            return repo.GetAll();
        }

        public bool Add(string name, out string err)
        {
            err = "";
            if (string.IsNullOrWhiteSpace(name))
            {
                err = "Color name is required.";
                return false;
            }
            if (repo.ExistsName(name))
            {
                err = "Color already exists.";
                return false;
            }
            return repo.Insert(name);
        }

        public bool Update(int id, string name, out string err)
        {
            err = "";
            if (id <= 0)
            {
                err = "Invalid color id.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                err = "Color name is required.";
                return false;
            }
            if (repo.ExistsName(name))
            {
                err = "Color already exists.";
                return false;
            }
            return repo.Update(id, name);
        }

        public bool Delete(int id, out string err)
        {
            err = "";
            if (id <= 0)
            {
                err = "Invalid color id.";
                return false;
            }
            return repo.Delete(id);
        }
    }
}
