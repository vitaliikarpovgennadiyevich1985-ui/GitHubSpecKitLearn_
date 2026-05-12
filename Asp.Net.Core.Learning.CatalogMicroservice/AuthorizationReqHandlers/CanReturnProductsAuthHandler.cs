using Asp.Net.Core.Learning.CatalogMicroservice.AuthorizationRequirements;
using Microsoft.AspNetCore.Authorization;

namespace Asp.Net.Core.Learning.CatalogMicroservice.AuthorizationReqHandlers
{
    public class CanReturnProductsAuthHandler : AuthorizationHandler<CanReturnProductsRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanReturnProductsRequirement requirement)
        {
            if (context.User.Identity is null || !context.User.Identity.IsAuthenticated)
            {                
                context.Fail();
                return;
            }

            if (!context.User.HasClaim("scope", "Catalog-Microservice-Read-Api"))
            {                
                context.Fail();
                return;
            }

            context.Succeed(requirement);
        }
    }
}
