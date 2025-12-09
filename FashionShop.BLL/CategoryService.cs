using System.Data;
using FashionShop.DAL;

namespace FashionShop.BLL
{
    public class CategoryService
    {
        private readonly CategoryRepository repo = new CategoryRepository();

        public DataTable GetAll()
        {
            return repo.GetAll();
        }

        public bool Add(string name, out string err)
        {
            err = "";

            if (string.IsNullOrWhiteSpace(name))
            {
                err = "Category name is required.";
                return false;
            }

            if (repo.ExistsName(name))
            {
                err = "Category name already exists.";
                return false;
            }

            return repo.Insert(name);
        }

        public bool Update(int id, string name, out string err)
        {
            err = "";

            if (id <= 0)
            {
                err = "Invalid category id.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                err = "Category name is required.";
                return false;
            }

            // nếu đổi tên sang tên đã có
            if (repo.ExistsName(name))
            {
                err = "Category name already exists.";
                return false;
            }

            return repo.Update(id, name);
        }

        public bool Delete(int id, out string err)
        {
            err = "";

            if (id <= 0)
            {
                err = "Invalid category id.";
                return false;
            }

            return repo.Delete(id);
        }
    }
}
