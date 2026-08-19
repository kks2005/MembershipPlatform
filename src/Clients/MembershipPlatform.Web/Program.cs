using MembershipPlatform.Web.Api;

var builder = WebApplication.CreateBuilder(args);

// Register Razor Pages as the UI framework for this client
builder.Services.AddRazorPages();

// Register IMembershipApiClient as a typed HttpClient
// This demonstrates dependency injection and the typed client pattern
// The HttpClient is configured with a base address from appsettings.json
builder.Services.AddHttpClient<IMembershipApiClient, MembershipApiClient>((services, client) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();

    // Read the API base URL from configuration
    // This allows different environments (Development, Production) to use different APIs
    var baseUrl = configuration["MembershipApi:BaseUrl"]
        ?? throw new InvalidOperationException(
            "The MembershipApi:BaseUrl configuration value is required.");

    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    // In production, use a global error handler and enforce HTTPS
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Enable serving static files (CSS, images, etc.) from wwwroot folder
app.UseStaticFiles();

// Enable routing to match incoming requests to Razor Pages
app.UseRouting();

// Enable anti-forgery token validation for form posts
app.UseAntiforgery();

// Map Razor Pages as endpoints
app.MapRazorPages();

app.Run();
