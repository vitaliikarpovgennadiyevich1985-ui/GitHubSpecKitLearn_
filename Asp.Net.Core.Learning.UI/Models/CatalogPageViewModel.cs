namespace Asp.Net.Core.Learning.UI.Models
{
    public class CatalogPageViewModel
    {
        public IReadOnlyList<Product> Items { get; set; } = [];

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public IReadOnlyList<int?> PageWindow { get; set; } = [];

        public IReadOnlyList<int> AvailablePageSizes { get; set; } = [10, 20, 50, 100];
    }
}
