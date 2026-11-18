using System.Net;
using System.Text.Json;
using EBay.ListingManagement.Configuration;
using EBay.ListingManagement.Exceptions;
using EBay.ListingManagement.Generated.GraphQL;
using Xunit;

namespace EBay.ListingManagement.Tests;

public sealed class GraphQLClientTests
{
    private const string EndListingValidationError = """
        {"data":{"endListing":{"__typename":"EndListingValidationError","errorCode":"LISTING_ALREADY_ENDED",
          "errorMessage":"The listing has already ended.","violations":[
            {"__typename":"StandardListingValidationRuleViolation","violationCode":"V1",
             "violationDescription":"Already ended","inputPath":["input","listingIdentifier"],"additionalInformation":null}],
          "warnings":null}}}
        """;

    [Fact]
    public async Task Typed_operation_sends_document_and_parses_union_result()
    {
        var sdk = new TestSdk { Handler = { ResponseBody = EndListingValidationError } };

        var result = await sdk.Client.GraphQL.EndListingAsync(EndInput());

        var error = Assert.IsAssignableFrom<IEndListing_EndListing_EndListingValidationError>(result);
        Assert.Equal("LISTING_ALREADY_ENDED", error.ErrorCode);
        var violation = Assert.Single(error.Violations);
        Assert.IsAssignableFrom<IEndListing_EndListing_Violations_StandardListingValidationRuleViolation>(violation);

        var request = sdk.Handler.Request!.RootElement;
        Assert.Equal("EndListing", request.GetProperty("operationName").GetString());
        Assert.StartsWith("mutation EndListing(", request.GetProperty("query").GetString());
        var input = request.GetProperty("variables").GetProperty("input");
        Assert.Equal("END_AUCTION_AND_SELL", input.GetProperty("endListingReason").GetString());
        Assert.Equal("110000000001", input.GetProperty("listingIdentifier").GetProperty("listingId").GetString());
        Assert.Equal("Bearer config-token", sdk.Handler.Authorization);
    }

    [Fact]
    public async Task Per_call_access_token_overrides_configuration()
    {
        var sdk = new TestSdk { Handler = { ResponseBody = EndListingValidationError } };

        await sdk.Client.GraphQL.EndListingAsync(EndInput(), accessToken: "call-token");

        Assert.Equal("Bearer call-token", sdk.Handler.Authorization);
    }

    [Fact]
    public async Task Unset_input_fields_are_omitted_and_explicit_null_is_sent()
    {
        var sdk = new TestSdk { Handler = { ResponseBody = EndListingValidationError } };

        await sdk.Client.GraphQL.EndListingAsync(EndInput());
        Assert.False(Input(sdk).GetProperty("listingIdentifier").TryGetProperty("sku", out _));

        var withNullSku = EndInput();
        withNullSku.ListingIdentifier.Sku = null;
        await sdk.Client.GraphQL.EndListingAsync(withNullSku);
        Assert.Equal(JsonValueKind.Null, Input(sdk).GetProperty("listingIdentifier").GetProperty("sku").ValueKind);
    }

    [Fact]
    public async Task DateTime_variables_are_sent_in_utc_z_format()
    {
        var sdk = new TestSdk { Handler = { ResponseBody = """{"data":{"sellerListingSearch":null}}""" } };
        var instant = new DateTimeOffset(2024, 1, 2, 3, 4, 5, 6, TimeSpan.Zero);

        await Record.ExceptionAsync(() => sdk.Client.GraphQL.SellerListingSearchAsync(
            activeListingsSearchInput: new SellerActiveListingSearchInput
            {
                Filters = new SellerActiveListingSearchFilterInput
                {
                    StartAtRange = new SellerListingDateTimeRangeInput { StartAt = instant, EndAt = instant }
                }
            }));

        var range = sdk.Handler.Request!.RootElement.GetProperty("variables")
            .GetProperty("activeListings_searchInput").GetProperty("filters").GetProperty("startAtRange");
        Assert.Equal("2024-01-02T03:04:05.006Z", range.GetProperty("startAt").GetString());
    }

    [Fact]
    public async Task GraphQL_errors_throw_GraphQLException_with_every_message()
    {
        var sdk = new TestSdk
        {
            Handler = { ResponseBody = """{"errors":[{"message":"boom"},{"message":"bang"}],"data":null}""" }
        };

        var error = await Assert.ThrowsAsync<GraphQLException>(() => sdk.Client.GraphQL.EndListingAsync(EndInput()));

        Assert.Equal(["boom", "bang"], error.Errors);
    }

    [Fact]
    public async Task Http_401_throws_UnauthorizedException()
    {
        var sdk = new TestSdk { Handler = { Status = HttpStatusCode.Unauthorized } };

        await Assert.ThrowsAsync<UnauthorizedException>(() => sdk.Client.GraphQL.EndListingAsync(EndInput()));
    }

    [Fact]
    public async Task Http_500_throws_SdkException_with_status()
    {
        var sdk = new TestSdk { Handler = { Status = HttpStatusCode.InternalServerError } };

        var error = await Assert.ThrowsAsync<SdkException>(() => sdk.Client.GraphQL.EndListingAsync(EndInput()));

        Assert.Contains("HTTP 500", error.Message);
    }

    [Fact]
    public async Task Raw_queries_map_401_to_UnauthorizedException()
    {
        var sdk = new TestSdk { Handler = { Status = HttpStatusCode.Unauthorized } };

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            sdk.Client.GraphQL.ExecuteRawUnsafeAsync("query { __typename }"));
    }

    [Fact]
    public void Standalone_client_exposes_typed_graphql_operations()
    {
        // Before the in-house codegen, typed operations required DI and threw NotSupportedException.
        using var client = new ListingManagementClient(new ListingManagementConfiguration { AccessToken = "t" });

        Assert.NotNull(client.GraphQL);
    }

    private static EndListingInput EndInput() => new()
    {
        EndListingReason = EndListingReason.EndAuctionAndSell,
        ListingIdentifier = new ListingIdentifierInput { ListingId = "110000000001" }
    };

    private static JsonElement Input(TestSdk sdk) =>
        sdk.Handler.Request!.RootElement.GetProperty("variables").GetProperty("input");
}
