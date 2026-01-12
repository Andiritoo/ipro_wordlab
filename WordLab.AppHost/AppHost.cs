var builder = DistributedApplication.CreateBuilder(args);

var sqlPasswordParam = builder.AddParameter("SqlServerPassword", true);

var database = builder
    .AddSqlServer("mssql", sqlPasswordParam, 44001)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("TZ", "Europe/Zurich")
    .WithDataVolume("wordLab-mssql-data")
    .AddDatabase("wordLab");

var migrator = builder
    .AddProject<Projects.DbMigrationService>("wordlab-db-migrator")
    .WithReference(database)
    .WaitFor(other: database);

builder
    .AddProject<Projects.WordLab>("webapp")
    .WithReference(database)
    .WaitForCompletion(migrator);

builder.Build().Run();
