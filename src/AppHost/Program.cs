using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
builder.Configuration.Sources.Add(new JsonConfigurationSource { Path = "appsettings.Local.json", Optional = true });

var isE2ETest = builder.Configuration["E2E_TEST"] == "true";

var dbPassword = builder.AddParameter("PostgresPassword", secret: true);

var postgresServer = builder
    .AddPostgres("eshopsupport-postgres", password: dbPassword);
var backendDb = postgresServer
    .AddDatabase("backenddb");

var vectorDb = builder
    .AddQdrant("vector-db");

var identityServer = builder.AddProject("identity-server", "../IdentityServer/IdentityServer.csproj")
    .WithExternalHttpEndpoints();

var identityEndpoint = identityServer
    .GetEndpoint("https");

// Use this if you want to use Ollama
var chatCompletion = builder.AddOllama("chatcompletion").WithDataVolume();

// ... or use this if you want to use OpenAI (having also configured the API key in appsettings)
//var chatCompletion = builder.AddConnectionString("chatcompletion");

var storage = builder.AddAzureStorage("eshopsupport-storage");
if (builder.Environment.IsDevelopment())
{
    storage.RunAsEmulator(r =>
    {
        if (!isE2ETest)
        {
            r.WithDataVolume();
        }
    });
}

var blobStorage = storage.AddBlobs("eshopsupport-blobs");

var pythonInference = builder.AddPythonUvicornApp("python-inference",
    Path.Combine("..", "PythonInference"), port: 62394);

var redis = builder.AddRedis("redis");

var backend = builder.AddProject("backend", "../Backend/Backend.csproj")
    .WithReference(backendDb)
    .WithReference(chatCompletion)
    .WithReference(blobStorage)
    .WithReference(vectorDb)
    .WithReference(pythonInference)
    .WithReference(redis)
    .WithEnvironment("IdentityUrl", identityEndpoint)
    .WithEnvironment("ImportInitialDataDir", Path.Combine(builder.AppHostDirectory, "..", "..", "seeddata", isE2ETest ? "test" : "dev"));

var staffWebUi = builder.AddProject("staffwebui", "../StaffWebUI/StaffWebUI.csproj")
    .WithExternalHttpEndpoints()
    .WithReference(backend)
    .WithReference(redis)
    .WithEnvironment("IdentityUrl", identityEndpoint);

var customerWebUi = builder.AddProject("customerwebui", "../CustomerWebUI/CustomerWebUI.csproj")
    .WithReference(backend)
    .WithEnvironment("IdentityUrl", identityEndpoint);

// Circular references: IdentityServer needs to know the endpoints of the web UIs
identityServer
    .WithEnvironment("CustomerWebUIEndpoint", customerWebUi.GetEndpoint("https"))
    .WithEnvironment("StaffWebUIEndpoint", staffWebUi.GetEndpoint("https"));

if (!isE2ETest)
{
    postgresServer.WithDataVolume();
    vectorDb.WithVolume("eshopsupport-vector-db-storage", "/qdrant/storage");
}

builder.Build().Run();

// Marker class for assembly discovery in tests (Aspire 13.x without workload)
public static class AssemblyMarker { }
