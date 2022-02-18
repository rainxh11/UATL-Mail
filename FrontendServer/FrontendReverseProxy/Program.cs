using Microsoft.Extensions.FileProviders;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using EmbedIO;
using UATL.Mail.FrontendServer;

var htmlDir = new DirectoryInfo(@"D:\WebProjects\University-APP\uatl-mail\Frontend\dist");

var config = ConfigHelper.GetConfig();
var webServer = FrontendWebServer.CreateWebServer(htmlDir, config.Urls);

var server = webServer.RunAsync();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();

// "{*path:regex(^(?!api).*$)}"
app.UseRouting();
app.UseEndpoints(ep =>
{
    ep.Redirect("/", "/app");
});

var proxy = app.RunAsync();

await Task.WhenAll(server, proxy);


static class EndpointExtensions
{
    public static IEndpointRouteBuilder Redirect(
        this IEndpointRouteBuilder endpoints,
        string from, string to)
    {
        return Redirect(endpoints,
            new Redirective(from, to));
    }

    public static IEndpointRouteBuilder RedirectPermanent(
        this IEndpointRouteBuilder endpoints,
        string from, string to)
    {
        return Redirect(endpoints,
            new Redirective(from, to, true));
    }

    public static IEndpointRouteBuilder Redirect(
        this IEndpointRouteBuilder endpoints,
        params Redirective[] paths
    )
    {
        foreach (var (from, to, permanent) in paths)
        {
            endpoints.MapGet(from, async http => { http.Response.Redirect(to, permanent); });
        }

        return endpoints;
    }
}

record Redirective(string From, string To, bool Permanent = false);

//var htmlDir = new DirectoryInfo(@"D:\WebProjects\University-APP\uatl-mail\Frontend\dist");

