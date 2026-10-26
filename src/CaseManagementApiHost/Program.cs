using CaseManagementApiHost;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Microsoft.PowerPlatform.Dataverse.Client;

var builder = WebApplication.CreateBuilder(args);

// Which API to serve, which version, and how often to check for a newer one.
var logicApi = builder.Configuration.GetSection(LogicApiOptions.SectionName).Get<LogicApiOptions>() ?? new LogicApiOptions();

// The Dataverse connection loads the API definition and runs every call. It contains a
// secret: keep it in user secrets locally and in App Service or Key Vault in Azure.
var dataverse = builder.Configuration.GetConnectionString("Dataverse");
if (string.IsNullOrWhiteSpace(dataverse))
{
    throw new InvalidOperationException(
        "ConnectionStrings:Dataverse is not configured. Run 'dotnet user-secrets set \"ConnectionStrings:Dataverse\" \"<connection string>\"' " +
        "in the project folder, or set the connection string on the host.");
}

// Portal customers sign in with Microsoft Entra ID (or Entra External ID) and send a bearer token.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];
    });
builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();

// Finds the Dataverse contact behind a signed-in customer; used by the user resolver.
builder.Services.AddSingleton<ICustomerDirectory, DataverseCustomerDirectory>();

var api = builder.Services
    .AddFlowonApi(logicApi.Project, logicApi.Api, _ => new ServiceClient(dataverse))
    .AddRestServer(options =>
    {
        options.VersioningStrategy = ApiVersionSetting.ToStrategy(logicApi.Version);
    })
    .AddUserResolver<CustomerUserResolver>();

// Restarts the host when the draft changes or a newer version is published. Serving an
// exact version leaves nothing to watch, so the watcher is only added for draft and latest.
if (logicApi.MetadataWatchIntervalMinutes > 0 && ApiVersionSetting.IsWatchable(logicApi.Version))
{
    api.AddMetadataWatcher(options => options.TimeIntervalInMinutes = logicApi.MetadataWatchIntervalMinutes);
}

// Swagger describes the generated endpoints, so you can browse and try them while you
// develop. It is left out of other environments, which don't publish their API description.
if (builder.Environment.IsDevelopment())
{
    builder.Services
        .AddEndpointsApiExplorer()
        .AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = $"{logicApi.Api} ({logicApi.Project})",
                Version = logicApi.Version,
            });

            // Lets you paste an access token with Authorize, for endpoints that need a signed-in caller.
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "An access token for the API from your sign-in provider.",
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
            });
        });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{logicApi.Api} {logicApi.Version}");
    });
}

app.UseHttpsRedirection();
app.UseRouting();

// Authentication must run before the REST server, so that the user resolver sees the caller.
app.UseAuthentication();
app.UseAuthorization();

// Generates the endpoints from the API definition, including GET /health.
app.UseRestServer();

app.Run();
