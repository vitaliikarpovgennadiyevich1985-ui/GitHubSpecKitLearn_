using System.Security.Cryptography;
using System.Text;
using Asp.Net.Core.Learning.CatalogMicroservice.Models;

namespace Asp.Net.Core.Learning.CatalogMicroservice.Data
{
    internal static class ProductCatalog
    {
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 20;
        private const int MaxPageSize = 100;

        public static readonly IReadOnlyList<Product> All;

        static ProductCatalog()
        {
            var products = new Product[100];

            for (int i = 0; i < 200; i++)
            {
                double hue = i * 3.6;
                (byte r, byte g, byte b) = HsvToRgb(hue);

                products[i] = new Product
                {
                    ProductId = new Guid(MD5.HashData(Encoding.UTF8.GetBytes($"product-{i}"))).ToString(),
                    Title = $"Product {i + 1}",
                    Description = $"Description for Product {i + 1}",
                    Price = (i + 1) * 10m,
                    Image = ProductImageGenerator.GenerateBase64Bmp(r, g, b)
                };
            }

            All = products;

            // Startup validation guard — data defects surface immediately rather than silently at runtime
            if (All.Count != 100)
                throw new InvalidOperationException(
                    $"ProductCatalog must contain exactly 100 products but has {All.Count}.");

            foreach (var product in All)
            {
                if (string.IsNullOrEmpty(product.ProductId))
                    throw new InvalidOperationException("A product has a null or empty ProductId.");
                if (string.IsNullOrEmpty(product.Title))
                    throw new InvalidOperationException("A product has a null or empty Title.");
                if (string.IsNullOrEmpty(product.Description))
                    throw new InvalidOperationException("A product has a null or empty Description.");
                if (string.IsNullOrEmpty(product.Image))
                    throw new InvalidOperationException("A product has a null or empty Image.");
                if (product.Price <= 0)
                    throw new InvalidOperationException(
                        $"Product '{product.ProductId}' has a Price of {product.Price} which is <= 0.");
            }
        }

        /// <summary>
        /// Converts a hue angle (0–360°) with full saturation and value (S=1, V=1) to an RGB triple.
        /// Uses the standard 6-sector HSV → RGB algorithm.
        /// </summary>
        private static (byte r, byte g, byte b) HsvToRgb(double hue)
        {
            // C = V × S = 1 × 1 = 1; m = V – C = 0
            double C = 1.0;
            double X = C * (1.0 - Math.Abs(hue / 60.0 % 2.0 - 1.0));

            double r1, g1, b1;

            if      (hue < 60)  { r1 = C; g1 = X; b1 = 0; }
            else if (hue < 120) { r1 = X; g1 = C; b1 = 0; }
            else if (hue < 180) { r1 = 0; g1 = C; b1 = X; }
            else if (hue < 240) { r1 = 0; g1 = X; b1 = C; }
            else if (hue < 300) { r1 = X; g1 = 0; b1 = C; }
            else                { r1 = C; g1 = 0; b1 = X; }

            return ((byte)(r1 * 255), (byte)(g1 * 255), (byte)(b1 * 255));
        }

        public static PagedResult<Product> GetPage(int? pageNumber, int? pageSize)
        {
            int normalizedPageNumber = pageNumber.GetValueOrDefault(DefaultPageNumber);
            if (normalizedPageNumber < 1)
            {
                normalizedPageNumber = DefaultPageNumber;
            }

            int normalizedPageSize = pageSize.GetValueOrDefault(DefaultPageSize);
            if (normalizedPageSize < 1)
            {
                normalizedPageSize = DefaultPageSize;
            }
            else if (normalizedPageSize > MaxPageSize)
            {
                normalizedPageSize = MaxPageSize;
            }

            int totalCount = All.Count;
            int skip = (normalizedPageNumber - 1) * normalizedPageSize;

            var ordered = All
                .OrderBy(product => product.Title)
                .ThenBy(product => product.ProductId);

            var pageItems = ordered
                .Skip(skip)
                .Take(normalizedPageSize)
                .ToArray();

            return new PagedResult<Product>
            {
                Items = pageItems,
                TotalCount = totalCount,
                PageNumber = normalizedPageNumber,
                PageSize = normalizedPageSize
            };
        }
    }
}
