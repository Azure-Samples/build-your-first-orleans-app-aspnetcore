using Microsoft.AspNetCore.Http.Extensions;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;

var builder = WebApplication.CreateBuilder();

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryGrainStorage("urls");
});

var app = builder.Build();

var grainFactory = app.Services.GetRequiredService<IGrainFactory>();

app.MapGet("/", () => "Hello World!");

app.MapGet("/shorten/{*path}", async (HttpContext context, string path) =>
{
    var shortenedRouteSegment = Guid.NewGuid().GetHashCode().ToString("X");
    var shortenerGrain = grainFactory.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
    await shortenerGrain.SetUrl(shortenedRouteSegment, path);
    var resultBuilder = new UriBuilder(context.Request.GetEncodedUrl())
    {
        Path = $"/go/{shortenedRouteSegment}"
    };

    return Results.Ok(resultBuilder.Uri);
});

app.MapGet("/go/{shortenedRouteSegment}", async (string shortenedRouteSegment) =>
{
    var shortenerGrain = grainFactory.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
    var url = await shortenerGrain.GetUrl();

    return url is not null ? Results.Redirect(url) : Results.NotFound();
});

app.Run();


public class UrlShortenerGrain : Grain, IUrlShortenerGrain
{
    private IPersistentState<KeyValuePair<string, string>> _cache;

    public UrlShortenerGrain(
        [PersistentState(
            stateName: "url",
            storageName: "urls")]
            IPersistentState<KeyValuePair<string, string>> state)
    {
        _cache = state;
    }

    public async Task SetUrl(string shortenedRouteSegment, string fullUrl)
    {
        _cache.State = new KeyValuePair<string, string>(shortenedRouteSegment, fullUrl);
        await _cache.WriteStateAsync();
    }

    public Task<string> GetUrl()
    {
        return Task.FromResult(_cache.State.Value);
    }
}

public interface IUrlShortenerGrain : IGrainWithStringKey
{
    Task SetUrl(string shortenedRouteSegment, string fullUrl);
    Task<string> GetUrl();
}