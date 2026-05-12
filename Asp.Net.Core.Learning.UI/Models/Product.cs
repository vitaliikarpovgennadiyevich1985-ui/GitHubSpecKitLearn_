namespace Asp.Net.Core.Learning.UI.Models
{
    public class Product
    {
        public string ProductId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public string Image { get; set; } = string.Empty;
    }
}
