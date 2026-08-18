using MembershipPlatform.Web.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHttpClient<IMembershipApiClient, MembershipApiClient>((services, client) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["MembershipApi:BaseUrl"]
        ?? throw new InvalidOperationException(
            "The MembershipApi:BaseUrl configuration value is required.");
    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();
app.MapRazorPages();

app.Run();
