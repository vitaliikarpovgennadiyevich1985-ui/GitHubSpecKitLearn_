var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", "Strong!Pass123", secret: true);
var identityDatabase = builder.AddSqlServer("SqlServer", sqlPassword).WithDataVolume("SqlServerVolume").AddDatabase("SqlServerIdentityDatabase");

var identityServerProject = builder.AddProject<Projects.Asp_Net_Core_Learning_IdentityServer>("IdentityServer").WithReference(identityDatabase)
    .WaitFor(identityDatabase);    

var catalogMicroserviceProject = builder.AddProject<Projects.Asp_Net_Core_Learning_CatalogMicroservice>("CatalogMicroservice")
    .WithReference(identityServerProject).WaitFor(identityServerProject);

var shoppingBasketMicroserviceProject = builder.AddProject<Projects.Asp_Net_Core_Learning_ShoppingBasketMicroservice>("ShoppingBasketMicroservice")
    .WithReference(identityServerProject).WithReference(catalogMicroserviceProject).WaitFor(identityServerProject);

var orderMicroserviceProject = builder.AddProject<Projects.Asp_Net_Core_Learning_OrderMicroservice>("OrderMicroservice")
    .WithReference(identityServerProject).WithReference(catalogMicroserviceProject).WithReference(shoppingBasketMicroserviceProject).WaitFor(identityServerProject);

builder.AddProject<Projects.Asp_Net_Core_Learning_UI>("UI")
    .WithReference(identityServerProject).WithReference(catalogMicroserviceProject).WithReference(shoppingBasketMicroserviceProject).WithReference(orderMicroserviceProject)
    .WaitFor(identityServerProject);

builder.AddProject<Projects.Asp_Net_Core_Learning_APIGateway>("asp-net-core-learning-apigateway");

builder.Build().Run();
