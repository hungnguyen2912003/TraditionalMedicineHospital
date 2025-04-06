using Project.Configurations;

var builder = WebApplication.CreateBuilder(args);

ServiceConfiguration.ConfigureServices(builder);
DatabaseConfiguration.ConfigureDatabase(builder);
RepositoryConfiguration.ConfigureRepository(builder);
AutoMapperConfiguration.ConfigureMappers(builder);
ValidatorConfiguration.ConfigureValidator(builder);
HelperConfiguration.ConfigureHelpers(builder);
JWTConfiguration.ConfigureJWT(builder);
SessionConfiguration.ConfigureSession(builder);

var app = builder.Build();
app.UseRouting();

EnvironmentConfiguration.ConfigureEnvironment(app);
MiddlewareConfiguration.ConfigureMiddleware(app);
PipelineConfiguration.ConfigurePipeline(app);
RoutingConfiguration.ConfigureRouting(app);

app.Run();
