using Asp.Net.Core.Learning.CatalogMicroservice.AuthorizationReqHandlers;
using Asp.Net.Core.Learning.CatalogMicroservice.AuthorizationRequirements;
using Asp.Net.Core.Learning.CatalogMicroservice.Data;
using Asp.Net.Core.Learning.CatalogMicroservice.Models;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(BearerTokenDefaults.AuthenticationScheme).AddJwtBearer(BearerTokenDefaults.AuthenticationScheme, options =>
{    
    options.Authority = builder.Configuration["IDENTITYSERVER_HTTPS"];
    
    options.TokenValidationParameters = new TokenValidationParameters 
    {
        ValidateAudience = true,        
        //To avoid JWT confusion attacks    
        ValidTypes = ["at+jwt"],
        NameClaimType = "name",
        RoleClaimType = "role",

        ValidateLifetime = true,
        //This property extends token expiratiob by its value
        //ClockSkew = TimeSpan.Zero
    };

    options.Audience = "Catalog-Microservice-Api";    
    
    options.MapInboundClaims = false;
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Catalog-Microservice-Read-Api", p =>
    {
        p.RequireAuthenticatedUser();
        p.RequireClaim("scope", "Catalog-Microservice-Read-Api");
        p.AddRequirements(new CanReturnProductsRequirement());
    });
});
builder.Services.AddScoped<IAuthorizationHandler, CanReturnProductsAuthHandler>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/products", (int? pageNumber, int? pageSize) => ProductCatalog.GetPage(pageNumber, pageSize))
   .RequireAuthorization("Catalog-Microservice-Read-Api");

app.Run();
