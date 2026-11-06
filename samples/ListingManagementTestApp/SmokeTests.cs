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
        await Check("--write refuses to run in production and sends nothing", WriteRefusedInProductionAsync);
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
        // Write calls only run in the sandbox, so the sandbox runs cover them too.
        var write = environment == EbayEnvironment.Sandbox;
        var options = new Options(environment, Token, 2, null, useDi, false, write);

        var config = SdkFactory.Configure(environment, Token);
        var exitCode = await Quietly(() => LiveRun.RunAsync(options, fake));

        Assert(exitCode == 0, $"live flow had {exitCode} failed call(s); rerun with SMOKE_VERBOSE=1 to see them");
        var expected = new List<string> { "SellerListingSearch", "SellerListings", "UserSavedSearches", "GET inventory_item", "POST bulk_get_inventory_item",
            "GET offer", "GET location", "GET task", "GET task/T1/download_result_file", "GET schedule_template", "GET image/sdk-test-does-not-exist", "GET store" };
        if (write)
        {
            expected.AddRange(["PUT inventory_item/SDKTEST", "POST image/create_image_from_file", "CreateListing", "EndListing", "DELETE inventory_item/SDKTEST"]);
        }
        var seen = fake.Requests.Select(r => r.Operation).ToList();
        var missing = expected.Where(e => !seen.Any(s => s.StartsWith(e, StringComparison.Ordinal))).ToList();
        Assert(missing.Count == 0, "never called: " + string.Join(", ", missing));

        var configured = new[] { config.GraphQLBaseUrl, config.InventoryBaseUrl, config.FeedBaseUrl, config.MediaBaseUrl, config.StoresBaseUrl };
        foreach (var request in fake.Requests)
        {
            Assert(configured.Any(url => request.Uri.AbsoluteUri.StartsWith(url, StringComparison.Ordinal)),
                $"{request.Operation} went to {request.Uri}, outside the configured base URLs");
            Assert(environment == EbayEnvironment.Production || request.Uri.Host.EndsWith(".sandbox.ebay.com", StringComparison.Ordinal),
                $"{request.Operation} left the sandbox: {request.Uri.Host}");
            Assert(request.Authorization == "Bearer " + Token, $"{request.Operation} Authorization: {request.Authorization}");
        }
        var details = fake.Requests.First(r => r.Operation == "SellerListings").Body!.RootElement.GetProperty("variables").GetProperty("input").GetProperty("listings");
        Assert(details.GetArrayLength() == 2 && details[0].GetProperty("listingId").GetString() == "110000000001",
            "sellerListings did not receive the ids from sellerListingSearch: " + details);
        if (write)
        {
            var modes = fake.Requests.Where(r => r.Operation == "CreateListing")
                .Select(r => r.Body!.RootElement.GetProperty("variables").GetProperty("input").GetProperty("options").GetProperty("operationMode").GetString());
            Assert(modes.SequenceEqual(["VALIDATE", "EXECUTE"]), "createListing modes: " + string.Join(", ", modes));
            var input = fake.Requests.First(r => r.Operation == "CreateListing").Body!.RootElement.GetProperty("variables").GetProperty("input");
            if (System.Environment.GetEnvironmentVariable("SMOKE_VERBOSE") is { Length: > 0 })
            {
                Console.WriteLine("createListing input: " + input.GetRawText());
            }
            Assert(!input.GetProperty("listingTerms").GetProperty("lifecycleTerms").TryGetProperty("scheduledToStartAt", out _),
                "unset scheduledToStartAt was sent: " + input.GetProperty("listingTerms").GetRawText());
        }
    }

    private static async Task WriteRefusedInProductionAsync()
    {
        var fake = new FakeEbay();
        var exitCode = await Quietly(() => LiveRun.RunAsync(new Options(EbayEnvironment.Production, Token, 2, null, false, false, true), fake));
        Assert(exitCode != 0 && fake.Requests.Count == 0, $"--write in production: exit {exitCode}, {fake.Requests.Count} request(s)");
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

    /// <summary>Hides the inner run's output unless SMOKE_VERBOSE is set.</summary>
    private static async Task<T> Quietly<T>(Func<Task<T>> body)
    {
        if (System.Environment.GetEnvironmentVariable("SMOKE_VERBOSE") is { Length: > 0 })
        {
            return await body();
        }
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
}
