using MembershipPlatform.Blazor;
using MembershipPlatform.Blazor.Api;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["MembershipApi:BaseUrl"]
    ?? throw new InvalidOperationException(
        "The MembershipApi:BaseUrl configuration value is required.");

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl, UriKind.Absolute)
});
builder.Services.AddScoped<IMembershipApiClient, MembershipApiClient>();

await builder.Build().RunAsync();
