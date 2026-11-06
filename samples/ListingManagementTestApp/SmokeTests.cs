using System.Net;
using System.Text;
using System.Text.Json;
using EBay.ListingManagement.Configuration;
using EBay.ListingManagement.Exceptions;
using EBay.ListingManagement.Generated.GraphQL;

namespace ListingManagementTestApp;

/// <summary>
/// Offline checks of this SDK branch: the real client, serializers and auth pipeline, with only the network
/// replaced by canned eBay responses. Each check prints PASS or FAIL; the exit code is the number of failures.
/// </summary>
internal static class SmokeTests
{
    private const string Token = "smoke-token";

    public static async Task<int> RunAsync()
    {
        var failures = 0;

        async Task Check(string name, Func<Task> body)
        {
            try
            {
                await body();
                Console.WriteLine($"PASS  {name}");
            }
            catch (Exception ex)
            {
                failures++;
                Console.WriteLine($"FAIL  {name}\n      {ex.GetType().Name}: {ex.Message}");
            }
        }

        foreach (var environment in new[] { EbayEnvironment.Production, EbayEnvironment.Sandbox })
        {
            foreach (var useDi in new[] { false, true })
            {
                await Check($"live flow, {environment}, {(useDi ? "DI" : "standalone")}: REST + GraphQL, token only to configured hosts",
                    () => LiveFlowAsync(environment, useDi));
            }
        }
        await Check("GraphQL unions parse into the generated interfaces", UnionsAsync);
        await Check("GraphQL request carries operationName, variables and the per-call token", RequestShapeAsync);
        await Check("GraphQL errors throw GraphQLException with every message", GraphQLErrorsAsync);
        await Check("HTTP 401 throws UnauthorizedException (GraphQL and REST)", UnauthorizedAsync);
        await Check("a base URL on an untrusted host is rejected unless added to AllowedHosts", UntrustedHostAsync);
        await Check("the token is never sent to a GraphQL host changed after construction", HostChangedAfterConstructionAsync);

        Console.WriteLine(failures == 0 ? "\nSMOKE OK" : $"\nSMOKE FAILED ({failures})");
        return failures;
    }

    private static async Task LiveFlowAsync(EbayEnvironment environment, bool useDi)
    {
        var fake = new FakeEbay();
        var options = new Options(environment, Token, 2, null, useDi, false);

        var config = SdkFactory.Configure(environment, Token);
        var exitCode = await Quietly(() => LiveRun.RunAsync(options, fake));

        Assert(exitCode == 0, $"live flow exit code {exitCode}");
        Assert(fake.Requests.Select(r => r.Operation).SequenceEqual(["SellerListingSearch", "SellerListings", "inventory_item", "store"]),
            "calls: " + string.Join(", ", fake.Requests.Select(r => r.Operation)));
        var configured = new[] { config.GraphQLBaseUrl, config.InventoryBaseUrl, config.StoresBaseUrl };
        foreach (var request in fake.Requests)
        {
            Assert(configured.Any(url => request.Uri.AbsoluteUri.StartsWith(url, StringComparison.Ordinal)),
                $"{request.Operation} went to {request.Uri}, outside the configured base URLs");
            Assert(environment == EbayEnvironment.Production || request.Uri.Host.EndsWith(".sandbox.ebay.com", StringComparison.Ordinal),
                $"{request.Operation} left the sandbox: {request.Uri.Host}");
            Assert(request.Authorization == "Bearer " + Token, $"{request.Operation} Authorization: {request.Authorization}");
        }
        var details = fake.Requests[1].Body!.RootElement.GetProperty("variables").GetProperty("input").GetProperty("listings");
        Assert(details.GetArrayLength() == 2 && details[0].GetProperty("listingId").GetString() == "110000000001",
            "sellerListings did not receive the ids from sellerListingSearch: " + details);
    }

