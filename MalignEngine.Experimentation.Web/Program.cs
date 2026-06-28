using MalignEngine;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace MalignEngine.Experimentation.Web;

class Program
{
    public static async Task Main(string[] args)
    {
        Application application = new Application();

        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<Home>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        var app = builder.Build();

        await app.RunAsync();
    }

}