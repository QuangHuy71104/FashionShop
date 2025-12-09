namespace FashionShop.DTO
{
    public class Product
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public int CategoryId { get; set; }
        public int SupplierId { get; set; }   // ✅ thêm

        public string Size { get; set; }

        public int ColorId { get; set; }      // ✅ dùng lưu DB
        public string Color { get; set; }     // (optional) dùng hiển thị

        public string Gender { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ImagePath { get; set; }
    }
}
