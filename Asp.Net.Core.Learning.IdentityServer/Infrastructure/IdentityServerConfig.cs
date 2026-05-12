using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Asp.Net.Core.Learning.IdentityServer.Infrastructure
{
    public class IdentityServerConfig
    {
        private const string OpenidApiScope = IdentityServerConstants.StandardScopes.OpenId;
        private const string ProfileApiScope = IdentityServerConstants.StandardScopes.Profile;
        private const string CatalogMicroserviceReadApiScope = "Catalog-Microservice-Read-Api";
        private const string CatalogMicroserviceWriteApiScope = "Catalog-Microservice-Write-Api";
        private const string ShoppingBasketMicroserviceApiScope = "ShoppingBasket-Microservice-Api";
        private const string OrderMicroserviceApiScope = "Order-Microservice-Api";

        public static IEnumerable<ApiScope> ApiScopes =>
        [
            new ApiScope(CatalogMicroserviceReadApiScope, "Catalog Microservice Read"),
            new ApiScope(CatalogMicroserviceWriteApiScope, "Catalog Microservice Write"),
            new ApiScope(ShoppingBasketMicroserviceApiScope, "Shopping Basket"),
            new ApiScope(OrderMicroserviceApiScope, "Order Basket")
        ];

        public static IEnumerable<ApiResource> ApiResources =>
        [
            new ApiResource("Catalog-Microservice-Api", ["role"])
            {
                Scopes = [CatalogMicroserviceReadApiScope, CatalogMicroserviceWriteApiScope]
            },
            new ApiResource("ShoppingBasket-Microservice-Api", ["role"])
            {
            Scopes =  [ShoppingBasketMicroserviceApiScope]
            },
            new ApiResource("Order-Microservice-Api", ["role"])
            {
                Scopes = [OrderMicroserviceApiScope]
            }
        ];

        public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResources.Phone(),
            new IdentityResource("roles", ["role"])
        ];

        public static IEnumerable<Client> Clients =>
        [
            new Client
            {
                ClientId = "WebUI",
                ClientSecrets = { new Secret("WebUISecret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireConsent = false,

                RedirectUris = { "https://localhost:7034/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:7034/signout-callback-oidc" },

                AllowedScopes = [OpenidApiScope, ProfileApiScope, "roles", IdentityServerConstants.StandardScopes.Email, IdentityServerConstants.StandardScopes.Phone, CatalogMicroserviceReadApiScope, CatalogMicroserviceWriteApiScope, ShoppingBasketMicroserviceApiScope, OrderMicroserviceApiScope],                                

                //Used to automatically get a new access token(probably identity and refresh tokens as well) when access token is expired.
                AllowOfflineAccess = true,
                UpdateAccessTokenClaimsOnRefresh = true,

                //AccessTokenLifetime = 10,
                //AbsoluteRefreshTokenLifetime = 1,
                //SlidingRefreshTokenLifetime = 1
            }
        ];
    }
}
