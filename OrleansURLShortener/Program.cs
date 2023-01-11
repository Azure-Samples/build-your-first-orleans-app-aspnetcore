using Microsoft.AspNetCore.Http.Extensions;
using Orleans.Runtime;

var builder = WebApplication.CreateBuilder();

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryGrainStorage("urls");
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/shorten",
    async (IGrainFactory grains, HttpRequest request, string redirect) =>
    {
        // Create a unique, short ID
        var shortenedRouteSegment = Guid.NewGuid().GetHashCode().ToString("X");

        // Create and persist a grain with the shortened ID and full URL
        var shortenerGrain = grains.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
        await shortenerGrain.SetUrl(shortenedRouteSegment, redirect);

        // Return the shortened URL for later use
        var resultBuilder = new UriBuilder($"{request.Scheme}://{request.Host.Value}")
        {
            Path = $"/go/{shortenedRouteSegment}"
        };

        return Results.Ok(resultBuilder.Uri);
    });

app.MapGet("/go/{shortenedRouteSegment}",
    async (IGrainFactory grains, string shortenedRouteSegment) =>
    {
        // Retrieve the grain using the shortened ID and redirect to the original URL        
        var shortenerGrain = grains.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
        var url = await shortenerGrain.GetUrl();

        return Results.Redirect(url);
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