using EBay.ListingManagement.Generated.GraphQL;
using Feed = EBay.ListingManagement.Generated.Feed.Models;
using Inv = EBay.ListingManagement.Generated.Inventory.Models;

namespace ListingManagementTestApp;

/// <summary>Read-only calls: GETs, GraphQL queries, and the read-only bulkGetInventoryItem POST.</summary>
internal sealed partial class LiveRun
{
    private readonly List<string> _listingIds = [];
    private readonly List<string> _skus = [];
    private readonly List<string> _videoIds = [];
    private readonly List<string> _groupKeys = [];
    private Inv.EbayOfferDetailsWithAll? _offer;

    private async Task RunReadsAsync()
    {
        await GraphQLReadsAsync();
        await InventoryReadsAsync();
        await FeedReadsAsync();
        await MediaReadsAsync();
        await StoresReadsAsync();
    }

    private async Task GraphQLReadsAsync()
    {
        var page = new PaginationInput { MaxPageSize = _options.Limit };
        await Step("GraphQL", "sellerListingSearch", async () =>
        {
            var result = await _client.GraphQL.SellerListingSearchAsync(
                new SellerActiveListingSearchInput { Pagination = page },
                new SellerEndedListingSearchInput { Pagination = page },
                new SellerScheduledListingSearchInput { Pagination = page });
            var active = Ids(result?.ActiveListings as ISellerListingSearch_SellerListingSearch_ActiveListings_SellerListings);
            var ended = Ids(result?.EndedListings as ISellerListingSearch_SellerListingSearch_EndedListings_SellerListings);
            var scheduled = Ids(result?.ScheduledListings as ISellerListingSearch_SellerListingSearch_ScheduledListings_SellerListings);
            _listingIds.AddRange(active.Concat(ended).Concat(scheduled).Distinct());
            return $"active {active.Count}, ended {ended.Count}, scheduled {scheduled.Count}: {Join(_listingIds)}";
        });

        if (_listingIds.Count == 0)
        {
            Skip("GraphQL", "sellerListings", "no listings found");
        }
        else
        {
            await Step("GraphQL", "sellerListings", async () =>
            {
                var result = await _client.GraphQL.SellerListingsAsync(new SellerListingsInput
                {
                    Listings = _listingIds.Select(id => new SellerListingInput { ListingId = id }).ToList(),
                });
                var found = result?.Listings.OfType<ISellerListings_SellerListings_Listings_ListingSuccess>()
                    .Where(s => s.Listing is not null).Select(s => s.Listing!).ToList() ?? [];
                return $"{found.Count} found: " + Join(found.Select(l =>
                    $"{l.ListingId} {(l is ISellerListings_SellerListings_Listings_Listing_VariationListing ? "variation" : "single")} " +
                    $"{l.ListingTerms?.LifecycleTerms?.ListingStatus} \"{l.SellerProduct?.Title}\""), 2);
            });
        }

        await Step("GraphQL", "userSavedSearches", async () =>
        {
            var result = await _client.GraphQL.UserSavedSearchesAsync(new UserSavedSearchesInput
            {
                Pagination = new PageInfoInput { MaxPageSize = _options.Limit },
            });
            return $"{result?.SavedSearches.Count ?? 0} saved search(es): {Join(result?.SavedSearches.Select(s => s.SavedSearchName) ?? [])}";
        });

        await Step("GraphQL", "raw query (ExecuteRawUnsafeAsync)", async () =>
        {
            using var doc = await _client.GraphQL.ExecuteRawUnsafeAsync("query { __typename }");
            return doc.RootElement.GetRawText();
        });
    }

