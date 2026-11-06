using EBay.ListingManagement;
using EBay.ListingManagement.Exceptions;
using EBay.ListingManagement.Generated.GraphQL;

namespace ListingManagementTestApp;

/// <summary>Read-only calls against eBay: nothing is created, changed or ended.</summary>
internal static class LiveRun
{
    public static async Task<int> RunAsync(Options options, HttpMessageHandler? handler = null)
    {
        var config = SdkFactory.Configure(options.Environment, options.Token, options.GraphQLUrl, options.Verbose);
        var (client, owner) = SdkFactory.Create(config, options.UseDependencyInjection, handler);
        using var _ = owner;

        Console.WriteLine($"Environment: {options.Environment}   client: {(options.UseDependencyInjection ? "AddListingManagementSdk" : "new ListingManagementClient")}");
        Console.WriteLine($"GraphQL: {config.GraphQLBaseUrl}");
        Console.WriteLine($"Inventory: {config.InventoryBaseUrl}");
        Console.WriteLine();

        var failures = 0;
        var listingIds = await Step("GraphQL sellerListingSearch (active listings)", () => SearchActiveListingsAsync(client, options.Limit));
        if (listingIds is { Count: > 0 })
        {
            await Step("GraphQL sellerListings (details for those listings)", () => ListingDetailsAsync(client, listingIds));
        }
        await Step("REST Inventory getInventoryItems", () => InventoryItemsAsync(client, options.Limit));
        await Step("REST Stores getStore", () => StoreAsync(client));

        Console.WriteLine(failures == 0 ? "All calls succeeded." : $"{failures} call(s) failed (see above).");
        return failures == 0 ? 0 : 1;

        async Task<T?> Step<T>(string title, Func<Task<T>> call)
        {
            Console.WriteLine($"== {title}");
            try
            {
                var result = await call();
                Console.WriteLine();
                return result;
            }
            catch (Exception ex)
            {
                failures++;
                Console.WriteLine($"   FAILED: {Describe(ex)}");
                Console.WriteLine();
                return default;
            }
        }
    }

    private static async Task<IReadOnlyList<string>> SearchActiveListingsAsync(ListingManagementClient client, int limit)
    {
        var result = await client.GraphQL.SellerListingSearchAsync(new SellerActiveListingSearchInput
        {
            Pagination = new PaginationInput { MaxPageSize = limit },
        });

        switch (result?.ActiveListings)
        {
            case ISellerListingSearch_SellerListingSearch_ActiveListings_SellerListings found:
                var ids = found.Listings?.Select(l => l.ListingId).ToList() ?? [];
                Console.WriteLine($"   {found.Pagination?.TotalElements ?? ids.Count} active listing(s); showing {ids.Count}: {string.Join(", ", ids)}");
                return ids;
            case ISellerListingSearch_SellerListingSearch_ActiveListings_SellerListingSearchInvalidInput invalid:
                throw new InvalidOperationException($"{invalid.ErrorCode}: {invalid.DisplayableErrorMessage}");
            default:
                Console.WriteLine("   no result");
                return [];
        }
    }

    private static async Task<int> ListingDetailsAsync(ListingManagementClient client, IReadOnlyList<string> listingIds)
    {
        var result = await client.GraphQL.SellerListingsAsync(new SellerListingsInput
        {
            Listings = listingIds.Select(id => new SellerListingInput { ListingId = id }).ToList(),
        });

        Console.WriteLine($"   {"Listing",-14} {"Kind",-10} {"Format",-14} {"Status",-10} {"Qty",4}  Title");
        foreach (var entry in result?.Listings ?? [])
        {
            if (entry is not ISellerListings_SellerListings_Listings_ListingSuccess { Listing: { } listing } success)
            {
                Console.WriteLine($"   {entry.__typename}");
                continue;
            }
            var kind = listing switch
            {
                ISellerListings_SellerListings_Listings_Listing_VariationListing => "variation",
                ISellerListings_SellerListings_Listings_Listing_SingleSkuListing => "single",
                _ => listing.GetType().Name,
            };
            var terms = listing.ListingTerms;
            Console.WriteLine(
                $"   {success.ListingId,-14} {kind,-10} {terms?.ListingFormat,-14} {terms?.LifecycleTerms?.ListingStatus,-10} " +
                $"{terms?.TotalQuantityAvailable,4}  {listing.SellerProduct?.Title}");
        }
        return result?.Listings.Count ?? 0;
    }

    private static async Task<int> InventoryItemsAsync(ListingManagementClient client, int limit)
    {
        var page = await client.Inventory.GetInventoryItemsAsync(limit: limit.ToString());
        var items = page?.InventoryItemsProp ?? [];
        Console.WriteLine($"   {page?.Total ?? 0} inventory item(s); showing {items.Count}");
        foreach (var item in items)
        {
            Console.WriteLine($"   {item.Sku,-24} qty {item.Availability?.ShipToLocationAvailability?.Quantity,4}  {item.Product?.Title}");
        }
        return items.Count;
    }

    private static async Task<bool> StoreAsync(ListingManagementClient client)
    {
        var store = await client.Stores.GetStoreAsync();
        Console.WriteLine($"   store: {store?.Name ?? "(none)"} {store?.Url}");
        return store is not null;
    }

    private static string Describe(Exception ex) => ex switch
    {
        UnauthorizedException => "401 Unauthorized: the token is missing, expired, for the other environment, or lacks the scope.",
        GraphQLException graphQL => "GraphQL errors: " + string.Join("; ", graphQL.Errors),
        HttpRequestException http => $"HTTP error: {http.Message}",
        // The specs declare error bodies as a single Error, but eBay sends {"errors":[...]}, so the mapped
        // exception has no details; only the status code is reliable. --verbose logs the request.
        Microsoft.Kiota.Abstractions.ApiException api => $"HTTP {api.ResponseStatusCode} ({api.GetType().FullName}); no error details, see README",
        _ => $"{ex.GetType().Name}: {ex.Message}",
    };
}
