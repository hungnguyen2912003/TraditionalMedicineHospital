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

// Configure the HTTP request pipeline
PipelineConfiguration.ConfigurePipeline(app);
MiddlewareConfiguration.ConfigureMiddleware(app);
EnvironmentConfiguration.ConfigureEnvironment(app);
RoutingConfiguration.ConfigureRouting(app);

app.Run();