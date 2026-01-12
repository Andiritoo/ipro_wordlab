using DbMigrationService;
using Infrastructure.Database;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddSqlServerDbContext<WordLabDbContext>("wordlab");
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
