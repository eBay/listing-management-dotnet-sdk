using EBay.ListingManagement.Configuration;
using EBay.ListingManagement.Generated.GraphQL;
using Inv = EBay.ListingManagement.Generated.Inventory.Models;

namespace ListingManagementTestApp;

/// <summary>
/// Sandbox-only calls that create, change or delete data. Everything is created under a fresh "SDKTEST-" SKU and
/// removed again at the end; a listing is only created if eBay first accepts the input in validate-only mode, and
/// it is ended straight away.
/// </summary>
internal sealed partial class LiveRun
{
    private async Task RunWritesAsync()
    {
        if (_options.Environment != EbayEnvironment.Sandbox)
        {
            throw new InvalidOperationException("Write calls only run in the sandbox.");
        }

        var sku = $"SDKTEST-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var title = $"SDK test item {sku}";
        var contentLanguage = new RequestOptions { Headers = { ["Content-Language"] = "en-US" } };

        await Step("Inventory", "createOrReplaceInventoryItem", async () =>
        {
            await _client.Inventory.CreateOrReplaceInventoryItemAsync(sku, new Inv.InventoryItem
            {
                Condition = Inv.ConditionEnum.NEW,
                Product = new Inv.Product { Title = title, Description = "Created by the SDK test app; deleted at the end of the run." },
                Availability = new Inv.Availability { ShipToLocationAvailability = new Inv.ShipToLocationAvailability { Quantity = 2 } },
            }, requestOptions: contentLanguage);
            return $"created {sku}";
        });
        await Step("Inventory", "getInventoryItem (round trip)", async () =>
        {
            var item = await _client.Inventory.GetInventoryItemAsync(sku);
            var quantity = item?.Availability?.ShipToLocationAvailability?.Quantity;
            return item?.Product?.Title == title && quantity == 2
                ? $"read back title and quantity {quantity}"
                : throw new InvalidOperationException($"read back \"{item?.Product?.Title}\" qty {quantity}");
        });

        string? imageUrl = null;
        await Step("Media", "createImageFromFile (multipart)", async () =>
        {
            var png = TestImage.Png(800, 800);
            using var image = new MemoryStream(png);
            var created = await _client.Media.CreateImageFromFileAsync(image, "sdk-test.png");
            imageUrl = created?.ImageUrl;
            return image.CanRead
                ? $"uploaded {png.Length} bytes → {imageUrl ?? "(no imageUrl)"}"
                : throw new InvalidOperationException("the SDK closed the caller's stream");
        });

        var listingInput = CreateListingInput(sku, title, imageUrl, OperationMode.Validate);
        var valid = false;
        await Step("GraphQL", "createListing (validate only)", async () =>
        {
            var result = await _client.GraphQL.CreateListingAsync(listingInput);
            switch (result)
            {
                case ICreateListing_CreateListing_CreateListingValidationSuccess success:
                    valid = true;
                    return "eBay accepted the input; fees: " + Join(success.ListingFees?.Select(f => f.ListingFeeType) ?? []);
                case ICreateListing_CreateListing_CreateListingValidationError error:
                    return $"parsed eBay's validation error {error.ErrorCode}: " + Join(error.Violations.Select(v => v.ViolationDescription), 2);
                default:
                    throw new InvalidOperationException($"unexpected {result?.__typename}");
            }
        });

        if (valid)
        {
            string? listingId = null;
            await Step("GraphQL", "createListing (execute)", async () =>
            {
                var result = await _client.GraphQL.CreateListingAsync(CreateListingInput(sku, title, imageUrl, OperationMode.Execute));
                listingId = (result as ICreateListing_CreateListing_CreateListingSuccess)?.Listing.ListingId
                    ?? throw new InvalidOperationException($"not created: {result?.__typename}");
                return $"listing {listingId}";
            });
            if (listingId is not null)
            {
                await Step("GraphQL", "endListing", async () =>
                {
                    var result = await _client.GraphQL.EndListingAsync(new EndListingInput
                    {
                        EndListingReason = EndListingReason.NotAvailable,
                        ListingIdentifier = new ListingIdentifierInput { ListingId = listingId },
                    });
                    return result is IEndListing_EndListing_EndListingSuccess
                        ? $"ended {listingId}"
                        : throw new InvalidOperationException($"not ended: {(result as IEndListing_EndListing_EndListingValidationError)?.ErrorMessage ?? result?.__typename}");
                });
            }
        }
        else
        {
            Skip("GraphQL", "createListing (execute)", "validation did not pass");
            await Step("GraphQL", "endListing (unknown listing)", async () =>
            {
                // Exercises the mutation and its error union without touching real data.
                var result = await _client.GraphQL.EndListingAsync(new EndListingInput
                {
                    EndListingReason = EndListingReason.NotAvailable,
                    ListingIdentifier = new ListingIdentifierInput { Sku = sku },
                });
                return result switch
                {
                    IEndListing_EndListing_EndListingValidationError error => $"parsed eBay's validation error {error.ErrorCode}: {error.ErrorMessage}",
                    _ => result?.__typename ?? "no result",
                };
            });
        }

        await Step("Inventory", "deleteInventoryItem (cleanup)", async () =>
        {
            await _client.Inventory.DeleteInventoryItemAsync(sku);
            return $"deleted {sku}";
        });
        await ExpectError("Inventory", "getInventoryItem (deleted → 404)", () => _client.Inventory.GetInventoryItemAsync(sku), 404);
    }

    private static CreateListingInput CreateListingInput(string sku, string title, string? imageUrl, OperationMode mode) => new()
    {
        IdempotencyKey = Guid.NewGuid().ToString("N"), // eBay requires 32 hex characters
        Marketplace = ListingMarketplace.EbayUs,
        Sku = sku,
        Options = new ListingOptionsInput { OperationMode = mode },
        Product = new CreateListingProductInput
        {
            Title = title,
            Description = "Created by the SDK test app.",
            Categories = new CreateListingProductCategoriesInput { PrimaryCategoryId = "88433" },
            ItemCondition = new CreateListingItemConditionInput { ConditionId = "1000" },
            ImageUrls = imageUrl is null ? null : [imageUrl],
        },
        ListingTerms = new CreateListingTermsInput
        {
            ListingFormat = ListingFormatType.FixedPrice,
            LifecycleTerms = new CreateListingLifecycleTermsInput { ListingDurationInDays = ListingDurationInDays.Gtc },
            ItemLocation = new CreateListingItemLocationInput { Country = CountryAlpha2Code.Us, PostalCode = "95125" },
        },
        ListingItems = new CreateListingItemsInput
        {
            Items =
            [
                new CreateListingItemInput
                {
                    ItemTerms = new CreateListingItemTermsInput
                    {
                        Availability = new CreateListingItemAvailabilityInput { QuantityAvailable = 2 },
                        PricingTerms = new CreateListingItemPricingTermsInput { FixedPrice = 9.99m },
                    },
                },
            ],
        },
    };
}
