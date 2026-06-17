// Copyright 2026 eBay Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


using EBay.ListingManagement.Configuration;
using EBay.ListingManagement.Exceptions;
using EBay.ListingManagement.Generated.Inventory;
using EBay.ListingManagement.Generated.Inventory.Inventory_item;
using EBay.ListingManagement.Generated.Inventory.Location;
using EBay.ListingManagement.Generated.Inventory.Location.Item.Disable;
using EBay.ListingManagement.Generated.Inventory.Location.Item.Enable;
using EBay.ListingManagement.Generated.Inventory.Models;
using EBay.ListingManagement.Generated.Inventory.Offer;
using Microsoft.Kiota.Abstractions;
using System.IO;
using System.Net;

namespace EBay.ListingManagement.Clients.Rest;

public sealed class InventoryClient
{
    private readonly InventoryApiClient _kiota;
    private readonly ListingManagementConfiguration _config;
    private readonly ITokenContext _tokenContext;

    internal InventoryClient(InventoryApiClient kiota, ListingManagementConfiguration config, ITokenContext tokenContext)
        => (_kiota, _config, _tokenContext) = (kiota, config, tokenContext);

    private void ApplyHeaders(RequestHeaders headers, RequestOptions? callOptions)
    {
        foreach (var (k, v) in _config.DefaultRequestOptions.Headers)
            headers.TryAdd(k, v);
        if (callOptions is null) return;
        foreach (var (k, v) in callOptions.Headers)
            headers[k] = new[] { v };
    }

    private async Task<T> CallAsync<T>(string? accessToken, Func<Task<T>> op)
    {
        try { return await _tokenContext.RunAsync(accessToken, op); }
        catch (ApiException ex) when (ex.ResponseStatusCode == (int)HttpStatusCode.Unauthorized)
        { throw new UnauthorizedException(ex); }
    }

    private async Task CallAsync(string? accessToken, Func<Task> op)
    {
        try { await _tokenContext.RunAsync(accessToken, op); }
        catch (ApiException ex) when (ex.ResponseStatusCode == (int)HttpStatusCode.Unauthorized)
        { throw new UnauthorizedException(ex); }
    }

    // ── Inventory Items ───────────────────────────────────────────────────────

