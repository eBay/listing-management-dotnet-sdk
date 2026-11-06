using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ListingManagementTestApp;

internal sealed record SeenRequest(string Operation, Uri Uri, string? Authorization, JsonDocument? Body);

/// <summary>
/// Answers every call the test app makes with eBay-shaped payloads, for the offline smoke test. Inventory items
/// written with PUT are kept, so the write round trip (create → read → delete → 404) behaves like eBay.
/// </summary>
internal sealed class FakeEbay : HttpMessageHandler
{
    private readonly ConcurrentDictionary<string, string> _items = new();

    public List<SeenRequest> Requests { get; } = [];
    public HttpStatusCode Status { get; init; } = HttpStatusCode.OK;
    public string? GraphQLOverride { get; init; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri!;
        var isJson = request.Content?.Headers.ContentType?.MediaType == "application/json";
        var text = isJson ? await request.Content!.ReadAsStringAsync(cancellationToken) : null;
        var body = string.IsNullOrEmpty(text) ? null : JsonDocument.Parse(text);
        var graphQL = uri.AbsolutePath.EndsWith("/graphql", StringComparison.Ordinal);
        // REST operations are named by method and the path after the API version, e.g. "GET offer".
        var path = string.Join('/', uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries).SkipWhile(s => !s.StartsWith('v')).Skip(1));
        var operation = graphQL
            ? body?.RootElement.TryGetProperty("operationName", out var name) == true ? name.GetString() ?? "raw" : "raw"
            : $"{request.Method} {path}";
        lock (Requests)
        {
            Requests.Add(new SeenRequest(operation, uri, request.Headers.Authorization?.ToString(), body));
        }

