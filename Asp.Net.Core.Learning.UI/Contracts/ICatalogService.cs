using Asp.Net.Core.Learning.UI.Models;

namespace Asp.Net.Core.Learning.UI.Contracts
{
    public interface ICatalogService
    {
        Task<PagedResult<Product>?> GetProducts(int pageNumber, int pageSize);
    }
}
