using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WeatherApp.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => 
{
    var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    var baseUri = new Uri(navigationManager.BaseUri);
    
    var httpsUrl = builder.Configuration["ApiSettings:BaseUrlHttps"] ?? "https://localhost:7228";
    var httpUrl = builder.Configuration["ApiSettings:BaseUrlHttp"] ?? "http://localhost:5006";
    
    var apiUri = baseUri.Scheme == "https" ? httpsUrl : httpUrl;
    return new HttpClient { BaseAddress = new Uri(apiUri) };
});

await builder.Build().RunAsync();
