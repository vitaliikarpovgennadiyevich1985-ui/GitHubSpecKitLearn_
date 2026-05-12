using Asp.Net.Core.Learning.UI.Contracts;
using Asp.Net.Core.Learning.UI.Infrastructure;
using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddOpenIdConnectAccessTokenManagement();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AccessTokenHandler>();
builder.Services.AddHttpClient<ICatalogService, CatalogService>(client =>
{
    client.BaseAddress = new Uri("https://CatalogMicroservice");
})
//Automatic from Udende nuget package + automatic refresh of an access token when expired
.AddUserAccessTokenHandler();
//Manual via custom handler
//.AddHttpMessageHandler<AccessTokenHandler>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => 
{
    //options.AccessDeniedPath = "/Account/AccessDenied";    
    options.Events.OnSigningOut += async soContext =>
    {        
        await soContext.HttpContext.RevokeRefreshTokenAsync();        
    };
})
.AddOpenIdConnect(options =>
{
    options.Authority = builder.Configuration["IDENTITYSERVER_HTTPS"];

    options.ClientId = "WebUI";
    options.ClientSecret = "WebUISecret";
    options.ResponseType = "code";
    options.UsePkce = true;

    //Used to get refresh token from identity server(duende in our case) together with identity and access tokens and save them in an encrypted cookie
    options.Scope.Add("offline_access");
    options.Scope.Add("Catalog-Microservice-Read-Api");
    options.Scope.Add("Catalog-Microservice-Write-Api");
    options.Scope.Add("ShoppingBasket-Microservice-Api");
    options.Scope.Add("Order-Microservice-Api");    
    options.Scope.Add("email");
    options.Scope.Add("phone");
    options.Scope.Add("roles");
    
    options.GetClaimsFromUserInfoEndpoint = true;

    //We can't use ClaimTypes.* as it introduces namespaces in claim types
    options.MapInboundClaims = false;
    
    //Probably mappings not needed
    options.ClaimActions.MapUniqueJsonKey("name", "name");
    options.ClaimActions.MapUniqueJsonKey("email", "email");
    
    options.ClaimActions.MapJsonKey("role", "role");
    options.ClaimActions.MapUniqueJsonKey("mobilephone", "phone");

    options.TokenValidationParameters = new TokenValidationParameters
    { 
        NameClaimType = "name",
        RoleClaimType = "role"
    };

    options.SaveTokens = true;    
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.SignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;   
});
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets().RequireAuthorization();

app.Run();
