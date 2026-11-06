using EBay.ListingManagement;
using EBay.ListingManagement.Configuration;
using EBay.ListingManagement.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ListingManagementTestApp;

internal enum EbayEnvironment
{
    Production,
    Sandbox,
}

/// <summary>Builds the SDK client for production or the eBay sandbox, standalone or through DI.</summary>
internal static class SdkFactory
{
    public const string SandboxGraphQLUrl = "https://graphqlapi.sandbox.ebay.com/graphql";

    public static ListingManagementConfiguration Configure(EbayEnvironment environment, string? token, string? graphQLUrl = null, bool verbose = false)
    {
        var config = new ListingManagementConfiguration { AccessToken = token, DiagnosticLogging = verbose };
        if (environment == EbayEnvironment.Sandbox)
        {
            // The REST sandbox hosts are in DefaultAllowedHosts, so they need no AllowedHosts entry.
            config.InventoryBaseUrl = "https://api.sandbox.ebay.com/sell/inventory/v1";
            config.FeedBaseUrl = "https://api.sandbox.ebay.com/sell/feed/v1";
            config.MediaBaseUrl = "https://apim.sandbox.ebay.com/commerce/media/v1_beta";
            config.StoresBaseUrl = "https://api.sandbox.ebay.com/sell/stores/v1";

            // api.sandbox.ebay.com/graphql returns 404; the sandbox GraphQL API is on graphqlapi.sandbox.ebay.com
            // (verified with a sandbox token). That host is not one of the SDK's defaults yet, so it is trusted explicitly.
            config.GraphQLBaseUrl = SandboxGraphQLUrl;
            config.AllowedHosts.Add(new Uri(SandboxGraphQLUrl).Host);
        }
        if (graphQLUrl is not null)
        {
            var host = new Uri(graphQLUrl).Host;
            if (!ListingManagementConfiguration.DefaultAllowedHosts.Contains(host, StringComparer.OrdinalIgnoreCase))
            {
                config.AllowedHosts.Add(host);
            }
            config.GraphQLBaseUrl = graphQLUrl;
        }
        return config;
    }

    /// <summary>The client and whatever owns it; disposing it disposes the client (or the DI container).</summary>
    public static (ListingManagementClient Client, IDisposable Owner) Create(
        ListingManagementConfiguration config, bool useDependencyInjection, HttpMessageHandler? handler = null)
    {
        if (!useDependencyInjection)
        {
            var client = handler is null ? new ListingManagementClient(config) : new ListingManagementClient(config, handler);
            return (client, client);
        }

        var services = new ServiceCollection();
        services.AddListingManagementSdk(config);
        if (handler is not null)
        {
            // A test double replaces the registered client; production code would not do this.
            services.AddSingleton(new ListingManagementClient(config, handler));
        }
        var provider = services.BuildServiceProvider();
        return (provider.GetRequiredService<ListingManagementClient>(), provider);
    }
}
