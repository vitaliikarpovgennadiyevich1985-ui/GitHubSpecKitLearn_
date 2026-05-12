using Asp.Net.Core.Learning.UI.Contracts;
using Asp.Net.Core.Learning.UI.Models;

namespace Asp.Net.Core.Learning.UI.Infrastructure
{
    public class CatalogService : ICatalogService
    {
        private readonly HttpClient _httpClient;

        public CatalogService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResult<Product>?> GetProducts(int pageNumber, int pageSize)
        {
            string requestUri = $"/products?pageNumber={pageNumber}&pageSize={pageSize}";
            return await _httpClient.GetFromJsonAsync<PagedResult<Product>>(requestUri);
        }
    }
}