    private async Task InventoryReadsAsync()
    {
        await Step("Inventory", "getInventoryItems", async () =>
        {
            var page = await _client.Inventory.GetInventoryItemsAsync(limit: _options.Limit.ToString());
            foreach (var item in page?.InventoryItemsProp ?? [])
            {
                if (item.Sku is { } sku) _skus.Add(sku);
                _videoIds.AddRange(item.Product?.VideoIds ?? []);
                _groupKeys.AddRange(item.InventoryItemGroupKeys ?? []);
            }
            return $"{page?.Total ?? 0} total: " + Join(page?.InventoryItemsProp?.Select(i =>
                $"{i.Sku} qty {i.Availability?.ShipToLocationAvailability?.Quantity}") ?? []);
        });

        if (_skus.Count == 0)
        {
            foreach (var name in new[] { "getInventoryItem", "bulkGetInventoryItem", "getOffers", "getProductCompatibility" })
            {
                Skip("Inventory", name, "no inventory items");
            }
        }
        else
        {
            var sku = _skus[0];
            await Step("Inventory", "getInventoryItem (per-call token)", async () =>
            {
                // Passes the token per call instead of relying on the configured fallback.
                var item = await _client.Inventory.GetInventoryItemAsync(sku, accessToken: _options.Token);
                return $"{item?.Sku}: \"{item?.Product?.Title}\", condition {item?.Condition}";
            });
            await Step("Inventory", "bulkGetInventoryItem (POST)", async () =>
            {
                var result = await _client.Inventory.BulkGetInventoryItemAsync(new Inv.BulkGetInventoryItem
                {
                    Requests = _skus.Take(25).Select(s => new Inv.GetInventoryItem { Sku = s }).ToList(),
                });
                return Join(result?.Responses?.Select(r => $"{r.Sku} {r.StatusCode}") ?? []);
            });
            await Step("Inventory", "getOffers", async () =>
            {
                var offers = await _client.Inventory.GetOffersAsync(sku);
                _offer = offers?.OffersProp?.FirstOrDefault();
                return $"{offers?.Total ?? 0} offer(s) for {sku}: " + Join(offers?.OffersProp?.Select(o =>
                    $"{o.OfferId} {o.Status} {o.PricingSummary?.Price?.Value} {o.PricingSummary?.Price?.Currency}") ?? []);
            }, 400, 404);
            await Step("Inventory", "getProductCompatibility", async () =>
            {
                var compatibility = await _client.Inventory.GetProductCompatibilityAsync(sku);
                return $"{compatibility?.CompatibleProducts?.Count ?? 0} compatible product(s)";
            }, 400, 404);
        }

        if (_offer?.OfferId is { } offerId)
        {
            await Step("Inventory", "getOffer", async () =>
            {
                var offer = await _client.Inventory.GetOfferAsync(offerId);
                return $"{offer?.OfferId} {offer?.Format} listing {offer?.Listing?.ListingId ?? "(unpublished)"}";
            });
        }
        else
        {
            Skip("Inventory", "getOffer", "no offers");
        }

        if (_offer is { Sku: { } offerSku, Listing.ListingId: { } listingId })
        {
            await Step("Inventory", "getSkuLocationMapping", async () =>
            {
                var mapping = await _client.Inventory.GetSkuLocationMappingAsync(listingId, offerSku);
                return $"{mapping?.Locations?.Count ?? 0} location(s)";
            }, 400, 404);
        }
        else
        {
            Skip("Inventory", "getSkuLocationMapping", "no published offer");
        }

        if (_groupKeys.FirstOrDefault() is { } groupKey)
        {
            await Step("Inventory", "getInventoryItemGroup", async () =>
            {
                var group = await _client.Inventory.GetInventoryItemGroupAsync(groupKey);
                return $"{group?.InventoryItemGroupKey}: \"{group?.Title}\", {group?.VariantSKUs?.Count ?? 0} variant(s)";
            });
        }
        else
        {
            Skip("Inventory", "getInventoryItemGroup", "no item groups");
        }

        string? locationKey = null;
        await Step("Inventory", "getInventoryLocations", async () =>
        {
            var locations = await _client.Inventory.GetInventoryLocationsAsync(limit: _options.Limit.ToString());
            locationKey = locations?.Locations?.FirstOrDefault()?.MerchantLocationKey;
            return $"{locations?.Total ?? 0} location(s): " + Join(locations?.Locations?.Select(l =>
                $"{l.MerchantLocationKey} ({l.Location?.Address?.City}, {l.Location?.Address?.Country})") ?? []);
        });
        if (locationKey is not null)
        {
            await Step("Inventory", "getInventoryLocation", async () =>
            {
                var location = await _client.Inventory.GetInventoryLocationAsync(locationKey);
                return $"{location?.MerchantLocationKey}: {location?.Name} {location?.MerchantLocationStatus}";
            });
        }
        else
        {
            Skip("Inventory", "getInventoryLocation", "no locations");
        }
    }

