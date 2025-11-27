namespace FashionShop.DTO
{
    public class Product
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string Gender { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