        if (Status != HttpStatusCode.OK)
        {
            return Respond(request, Status, "{}");
        }
        if (graphQL)
        {
            return Respond(request, HttpStatusCode.OK, GraphQLOverride ?? GraphQL(operation, body));
        }
        return Rest(request, request.Method.Method, path, text);
    }

    private HttpResponseMessage Rest(HttpRequestMessage request, string method, string path, string? body)
    {
        var segments = path.Split('/');
        switch (method, segments)
        {
            case ("GET", ["inventory_item"]):
                return Json(request, InventoryItems);
            case ("PUT", ["inventory_item", var sku]):
                _items[sku] = body ?? "{}";
                return Respond(request, HttpStatusCode.NoContent, null);
            case ("GET", ["inventory_item", var sku]):
                if (_items.TryGetValue(sku, out var stored))
                {
                    var item = JsonNode.Parse(stored)!.AsObject();
                    item["sku"] = sku;
                    return Json(request, item.ToJsonString());
                }
                return sku == "CAM-001" ? Json(request, InventoryItem) : Respond(request, HttpStatusCode.NotFound, "{}");
            case ("DELETE", ["inventory_item", var sku]):
                return Respond(request, _items.TryRemove(sku, out _) ? HttpStatusCode.NoContent : HttpStatusCode.NotFound, null);
            case ("POST", ["bulk_get_inventory_item"]):
                return Json(request, """{"responses":[{"sku":"CAM-001","statusCode":200}]}""");
            case ("GET", ["inventory_item", _, "product_compatibility"]):
                return Respond(request, HttpStatusCode.NotFound, "{}");
            case ("GET", ["offer"]):
                return Json(request, """{"total":1,"offers":[""" + Offer + "]}");
            case ("GET", ["offer", _]):
                return Json(request, Offer);
            case ("GET", ["listing", _, "sku", _, "locations"]):
                return Respond(request, HttpStatusCode.NotFound, "{}");
            case ("GET", ["location"]):
                return Json(request, """{"total":1,"locations":[{"merchantLocationKey":"WH1","location":{"address":{"city":"San Jose","country":"US"}}}]}""");
            case ("GET", ["location", _]):
                return Json(request, """{"merchantLocationKey":"WH1","name":"Main warehouse","merchantLocationStatus":"ENABLED"}""");
            case ("GET", ["task"]):
                return Json(request, """{"total":1,"tasks":[{"taskId":"T1","status":"COMPLETED","feedType":"LMS_ADD_FIXED_PRICE_ITEM"}]}""");
            case ("GET", ["task", _]):
                return Json(request, """{"taskId":"T1","status":"COMPLETED","feedType":"LMS_ADD_FIXED_PRICE_ITEM"}""");
            case ("GET", ["task", _, "download_result_file"]):
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes("sku,status\nCAM-001,OK\n")),
                };
            case ("GET", ["inventory_task" or "order_task" or "customer_service_metric_task" or "schedule"]):
                return Json(request, """{"total":0}""");
            case ("GET", ["schedule_template"]):
                return Json(request, """{"total":1,"scheduleTemplates":[{"scheduleTemplateId":"ST1","name":"Daily orders","frequency":"DAILY"}]}""");
            case ("GET", ["schedule_template", _]):
                return Json(request, """{"scheduleTemplateId":"ST1","feedType":"LMS_ORDER_REPORT","status":"ACTIVE"}""");
            case ("GET", ["image", _]):
                return Respond(request, HttpStatusCode.NotFound, "{}");
            case ("POST", ["image", "create_image_from_file"]):
                return Json(request, """{"imageUrl":"https://i.ebayimg.sandbox.ebay.com/images/g/sdk-test/s-l1600.jpg"}""");
            case ("GET", ["store"]):
                return Json(request, """{"name":"Camera Corner","url":"https://www.ebay.com/str/cameracorner"}""");
            case ("GET", ["store", "categories" or "tasks"]):
                return Json(request, "{}");
            default:
                return Respond(request, HttpStatusCode.NotFound, """{"errors":[{"message":"fake eBay has no route for this call"}]}""");
        }
    }

    private static string GraphQL(string operation, JsonDocument? body) => operation switch
    {
        "SellerListingSearch" => SellerListingSearch,
        "SellerListings" => SellerListings,
        "UserSavedSearches" => """{"data":{"userSavedSearches":{"__typename":"UserSavedSearchesOutput","savedSearches":[],"pagination":null}}}""",
        "CreateListing" when body?.RootElement.GetProperty("variables").GetProperty("input").GetProperty("options").GetProperty("operationMode").GetString() == "VALIDATE" =>
            """{"data":{"createListing":{"__typename":"CreateListingValidationSuccess","listing":{"__typename":"SellerListing","listingId":"0"},"listingFees":[{"__typename":"SellerListingFee","listingFeeType":"BOLD"}],"warnings":null}}}""",
        "CreateListing" =>
            """{"data":{"createListing":{"__typename":"CreateListingSuccess","listing":{"__typename":"SellerListing","listingId":"110000000099"},"listingFees":null,"warnings":null}}}""",
        "EndListing" =>
            """{"data":{"endListing":{"__typename":"EndListingSuccess","listing":{"__typename":"SellerListing","listingId":"110000000099"},"warnings":null}}}""",
        _ => """{"data":{"__typename":"Query"}}""",
    };

    private static HttpResponseMessage Json(HttpRequestMessage request, string json) => Respond(request, HttpStatusCode.OK, json);

    private static HttpResponseMessage Respond(HttpRequestMessage request, HttpStatusCode status, string? json) => new(status)
    {
        RequestMessage = request,
        Content = json is null ? null : new StringContent(json, Encoding.UTF8, "application/json"),
    };

    private const string Offer =
        """{"offerId":"O1","sku":"CAM-001","status":"PUBLISHED","format":"FIXED_PRICE","listing":{"listingId":"110000000001"},"pricingSummary":{"price":{"value":"19.99","currency":"USD"}}}""";

    private const string InventoryItem =
        """{"sku":"CAM-001","condition":"USED_EXCELLENT","product":{"title":"Vintage camera"},"availability":{"shipToLocationAvailability":{"quantity":3}}}""";

    private const string InventoryItems = """{"total":1,"size":1,"inventoryItems":[""" + InventoryItem + "]}";

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
}
