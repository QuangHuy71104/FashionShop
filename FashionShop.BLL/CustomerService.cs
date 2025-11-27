using FashionShop.DAL;
using FashionShop.DTO;
using System.Data;

namespace FashionShop.BLL
{
    public class CustomerService
    {
        private CustomerRepository repo = new CustomerRepository();

        public DataTable GetAll() => repo.GetAll();
        public DataTable Search(string kw) => repo.Search(kw);
        public DataTable GetForCombo() => repo.GetForCombo();

        public bool Add(Customer c, out string err)
        {
            err = "";
            if (string.IsNullOrWhiteSpace(c.Name))
            { err = "Tên khách không được trống"; return false; }
            if (!string.IsNullOrWhiteSpace(c.Phone) && repo.ExistsPhone(c.Phone))
            { err = "SĐT đã tồn tại"; return false; }

            return repo.Insert(c) > 0;
        }

        public bool Update(Customer c, out string err)
        {
            err = "";
            if (string.IsNullOrWhiteSpace(c.Name))
            { err = "Tên khách không được trống"; return false; }
            return repo.Update(c) > 0;
        }

        public bool Delete(int id) => repo.Delete(id) > 0;
    }
}
