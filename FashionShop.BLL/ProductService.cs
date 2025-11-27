using FashionShop.DAL;
using FashionShop.DTO;
using System.Data;

namespace FashionShop.BLL
{
    public class ProductService
    {
        private ProductRepository repo = new ProductRepository();

        public DataTable GetAll() => repo.GetAll();
        public DataTable Search(string kw) => repo.Search(kw);
        public DataTable GetCategories() => repo.GetCategories();
        public DataTable GetForSale() => repo.GetProductsForSale();

        public bool Add(Product p, out string err)
        {
            err = "";
            if (string.IsNullOrWhiteSpace(p.Code) || string.IsNullOrWhiteSpace(p.Name))
            { err = "Mã/tên không được trống"; return false; }
            if (repo.ExistsCode(p.Code))
            { err = "Mã đã tồn tại"; return false; }
            if (p.Price <= 0) { err = "Giá > 0"; return false; }
            if (p.Stock < 0) { err = "Tồn kho >= 0"; return false; }

            return repo.Insert(p) > 0;
        }

        public bool Update(Product p, out string err)
        {
            err = "";
            if (string.IsNullOrWhiteSpace(p.Name))
            { err = "Tên không được trống"; return false; }
            if (p.Price <= 0) { err = "Giá > 0"; return false; }
            if (p.Stock < 0) { err = "Tồn kho >= 0"; return false; }

            return repo.Update(p) > 0;
        }

        public bool Delete(string code) => repo.Delete(code) > 0;
    }
}
