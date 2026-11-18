using EBay.ListingManagement.Configuration;
using Xunit;

namespace EBay.ListingManagement.Tests;

public sealed class TrustedHostsTests
{
    private const string SandboxInventory = "https://api.sandbox.ebay.com/sell/inventory/v1";
    private const string ProxyInventory = "https://ebay-proxy.example.com/sell/inventory/v1";
    private const string InventoryItems = """{"inventoryItems":[],"total":0}""";

    [Fact]
    public void Untrusted_base_url_is_rejected_by_both_constructors()
    {
        var config = new ListingManagementConfiguration { AccessToken = "t", InventoryBaseUrl = ProxyInventory };

        var standalone = Assert.Throws<InvalidOperationException>(() => new ListingManagementClient(config));
        Assert.Contains("AllowedHosts", standalone.Message);
        Assert.Throws<InvalidOperationException>(() =>
            new TestSdk(configure: c => c.InventoryBaseUrl = ProxyInventory));
    }

    [Fact]
    public async Task Sandbox_hosts_are_trusted_by_default()
    {
        var sdk = new TestSdk(configure: c => c.InventoryBaseUrl = SandboxInventory);
        sdk.Handler.ResponseBody = InventoryItems;

        await sdk.Client.Inventory.GetInventoryItemsAsync(limit: "1");

        Assert.Equal("api.sandbox.ebay.com", sdk.Handler.RequestUri!.Host);
        Assert.Equal("Bearer config-token", sdk.Handler.Authorization);
    }

    [Fact]
    public async Task Media_requests_use_the_configured_apim_host()
    {
        // The Media spec declares its server as a template; the configured base URL must win.
        var sdk = new TestSdk();
        sdk.Handler.ResponseBody = """{"imageUrl":"https://i.ebayimg.com/x.jpg"}""";

        await sdk.Client.Media.GetImageAsync("img-1");

        Assert.Equal("https://apim.ebay.com/commerce/media/v1_beta/image/img-1", sdk.Handler.RequestUri!.AbsoluteUri);
        Assert.Equal("Bearer config-token", sdk.Handler.Authorization);
    }

    [Fact]
    public async Task Additional_allowed_host_receives_rest_requests_with_token()
    {
        var sdk = new TestSdk(configure: c =>
        {
            c.InventoryBaseUrl = ProxyInventory;
            c.AllowedHosts.Add("ebay-proxy.example.com");
        });
        sdk.Handler.ResponseBody = InventoryItems;

        await sdk.Client.Inventory.GetInventoryItemsAsync(limit: "1");

        Assert.Equal("ebay-proxy.example.com", sdk.Handler.RequestUri!.Host);
        Assert.Equal("Bearer config-token", sdk.Handler.Authorization);
    }

    [Fact]
    public async Task Token_is_never_sent_to_a_host_configured_after_construction()
    {
        // Regression: the token used to be attached for any HTTPS host.
        var sdk = new TestSdk();
        sdk.Config.GraphQLBaseUrl = "https://attacker.example.com/graphql";

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sdk.Client.GraphQL.ExecuteRawUnsafeAsync("query { __typename }"));

        Assert.Equal(0, sdk.Handler.Calls);
    }

    [Fact]
    public void Default_hosts_are_read_only()
    {
        Assert.Contains("api.ebay.com", ListingManagementConfiguration.DefaultAllowedHosts);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<string>)ListingManagementConfiguration.DefaultAllowedHosts).Add("evil.example.com"));
    }
}
