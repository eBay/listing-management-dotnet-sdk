using EBay.ListingManagement.Clients.GraphQL;
using EBay.ListingManagement.Clients.Rest;
using EBay.ListingManagement.Configuration;
using EBay.ListingManagement.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EBay.ListingManagement.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void Registers_one_shared_entry_client_and_no_individual_api_clients()
    {
        var services = new ServiceCollection();
        services.AddListingManagementSdk(new ListingManagementConfiguration { AccessToken = "t" });

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<ListingManagementClient>();

        Assert.Same(client, provider.CreateScope().ServiceProvider.GetRequiredService<ListingManagementClient>());
        Assert.NotNull(client.Inventory);
        Assert.NotNull(client.GraphQL);
        Assert.Null(provider.GetService<InventoryClient>());
        Assert.Null(provider.GetService<ListingGraphQLClient>());
    }

    [Fact]
    public void Rejects_a_null_primary_handler()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ListingManagementClient(new ListingManagementConfiguration(), (HttpMessageHandler)null!));
    }
}
