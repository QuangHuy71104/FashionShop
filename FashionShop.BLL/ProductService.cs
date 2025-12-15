using FashionShop.DAL;
using FashionShop.DTO;
using System;
using System.Data;

namespace FashionShop.BLL
{
    public class ProductService
    {
        private readonly ProductRepository repo = new ProductRepository();

        public DataTable GetAll() => repo.GetAll();
        public DataTable Search(string kw) => repo.Search(kw);

        public DataTable GetCategories() => repo.GetCategories();
        public DataTable GetColors() => repo.GetColors();

        public DataTable GetForSale() => repo.GetProductsForSale();

        // ✅ NEW: lấy ảnh (BLOB) theo product_code để FrmProducts hiển thị khi click
        public Tuple<byte[], string> GetImageByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Tuple.Create<byte[], string>(null, null);

            return repo.GetImageByCode(code.Trim());
        }

        public bool Add(Product p, out string err)
        {
            err = "";

            if (p == null)
            {
                err = "Dữ liệu không hợp lệ";
                return false;
            }

            if (string.IsNullOrWhiteSpace(p.Code) || string.IsNullOrWhiteSpace(p.Name))
            {
                err = "Mã/tên không được trống";
                return false;
            }

            if (repo.ExistsCode(p.Code))
            {
                err = "Mã đã tồn tại";
                return false;
            }

            if (p.Price <= 0)
            {
                err = "Giá > 0";
                return false;
            }

            if (p.Stock < 0)
            {
                err = "Tồn kho >= 0";
                return false;
            }

            // ảnh BLOB: cho phép null (không chọn ảnh vẫn add được)
            return repo.Insert(p) > 0;
        }

        public bool Update(Product p, out string err)
        {
            err = "";

            if (p == null)
            {
                err = "Dữ liệu không hợp lệ";
                return false;
            }

            // lưu ý: update của bạn hiện dùng product_code làm khóa
            if (string.IsNullOrWhiteSpace(p.Code))
            {
                err = "Mã sản phẩm không được trống";
                return false;
            }

            if (string.IsNullOrWhiteSpace(p.Name))
            {
                err = "Tên không được trống";
                return false;
            }

            if (p.Price <= 0)
            {
                err = "Giá > 0";
                return false;
            }

            if (p.Stock < 0)
            {
                err = "Tồn kho >= 0";
                return false;
            }

            return repo.Update(p) > 0;
        }

        public bool Delete(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            return repo.Delete(code.Trim()) > 0;
        }
    }
}