    private async Task FeedReadsAsync()
    {
        Feed.TaskObject? task = null;
        await Step("Feed", "getTasks", async () =>
        {
            var tasks = await _client.Feed.GetTasksAsync(feedType: "LMS_ADD_FIXED_PRICE_ITEM", limit: _options.Limit, lookBackDays: 30);
            task = tasks?.Tasks?.FirstOrDefault(t => t.Status is Feed.FeedStatusEnum.COMPLETED or Feed.FeedStatusEnum.COMPLETED_WITH_ERROR)
                ?? tasks?.Tasks?.FirstOrDefault();
            return $"{tasks?.Total ?? 0} task(s): " + Join(tasks?.Tasks?.Select(t => $"{t.TaskId} {t.Status}") ?? []);
        }, 400, 404);
        if (task?.TaskId is { } taskId)
        {
            await Step("Feed", "getTask", async () =>
            {
                var detail = await _client.Feed.GetTaskAsync(taskId);
                return $"{detail?.TaskId} {detail?.FeedType} {detail?.Status}";
            });
            await Step("Feed", "getResultFile", async () =>
            {
                // The generated wrapper returns no body (see README), so this only checks the request succeeds.
                await _client.Feed.GetResultFileAsync(taskId);
                return "200; file content is not exposed by the SDK";
            }, 400, 404);
        }
        else
        {
            Skip("Feed", "getTask", "no feed tasks");
            Skip("Feed", "getResultFile", "no feed tasks");
        }

        await Step("Feed", "getInventoryTasks", async () =>
        {
            var tasks = await _client.Feed.GetInventoryTasksAsync("LMS_ACTIVE_INVENTORY_REPORT", limit: _options.Limit);
            return $"{tasks?.Total ?? 0} task(s): " + Join(tasks?.Tasks?.Select(t => $"{t.TaskId} {t.Status}") ?? []);
        }, 400, 404);
        await Step("Feed", "getOrderTasks", async () =>
        {
            var tasks = await _client.Feed.GetOrderTasksAsync(feedType: "LMS_ORDER_REPORT", limit: _options.Limit, lookBackDays: 30);
            return $"{tasks?.Total ?? 0} task(s)";
        }, 400, 404);
        await Step("Feed", "getCustomerServiceMetricTasks", async () =>
        {
            var tasks = await _client.Feed.GetCustomerServiceMetricTasksAsync("CUSTOMER_SERVICE_METRICS_REPORT", limit: _options.Limit);
            return $"{tasks?.Total ?? 0} task(s)";
        }, 400, 404);

        string? templateId = null;
        await Step("Feed", "getScheduleTemplates", async () =>
        {
            var templates = await _client.Feed.GetScheduleTemplatesAsync("LMS_ORDER_REPORT");
            templateId = templates?.ScheduleTemplates?.FirstOrDefault()?.ScheduleTemplateId;
            return Join(templates?.ScheduleTemplates?.Select(t => $"{t.ScheduleTemplateId} {t.Name} ({t.Frequency})") ?? []);
        }, 400, 404);
        if (templateId is not null)
        {
            await Step("Feed", "getScheduleTemplate", async () =>
            {
                var template = await _client.Feed.GetScheduleTemplateAsync(templateId);
                return $"{template?.ScheduleTemplateId} {template?.FeedType} {template?.Status}";
            });
        }
        else
        {
            Skip("Feed", "getScheduleTemplate", "no schedule templates");
        }

        string? scheduleId = null;
        await Step("Feed", "getSchedules", async () =>
        {
            var schedules = await _client.Feed.GetSchedulesAsync("LMS_ORDER_REPORT");
            scheduleId = schedules?.Schedules?.FirstOrDefault()?.ScheduleId;
            return $"{schedules?.Total ?? 0} schedule(s)";
        }, 400, 404);
        if (scheduleId is not null)
        {
            await Step("Feed", "getSchedule", async () =>
            {
                var schedule = await _client.Feed.GetScheduleAsync(scheduleId);
                return $"{schedule?.ScheduleId} {schedule?.ScheduleName} {schedule?.Status}";
            });
        }
        else
        {
            Skip("Feed", "getSchedule", "no schedules");
        }
    }

    private async Task MediaReadsAsync()
    {
        if (_videoIds.Count > 0)
        {
            await Step("GraphQL", "videosByIds", async () =>
            {
                var result = await _client.GraphQL.VideosByIdsAsync(new VideosByIdsInput { VideoIds = _videoIds.Distinct().ToList() });
                return result switch
                {
                    IVideosByIds_VideosByIds_VideoList list => Join(list.VideoList.Select(v =>
                        v is IVideosByIds_VideosByIds_VideoList_VideoByIdSuccess ok ? $"{ok.Video.VideoId} {ok.Video.Status}" : v?.__typename)),
                    _ => result?.__typename ?? "no result",
                };
            });
            await Step("Media", "getVideo", async () =>
            {
                var video = await _client.Media.GetVideoAsync(_videoIds[0]);
                return $"{video?.VideoId} {video?.Status} \"{video?.Title}\"";
            }, 404);
        }
        else
        {
            Skip("GraphQL", "videosByIds", "no videos on inventory items");
            Skip("Media", "getVideo", "no videos on inventory items");
        }

        // An unknown id exercises the REST error path and the apim host.
        await ExpectError("Media", "getImage (unknown id → error)",
            () => _client.Media.GetImageAsync("sdk-test-does-not-exist"), 400, 404);
    }

    private async Task StoresReadsAsync()
    {
        // eBay answers 400 for accounts without a store subscription.
        await Step("Stores", "getStore", async () =>
        {
            var store = await _client.Stores.GetStoreAsync();
            return $"{store?.Name} {store?.Url}";
        }, 400, 404);
        await Step("Stores", "getStoreCategories", async () =>
        {
            var categories = await _client.Stores.GetStoreCategoriesAsync();
            return $"{categories?.StoreCategories?.Count ?? 0} top-level categories";
        }, 400, 404);
        await Step("Stores", "getStoreTasks", async () =>
        {
            var tasks = await _client.Stores.GetStoreTasksAsync();
            return $"{tasks?.Task?.Count ?? 0} task(s)";
        }, 400, 404);
    }

    private static List<string> Ids(ISellerListingSearch_SellerListingSearch_ActiveListings_SellerListings? found) =>
        found?.Listings?.Select(l => l.ListingId).ToList() ?? [];

    private static List<string> Ids(ISellerListingSearch_SellerListingSearch_EndedListings_SellerListings? found) =>
        found?.Listings?.Select(l => l.ListingId).ToList() ?? [];

    private static List<string> Ids(ISellerListingSearch_SellerListingSearch_ScheduledListings_SellerListings? found) =>
        found?.Listings?.Select(l => l.ListingId).ToList() ?? [];
}