    private static async Task UnionsAsync()
    {
        using var client = new EBay.ListingManagement.ListingManagementClient(SdkFactory.Configure(EbayEnvironment.Production, Token), new FakeEbay());

        var search = await client.GraphQL.SellerListingSearchAsync(new SellerActiveListingSearchInput());
        var found = search?.ActiveListings as ISellerListingSearch_SellerListingSearch_ActiveListings_SellerListings;
        Assert(found is { Pagination.TotalElements: 2 }, "activeListings is not SellerListings with 2 elements");

        var details = await client.GraphQL.SellerListingsAsync(new SellerListingsInput
        {
            Listings = [new SellerListingInput { ListingId = "110000000001" }, new SellerListingInput { ListingId = "110000000002" }],
        });
        var listings = details!.Listings.OfType<ISellerListings_SellerListings_Listings_ListingSuccess>().Select(s => s.Listing).ToList();
        Assert(listings[0] is ISellerListings_SellerListings_Listings_Listing_SingleSkuListing, "first listing is not SingleSkuListing");
        Assert(listings[1] is ISellerListings_SellerListings_Listings_Listing_VariationListing, "second listing is not VariationListing");
        Assert(listings[0]!.ListingTerms?.LifecycleTerms?.ListingStatus == ListingStatusEnum.Active, "listing status not parsed as enum");
        Assert(listings[0]!.ListingTerms?.LifecycleTerms?.CreatedAt == new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero),
            "createdAt not parsed as DateTimeOffset");
    }

    private static async Task RequestShapeAsync()
    {
        var fake = new FakeEbay();
        using var client = new EBay.ListingManagement.ListingManagementClient(SdkFactory.Configure(EbayEnvironment.Production, Token), fake);

        await client.GraphQL.SellerListingSearchAsync(
            new SellerActiveListingSearchInput { Pagination = new PaginationInput { MaxPageSize = 3 } },
            accessToken: "per-call-token");

        var request = fake.Requests.Single();
        var body = request.Body!.RootElement;
        Assert(body.GetProperty("operationName").GetString() == "SellerListingSearch", "operationName");
        Assert(body.GetProperty("query").GetString()!.StartsWith("query SellerListingSearch(", StringComparison.Ordinal), "query text");
        var input = body.GetProperty("variables").GetProperty("activeListings_searchInput");
        Assert(input.GetProperty("pagination").GetProperty("maxPageSize").GetInt32() == 3, "maxPageSize variable");
        Assert(!input.TryGetProperty("filters", out _), "unset input fields must be omitted");
        Assert(request.Authorization == "Bearer per-call-token", "per-call token did not override the configured one");
    }

    private static async Task GraphQLErrorsAsync()
    {
        var fake = new FakeEbay { GraphQLOverride = """{"errors":[{"message":"boom"},{"message":"bang"}],"data":null}""" };
        using var client = new EBay.ListingManagement.ListingManagementClient(SdkFactory.Configure(EbayEnvironment.Production, Token), fake);

        var error = await Throws<GraphQLException>(() => client.GraphQL.SellerListingSearchAsync(new SellerActiveListingSearchInput()));
        Assert(error.Errors.SequenceEqual(["boom", "bang"]), "errors: " + string.Join(", ", error.Errors));
    }

    private static async Task UnauthorizedAsync()
    {
        var fake = new FakeEbay { Status = HttpStatusCode.Unauthorized };
        using var client = new EBay.ListingManagement.ListingManagementClient(SdkFactory.Configure(EbayEnvironment.Production, Token), fake);

        await Throws<UnauthorizedException>(() => client.GraphQL.SellerListingSearchAsync(new SellerActiveListingSearchInput()));
        await Throws<UnauthorizedException>(() => client.Inventory.GetInventoryItemsAsync());
    }

    private static Task UntrustedHostAsync()
    {
        var config = SdkFactory.Configure(EbayEnvironment.Production, Token, "https://ebay-proxy.example.com/graphql");
        Assert(config.AllowedHosts.Contains("ebay-proxy.example.com"), "--graphql-url did not add the proxy host to AllowedHosts");
        new EBay.ListingManagement.ListingManagementClient(config, new FakeEbay()).Dispose();

        config.AllowedHosts.Clear();
        var error = ThrowsSync<InvalidOperationException>(() => new EBay.ListingManagement.ListingManagementClient(config, new FakeEbay()));
        Assert(error.Message.Contains("AllowedHosts", StringComparison.Ordinal), "message should point to AllowedHosts: " + error.Message);
        return Task.CompletedTask;
    }

    private static async Task HostChangedAfterConstructionAsync()
    {
        var fake = new FakeEbay();
        var config = SdkFactory.Configure(EbayEnvironment.Production, Token);
        using var client = new EBay.ListingManagement.ListingManagementClient(config, fake);
        config.GraphQLBaseUrl = "https://attacker.example.com/graphql";

        await Throws<InvalidOperationException>(() => client.GraphQL.ExecuteRawUnsafeAsync("query { __typename }"));
        Assert(fake.Requests.Count == 0, "a request was sent");
    }

    private static async Task<T> Quietly<T>(Func<Task<T>> body)
    {
        var console = Console.Out;
        Console.SetOut(TextWriter.Null);
        try
        {
            return await body();
        }
        finally
        {
            Console.SetOut(console);
        }
    }

    private static async Task<TException> Throws<TException>(Func<Task> body) where TException : Exception
    {
        try
        {
            await body();
        }
        catch (TException ex)
        {
            return ex;
        }
        throw new InvalidOperationException($"expected {typeof(TException).Name}");
    }

    private static TException ThrowsSync<TException>(Action body) where TException : Exception
    {
        try
        {
            body();
        }
        catch (TException ex)
        {
            return ex;
        }
        throw new InvalidOperationException($"expected {typeof(TException).Name}");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    internal sealed record SeenRequest(string Operation, Uri Uri, string? Authorization, JsonDocument? Body);

    /// <summary>Answers the calls the test app makes with eBay-shaped payloads.</summary>
    private sealed class FakeEbay : HttpMessageHandler
    {
        public List<SeenRequest> Requests { get; } = [];
        public HttpStatusCode Status { get; init; } = HttpStatusCode.OK;
        public string? GraphQLOverride { get; init; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri!;
            var text = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            var body = string.IsNullOrEmpty(text) ? null : JsonDocument.Parse(text);
            var operation = body?.RootElement.TryGetProperty("operationName", out var name) == true
                ? name.GetString()!
                : uri.AbsolutePath.Split('/').Last();
            lock (Requests)
            {
                Requests.Add(new SeenRequest(operation, uri, request.Headers.Authorization?.ToString(), body));
            }

            var json = operation switch
            {
                _ when GraphQLOverride is not null && uri.AbsolutePath.EndsWith("/graphql", StringComparison.Ordinal) => GraphQLOverride,
                "SellerListingSearch" => SellerListingSearch,
                "SellerListings" => SellerListings,
                "inventory_item" => InventoryItems,
                "store" => Store,
                _ => null,
            };
            return new HttpResponseMessage(json is null ? HttpStatusCode.NotFound : Status)
            {
                RequestMessage = request,
                Content = new StringContent(json ?? "{}", Encoding.UTF8, "application/json"),
            };
        }
    }

    private const string SellerListingSearch = """
        {"data":{"sellerListingSearch":{"__typename":"SellerListingSearchOutput",
          "activeListings":{"__typename":"SellerListings",
            "listings":[{"__typename":"ListingReference","listingId":"110000000001"},{"__typename":"ListingReference","listingId":"110000000002"}],
            "pagination":{"__typename":"PaginationOutput","nextCursor":null,"previousCursor":null,"totalElements":2,"totalPages":1}},
          "scheduledListings":{"__typename":"SellerListings","listings":[],"pagination":null},
          "endedListings":{"__typename":"SellerListings","listings":[],"pagination":null}}}}
        """;

    private const string SellerListings = """
        {"data":{"sellerListings":{"__typename":"SellerListingsOutput","listings":[
          {"__typename":"ListingSuccess","listingId":"110000000001","listing":{"__typename":"SingleSkuListing","listingId":"110000000001",
            "marketplace":"EBAY_US","sellerProduct":{"__typename":"SellerProduct","title":"Vintage camera","ebayGeneratedTitle":null,"description":null,"images":null},
            "listingTerms":{"__typename":"ListingTerms","listingFormat":"FIXED_PRICE","totalQuantityAvailable":3,
              "lifecycleTerms":{"__typename":"ListingLifecycle","listingStatus":"ACTIVE","createdAt":"2026-10-01T12:00:00Z","startAt":null,"actualEndAt":null,"scheduledEndAt":null,"endListingReason":null,"listingDurationInDays":null}}}},
          {"__typename":"ListingSuccess","listingId":"110000000002","listing":{"__typename":"VariationListing","listingId":"110000000002",
            "marketplace":"EBAY_US","sellerProduct":{"__typename":"SellerProduct","title":"T-shirt","ebayGeneratedTitle":null,"description":null,"images":null},
            "listingTerms":null}}]}}}
        """;

    private const string InventoryItems = """
        {"total":1,"size":1,"inventoryItems":[{"sku":"CAM-001","product":{"title":"Vintage camera"},
          "availability":{"shipToLocationAvailability":{"quantity":3}}}]}
        """;

    private const string Store = """{"name":"Camera Corner","url":"https://www.ebay.com/str/cameracorner"}""";
}
