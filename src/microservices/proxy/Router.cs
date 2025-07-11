using System.Text;
using Microsoft.AspNetCore.Http.Extensions;

namespace Proxy;

public class Router
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public Router(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<HttpResponseMessage> RouteRequest(HttpRequest request)
    {
        var monolithUrl = Environment.GetEnvironmentVariable("MONOLITH_URL") ?? "http://monolith:8080";
        var moviesUrl = Environment.GetEnvironmentVariable("MOVIES_SERVICE_URL") ?? "http://movies-service:8081";
        var eventsUrl = Environment.GetEnvironmentVariable("EVENTS_SERVICE_URL") ?? "http://events-service:8082";
        var gradualMigrationFlagStr = Environment.GetEnvironmentVariable("GRADUAL_MIGRATION");
        var percentStr = Environment.GetEnvironmentVariable("MOVIES_MIGRATION_PERCENT");
        
        var gradualMigrationFlag = string.IsNullOrWhiteSpace(gradualMigrationFlagStr) || bool.Parse(gradualMigrationFlagStr);
        var percent = string.IsNullOrWhiteSpace(percentStr) ? 50 : double.Parse(percentStr);

        var urlStr = request.GetEncodedUrl();
        var ub = new UriBuilder(urlStr);

        Uri? uri = null;
        if (gradualMigrationFlag)
        {
			var isMicroservices = false;
            if (urlStr.Contains("movies", StringComparison.OrdinalIgnoreCase))
            {
                var puri = new Uri(moviesUrl);
                ub.Host = puri.Host;
                ub.Port = puri.Port;
				isMicroservices = true;
            }
            if (urlStr.Contains("events", StringComparison.OrdinalIgnoreCase))
            {
                var puri = new Uri(eventsUrl);
                ub.Host = puri.Host;
                ub.Port = puri.Port;
				isMicroservices = true;
            }

            if (isMicroservices && Random.Shared.Next(0, 100) <= percent)
            {
                uri = ub.Uri;
            }
            else
            {
                var puri = new Uri(monolithUrl);
                ub.Host = puri.Host;
                ub.Port = puri.Port;
                uri = ub.Uri;
            }
        }
		else
		{
			var puri = new Uri(monolithUrl);
            ub.Host = puri.Host;
            ub.Port = puri.Port;
            uri = ub.Uri;
		}

        return await SendRequest(request, uri);
    }

    private async Task<HttpResponseMessage> SendRequest(HttpRequest request, Uri destination)
    {
        using var client = _httpClientFactory.CreateClient();
        string requestContent;
        await using (Stream receiveStream = request.Body)
        {
            using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
            {
                requestContent = await readStream.ReadToEndAsync();
            }
        }

        var newRequest = new HttpRequestMessage(new HttpMethod(request.Method), destination);
        newRequest.Content = new StringContent(requestContent, Encoding.UTF8, request.ContentType);

        var response = await client.SendAsync(newRequest);
        return response;
    }
}