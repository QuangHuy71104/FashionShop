using FashionShop.DAL;
using FashionShop.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FashionShop.BLL
{
    public class OrderService
    {
        private OrderRepository repo = new OrderRepository();

        public int Checkout(int employeeId, int? customerId, List<OrderDetail> cart, out string err)
        {
            err = "";
            if (cart == null || cart.Count == 0)
            { err = "Giỏ hàng trống"; return -1; }
            if (cart.Any(x => x.Quantity <= 0))
            { err = "Số lượng không hợp lệ"; return -1; }

            decimal total = cart.Sum(x => x.SubTotal);
            return repo.InsertOrder(employeeId, customerId, total, cart);
        }

        public decimal GetRevenue()
        {
            try
            {
                return repo.GetRevenue();
            }
            catch
            {
                return 0m;
            }
        }

    }
}
