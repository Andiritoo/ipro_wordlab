var builder = DistributedApplication.CreateBuilder(args);

builder
    .AddProject<Projects.WordLab>("webapp");

builder.Build().Run();
