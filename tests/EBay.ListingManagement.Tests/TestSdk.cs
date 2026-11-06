using System.Net;
using System.Text;
using System.Text.Json;
using EBay.ListingManagement.Configuration;

namespace EBay.ListingManagement.Tests;

/// <summary>SDK client whose every request goes through a fake primary handler.</summary>
internal sealed class TestSdk
{
    public FakeHandler Handler { get; } = new();
    public ListingManagementClient Client { get; }

    public ListingManagementConfiguration Config { get; }

    public TestSdk(string? accessToken = "config-token", Action<ListingManagementConfiguration>? configure = null)
    {
        Config = new ListingManagementConfiguration { AccessToken = accessToken };
        configure?.Invoke(Config);
        Client = new ListingManagementClient(Config, Handler);
    }
}

internal sealed class FakeHandler : HttpMessageHandler
{
    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
    public string ResponseBody { get; set; } = "{}";
    public JsonDocument? Request { get; private set; }
    public string? Authorization { get; private set; }
    public Uri? RequestUri { get; private set; }
    public int Calls { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? "{}" : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        Request = JsonDocument.Parse(body);
        Authorization = request.Headers.Authorization?.ToString();
        RequestUri = request.RequestUri;
        Calls++;
        return new HttpResponseMessage(Status)
        {
            Content = new StringContent(ResponseBody, Encoding.UTF8, "application/json"),
            RequestMessage = request
        };
    }
}
