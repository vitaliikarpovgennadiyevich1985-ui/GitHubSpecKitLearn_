using Asp.Net.Core.Learning.CatalogMicroservice.AuthorizationRequirements;
using Microsoft.AspNetCore.Authorization;

namespace Asp.Net.Core.Learning.CatalogMicroservice.AuthorizationReqHandlers
{
    public class CanReturnProductsAuthAttribute : AuthorizeAttribute, IAuthorizationRequirementData
    {
        public IEnumerable<IAuthorizationRequirement> GetRequirements()
        {
            return [new CanReturnProductsRequirement()];
        }
    }
}
