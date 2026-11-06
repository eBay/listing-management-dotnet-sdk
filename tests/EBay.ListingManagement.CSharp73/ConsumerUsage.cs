using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EBay.ListingManagement;
using EBay.ListingManagement.Configuration;
using EBay.ListingManagement.Exceptions;
using EBay.ListingManagement.Generated.GraphQL;

namespace EBay.ListingManagement.CSharp73
{
    /// <summary>Typical consumer code written in C# 7.3; it only has to compile.</summary>
    public static class ConsumerUsage
    {
        public static async Task<string> RunAsync()
        {
            var config = new ListingManagementConfiguration
            {
                AccessToken = "token",
                HttpTimeout = TimeSpan.FromSeconds(60),
                DiagnosticLogging = true,
                InventoryBaseUrl = "https://ebay-proxy.example.com/sell/inventory/v1",
                AllowedHosts = { "ebay-proxy.example.com" },
                DefaultRequestOptions = new RequestOptions
                {
                    Headers = new Dictionary<string, string> { { "X-EBAY-C-MARKETPLACE-ID", "EBAY_US" } }
                }
            };

            using (var client = new ListingManagementClient(config))
            {
                try
                {
                    var items = await client.Inventory.GetInventoryItemsAsync(limit: "10");

                    var result = await client.GraphQL.EndListingAsync(
                        new EndListingInput
                        {
                            EndListingReason = EndListingReason.NotAvailable,
                            ListingIdentifier = new ListingIdentifierInput { Sku = "SKU-1" }
                        },
                        accessToken: "per-call-token");

                    var success = result as IEndListing_EndListing_EndListingSuccess;
                    if (success != null)
                    {
                        return "ended; " + (items != null ? items.Total : 0) + " inventory items";
                    }

                    var error = result as IEndListing_EndListing_EndListingValidationError;
                    return error != null ? error.ErrorMessage : result?.__typename;
                }
                catch (UnauthorizedException)
                {
                    return "unauthorized";
                }
                catch (GraphQLException ex)
                {
                    return string.Join("; ", ex.Errors);
                }
            }
        }
    }
}
