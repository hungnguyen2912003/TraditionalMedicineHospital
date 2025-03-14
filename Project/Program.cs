using Project.Configurations;

var builder = WebApplication.CreateBuilder(args);

ServiceConfiguration.ConfigureServices(builder);
DatabaseConfiguration.ConfigureDatabase(builder);

var app = builder.Build();

EnvironmentConfiguration.ConfigureEnvironment(app);
PipelineConfiguration.ConfigurePipeline(app);
RoutingConfiguration.ConfigureRouting(app);

app.Run();
