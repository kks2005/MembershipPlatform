using MembershipPlatform.Api.ErrorHandling;
using MembershipPlatform.Application.Classes;
using MembershipPlatform.Application.CheckIns;
using MembershipPlatform.Application.Members;
using MembershipPlatform.Data.Sqlite;
using MembershipPlatform.Data.SqlServer;
using MembershipPlatform.Storage.Local;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var persistenceProvider = builder.Configuration["Persistence:Provider"] ?? "Sqlite";
var storageProvider = builder.Configuration["Storage:Provider"] ?? "Local";
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];
string? sqliteConnectionString = null;

builder.Services.AddScoped<CheckInMember>();
builder.Services.AddScoped<GetMemberCheckIns>();
builder.Services.AddScoped<GetMembers>();
builder.Services.AddScoped<GetClasses>();
builder.Services.AddScoped<RegisterMemberForClass>();
builder.Services.AddScoped<GetClassesForMember>();
builder.Services.AddScoped<GetMembersForClass>();
builder.Services.AddScoped<GetClassRegistrationSummary>();
builder.Services.AddScoped<UploadMemberDocument>();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddExceptionHandler(options =>
    options.ExceptionHandler = _ => Task.CompletedTask);
builder.Services.AddControllers();
builder.Services.AddCors(options =>
    options.AddPolicy("ClientOrigins", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    }));

switch (persistenceProvider)
{
    case "Sqlite":
        sqliteConnectionString = builder.Configuration.GetConnectionString("Sqlite")
            ?? throw new InvalidOperationException(
                "The ConnectionStrings:Sqlite configuration value is required.");
        builder.Services.AddSqlitePersistence(sqliteConnectionString);
        break;

    case "SqlServer":
        var sqlServerConnectionString = builder.Configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException(
                "The ConnectionStrings:SqlServer configuration value is required.");
        builder.Services.AddSqlServerPersistence(sqlServerConnectionString);
        break;

    default:
        throw new InvalidOperationException(
            $"The persistence provider '{persistenceProvider}' is not supported.");
}

switch (storageProvider)
{
    case "Local":
        var configuredStorageRoot = builder.Configuration["Storage:Local:RootPath"];
        var localApplicationData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);
        var defaultStorageBase = string.IsNullOrWhiteSpace(localApplicationData)
            ? Path.GetTempPath()
            : localApplicationData;
        var localStorageRoot = string.IsNullOrWhiteSpace(configuredStorageRoot)
            ? Path.Combine(defaultStorageBase, "MembershipPlatform", "documents")
            : configuredStorageRoot;
        builder.Services.AddLocalMemberDocumentStorage(localStorageRoot);
        break;

    default:
        throw new InvalidOperationException(
            $"The storage provider '{storageProvider}' is not supported.");
}

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("ClientOrigins");

if (sqliteConnectionString is not null)
{
    await SqliteDatabaseInitializer.InitializeAsync(sqliteConnectionString);

    if (builder.Configuration.GetValue<bool>("Persistence:SeedData"))
    {
        await SqliteDataSeeder.SeedAsync(sqliteConnectionString);
    }
}

app.MapControllers();

app.Run();
