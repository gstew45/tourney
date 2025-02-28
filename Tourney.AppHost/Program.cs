var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Tourney_ApiService>("apiservice");

builder.AddProject<Projects.Tourney_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
