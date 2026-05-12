namespace Asp.Net.Core.Learning.CatalogMicroservice.Models
{
    public class Product
    {
        public string ProductId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string Image { get; set; } = string.Empty;
    }
}
