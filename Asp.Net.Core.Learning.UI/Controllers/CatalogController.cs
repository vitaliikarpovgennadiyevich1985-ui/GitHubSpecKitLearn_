using Asp.Net.Core.Learning.UI.Contracts;
using Asp.Net.Core.Learning.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace Asp.Net.Core.Learning.UI.Controllers
{
    public class CatalogController : Controller
    {
        private static readonly int[] AvailablePageSizes = [10, 20, 50, 100];
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 20;

        private readonly ICatalogService _catalogService;

        public CatalogController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        public async Task<IActionResult> Index(int? pageNumber, int? pageSize)
        {
            var pagedResult = await _catalogService.GetProducts(pageNumber ?? DefaultPageNumber, pageSize ?? DefaultPageSize);
            var model = ToCatalogPageViewModel(pagedResult, pageNumber, pageSize);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Page(int? pageNumber, int? pageSize)
        {
            var pagedResult = await _catalogService.GetProducts(pageNumber ?? DefaultPageNumber, pageSize ?? DefaultPageSize);
            var model = ToCatalogPageViewModel(pagedResult, pageNumber, pageSize);
            return PartialView("_CatalogPage", model);
        }

        private static CatalogPageViewModel ToCatalogPageViewModel(PagedResult<Product>? pagedResult, int? requestedPageNumber, int? requestedPageSize)
        {
            if (pagedResult is null)
            {
                int fallbackPageSize = NormalizePageSize(requestedPageSize);
                int fallbackPageNumber = NormalizePageNumber(requestedPageNumber);

                return new CatalogPageViewModel
                {
                    Items = [],
                    TotalCount = 0,
                    PageNumber = fallbackPageNumber,
                    PageSize = fallbackPageSize,
                    AvailablePageSizes = AvailablePageSizes,
                    PageWindow = []
                };
            }

            int safePageSize = pagedResult.PageSize > 0 ? pagedResult.PageSize : NormalizePageSize(requestedPageSize);
            int safePageNumber = pagedResult.PageNumber > 0 ? pagedResult.PageNumber : NormalizePageNumber(requestedPageNumber);

            int totalPages = safePageSize <= 0
                ? 0
                : (int)Math.Ceiling(pagedResult.TotalCount / (double)safePageSize);

            return new CatalogPageViewModel
            {
                Items = pagedResult.Items,
                TotalCount = pagedResult.TotalCount,
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                AvailablePageSizes = AvailablePageSizes,
                PageWindow = BuildPageWindow(safePageNumber, totalPages)
            };
        }

        private static int NormalizePageNumber(int? pageNumber)
        {
            int normalized = pageNumber.GetValueOrDefault(DefaultPageNumber);
            return normalized < 1 ? DefaultPageNumber : normalized;
        }

        private static int NormalizePageSize(int? pageSize)
        {
            int normalized = pageSize.GetValueOrDefault(DefaultPageSize);
            if (!AvailablePageSizes.Contains(normalized))
            {
                return DefaultPageSize;
            }

            return normalized;
        }

        private static IReadOnlyList<int?> BuildPageWindow(int currentPage, int totalPages)
        {
            if (totalPages <= 0)
            {
                return [];
            }

            var pages = new SortedSet<int>
            {
                1,
                totalPages,
                currentPage - 1,
                currentPage,
                currentPage + 1
            };

            pages.RemoveWhere(page => page < 1 || page > totalPages);

            var window = new List<int?>();
            int? previous = null;

            foreach (int page in pages)
            {
                if (previous.HasValue && page - previous.Value > 1)
                {
                    window.Add(null);
                }

                window.Add(page);
                previous = page;
            }

            return window;
        }
    }
}