    public Task<InventoryItemWithSkuLocaleGroupid?> GetInventoryItemAsync(string sku, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Inventory_item[sku].GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<InventoryItems?> GetInventoryItemsAsync(
        Action<RequestConfiguration<Inventory_itemRequestBuilder.Inventory_itemRequestBuilderGetQueryParameters>>? configure = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Inventory_item.GetAsync(cfg =>
        {
            configure?.Invoke(cfg);
            ApplyHeaders(cfg.Headers, requestOptions);
        }, ct));

    public Task<BaseResponse?> CreateOrReplaceInventoryItemAsync(string sku, InventoryItem body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Inventory_item[sku].PutAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task DeleteInventoryItemAsync(string sku, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Inventory_item[sku].DeleteAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<BulkInventoryItemResponse?> BulkCreateOrReplaceInventoryItemAsync(BulkInventoryItem body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Bulk_create_or_replace_inventory_item.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<BulkGetInventoryItemResponse?> BulkGetInventoryItemAsync(BulkGetInventoryItem body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Bulk_get_inventory_item.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    // ── Inventory Item Groups ─────────────────────────────────────────────────

    public Task<InventoryItemGroup?> GetInventoryItemGroupAsync(string inventoryItemGroupKey, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Inventory_item_group[inventoryItemGroupKey].GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<BaseResponse?> CreateOrReplaceInventoryItemGroupAsync(string inventoryItemGroupKey, InventoryItemGroup body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Inventory_item_group[inventoryItemGroupKey].PutAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task DeleteInventoryItemGroupAsync(string inventoryItemGroupKey, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Inventory_item_group[inventoryItemGroupKey].DeleteAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    // ── Inventory Locations ───────────────────────────────────────────────────

    public Task<LocationResponse?> GetInventoryLocationsAsync(
        Action<RequestConfiguration<LocationRequestBuilder.LocationRequestBuilderGetQueryParameters>>? configure = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Location.GetAsync(cfg =>
        {
            configure?.Invoke(cfg);
            ApplyHeaders(cfg.Headers, requestOptions);
        }, ct));

    public Task<InventoryLocationResponse?> GetInventoryLocationAsync(string merchantLocationKey, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Location[merchantLocationKey].GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task CreateInventoryLocationAsync(string merchantLocationKey, InventoryLocationFull body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Location[merchantLocationKey].PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task DeleteInventoryLocationAsync(string merchantLocationKey, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Location[merchantLocationKey].DeleteAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task UpdateInventoryLocationAsync(string merchantLocationKey, InventoryLocation body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Location[merchantLocationKey].Update_location_details.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<EnablePostResponse?> EnableInventoryLocationAsync(string merchantLocationKey, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Location[merchantLocationKey].Enable.PostAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<DisablePostResponse?> DisableInventoryLocationAsync(string merchantLocationKey, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Location[merchantLocationKey].Disable.PostAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    // ── Offers ────────────────────────────────────────────────────────────────

    public Task<Offers?> GetOffersAsync(
        Action<RequestConfiguration<OfferRequestBuilder.OfferRequestBuilderGetQueryParameters>>? configure = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Offer.GetAsync(cfg =>
        {
            configure?.Invoke(cfg);
            ApplyHeaders(cfg.Headers, requestOptions);
        }, ct));

    public Task<EbayOfferDetailsWithAll?> GetOfferAsync(string offerId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Offer[offerId].GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<OfferResponse?> CreateOfferAsync(EbayOfferDetailsWithKeys body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Offer.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<OfferResponse?> UpdateOfferAsync(string offerId, EbayOfferDetailsWithId body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Offer[offerId].PutAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task DeleteOfferAsync(string offerId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Offer[offerId].DeleteAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<PublishResponse?> PublishOfferAsync(string offerId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Offer[offerId].Publish.PostAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<WithdrawResponse?> WithdrawOfferAsync(string offerId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Offer[offerId].Withdraw.PostAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<PublishResponse?> PublishOfferByInventoryItemGroupAsync(PublishByInventoryItemGroupRequest body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Offer.Publish_by_inventory_item_group.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task WithdrawOfferByInventoryItemGroupAsync(WithdrawByInventoryItemGroupRequest body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Offer.Withdraw_by_inventory_item_group.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<BulkOfferResponse?> BulkCreateOfferAsync(BulkEbayOfferDetailsWithKeys body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Bulk_create_offer.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<BulkPublishResponse?> BulkPublishOfferAsync(BulkOffer body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Bulk_publish_offer.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<BulkPriceQuantityResponse?> BulkUpdatePriceQuantityAsync(BulkPriceQuantity body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Bulk_update_price_quantity.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<FeesSummaryResponse?> GetListingFeesAsync(OfferKeysWithId body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Offer.Get_listing_fees.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<BulkMigrateListingResponse?> BulkMigrateListingAsync(BulkMigrateListing body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Bulk_migrate_listing.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    // ── Product Compatibility ─────────────────────────────────────────────────

    public Task<Compatibility?> GetProductCompatibilityAsync(string sku, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Inventory_item[sku].Product_compatibility.GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<BaseResponse?> CreateOrReplaceProductCompatibilityAsync(string sku, Compatibility body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Inventory_item[sku].Product_compatibility.PutAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task DeleteProductCompatibilityAsync(string sku, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Inventory_item[sku].Product_compatibility.DeleteAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    // ── SKU Location Mappings ─────────────────────────────────────────────────

    public Task<LocationMapping?> GetSkuLocationMappingAsync(string listingId, string sku, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Listing[listingId].Sku[sku].Locations.GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task CreateOrReplaceSkuLocationMappingAsync(string listingId, string sku, LocationMapping body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Listing[listingId].Sku[sku].Locations.PutAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task DeleteSkuLocationMappingAsync(string listingId, string sku, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Listing[listingId].Sku[sku].Locations.DeleteAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));
}
