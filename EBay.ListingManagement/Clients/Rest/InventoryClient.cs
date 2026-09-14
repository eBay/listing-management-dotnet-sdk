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
using EBay.ListingManagement.Generated.Inventory.Models;
using EBay.ListingManagement.Generated.Inventory;
using Microsoft.Kiota.Abstractions;

namespace EBay.ListingManagement.Clients.Rest;

public sealed class InventoryClient
{
    private readonly InventoryApiClient _kiota;
    private readonly ListingManagementConfiguration _config;
    private readonly ITokenContext _tokenContext;
    private readonly IRequestAdapter _adapter;

    internal InventoryClient(InventoryApiClient kiota, ListingManagementConfiguration config, ITokenContext tokenContext, IRequestAdapter adapter)
    {
        _kiota = kiota;
        _config = config;
        _tokenContext = tokenContext;
        _adapter = adapter;
    }

    /// <summary>
    /// <b>Note:</b> Please note that any eBay listing created using the Inventory API cannot be revised or relisted using the Trading API calls.
    /// <b>Note:</b> Each listing can be revised up to 250 times in one calendar day. If this revision threshold is reached, the seller will be blocked from revising the item until the next calendar day.
    /// This call can be used to create and/or update up to 25 new inventory item records. It is up to sellers whether they want to create a complete inventory item records right from the start, or sellers can provide only some information with the initial <b>bulkCreateOrReplaceInventoryItem</b> call, and then make one or more additional <b>bulkCreateOrReplaceInventoryItem</b> calls to complete all required fields for the inventory item records and prepare for publishing. Upon first creating inventory item records, only the SKU values are required.
    /// Important! </b>"&gt;<b>Important!</b>Publish offer note: Fields may be optional or conditionally required when calling this method, but become required when publishing the offer to create an active listing. For this method, see <see href="/api-docs/sell/static/inventory/publishing-offers.html#inventory_item">Inventory item fields</see> for a list of fields required to publish an offer.
    /// <b>Note:</b> In addition to the <c>authorization</c> header, which is required for all eBay REST API calls, this call also requires the <c>Content-Language</c> and <c>Content-Type</c> headers.
    /// In the case of updating existing inventory item records, the <b>bulkCreateOrReplaceInventoryItem</b> call will do a complete replacement of the existing inventory item records, so all fields that are currently defined for the inventory item record are required in that update action, regardless of whether their values changed. So, when replacing/updating an inventory item record, it is advised that the seller run a 'Get' call to retrieve the full details of the inventory item records and see all of its current values/settings before attempting to update the records. Any changes that are made to inventory item records that are part of one or more active eBay listings, a successful call will automatically update these active listings.
    /// The key information that is set with the <b>bulkCreateOrReplaceInventoryItem</b> call include:
    /// - Seller-defined SKU value for the product. Each seller product, including products within an item inventory group, must have their own SKU value.
    /// - Condition of the item
    /// - Product details, including any product identifier(s), such as a UPC, ISBN, EAN, or Brand/Manufacturer Part Number pair, a product description, a product title, product/item aspects, and links to images. eBay will use any supplied eBay Product ID (ePID) or a GTIN (UPC, ISBN, or EAN) and attempt to match those identifiers to a product in the eBay Catalog, and if a product match is found, the product details for the inventory item will automatically be populated.
    /// - Quantity of the inventory item that is available for purchase
    /// - Package weight and dimensions, which is required if the seller will be offering calculated shipping options. The package weight will also be required if the seller will be providing flat-rate shipping services, but charging a weight surcharge.
    /// For those who prefer to create or update a single inventory item record, the <see href="/develop/api/sell/inventory_api#sell-inventory_api-inventory_item-createorreplaceinventoryitem">createOrReplaceInventoryItem</see> method can be used.
    /// </summary>
    public Task<BulkInventoryItemResponse?> BulkCreateOrReplaceInventoryItemAsync(
        BulkInventoryItem body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Bulk_create_or_replace_inventory_item.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call retrieves up to 25 inventory item records. The SKU value of each inventory item record to retrieve is specified in the request payload.
    /// <b>Note:</b> In addition to the <c>authorization</c> header, which is required for all Inventory API calls, this call also requires the <c>Content-Type</c> header.
    /// For those who prefer to retrieve only one inventory item record by SKU value, the <see href="/develop/api/sell/inventory_api#sell-inventory_api-inventory_item-getinventoryitem">getInventoryItem</see> method can be used. To retrieve all inventory item records defined on the seller's account, the <see href="/develop/api/sell/inventory_api#sell-inventory_api-inventory_item-getinventoryitems">getInventoryItems</see> method can be used (with pagination control if desired).
    /// </summary>
    public Task<BulkGetInventoryItemResponse?> BulkGetInventoryItemAsync(
        BulkGetInventoryItem body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Bulk_get_inventory_item.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call is used by the seller to update the total ship-to-home quantity of one inventory item, and/or to update the price and/or quantity of one or more offers associated with one inventory item. Up to 25 offers associated with an inventory item may be updated with one <b>bulkUpdatePriceQuantity</b> call. Only one SKU (one product) can be updated per call.
    /// <b>Note:</b> Each listing can be revised up to 250 times in one calendar day. If this revision threshold is reached, the seller will be blocked from revising the item until the next calendar day.
    /// <b>Note:</b> In addition to the <c>authorization</c> header, which is required for all Inventory API calls, this call also requires the <c>Content-Type</c> header.
    /// The <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-getoffers">getOffers</see> call can be used to retrieve all offers associated with a SKU. The seller will just pass in the correct SKU value through the <b>sku</b> query parameter. To update an offer, the <b>offerId</b> value is required, and this value is returned in the <b>getOffers</b> call response. It is also useful to know which offers are unpublished and which ones are published. To get this status, look for the <b>status</b> value in the <b>getOffers</b> call response. Offers in the published state are live eBay listings, and these listings will be revised with a successful <b>bulkUpdatePriceQuantity</b> call.
    /// An issue will occur if duplicate <b>offerId</b> values are passed through the same <b>offers</b> container, or if one or more of the specified offers are associated with different products/SKUs.
    /// <b>Note:</b> For multiple-variation listings, it is recommended that the <see href="/develop/api/sell/inventory_api#sell-inventory_api-inventory_item-bulkupdatepricequantity">bulkUpdatePriceQuantity</see> call be used to update price and quantity information for each SKU within that multiple-variation listing instead of using <see href="/develop/api/sell/inventory_api#sell-inventory_api-inventory_item-createorreplaceinventoryitem">createOrReplaceInventoryItem</see> calls to update the price and quantity for each SKU. Just remember that only one SKU (one product variation) can be updated per call.
    /// </summary>
    public Task<BulkPriceQuantityResponse?> BulkUpdatePriceQuantityAsync(
        BulkPriceQuantity body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Bulk_update_price_quantity.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call retrieves the inventory item record for a given SKU. The SKU value is passed in at the end of the call URI. There is no request payload for this call.
    /// The <c>authorization</c> header is the only required HTTP header for this call, and it is required for all Inventory API calls. See the <b>HTTP request headers</b> section for more information.
    /// For those who prefer to retrieve numerous inventory item records by SKU value with one call (up to 25 at a time), the <b>bulkGetInventoryItem</b> method can be used. To retrieve all inventory item records defined on the seller's account, the <b>getInventoryItems</b> method can be used (with pagination control if desired).
    /// </summary>
    public Task<InventoryItemWithSkuLocaleGroupid?> GetInventoryItemAsync(
        string sku,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Inventory_item[sku].GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// <b>Note:</b> Please note that any eBay listing created using the Inventory API cannot be revised or relisted using the Trading API calls.
    /// <b>Note:</b> Each listing can be revised up to 250 times in one calendar day. If this revision threshold is reached, the seller will be blocked from revising the item until the next calendar day.
    /// This call creates a new inventory item record or replaces an existing inventory item record. It is up to sellers whether they want to create a complete inventory item record right from the start, or sellers can provide only some information with the initial <b>createOrReplaceInventoryItem</b> call, and then make one or more additional <b>createOrReplaceInventoryItem</b> calls to complete all required fields for the inventory item record and prepare it for publishing. Upon first creating an inventory item record, only the SKU value in the call path is required.
    /// Important! </b>"&gt;<b>Important!</b>Publish offer note: Fields may be optional or conditionally required when calling this method, but become required when publishing the offer to create an active listing. For this method, see <see href="/api-docs/sell/static/inventory/publishing-offers.html#inventory_item">Inventory item fields</see> for a list of fields required to publish an offer.
    /// <b>Note:</b> In addition to the <c>authorization</c> header, which is required for all Inventory API calls, this call also requires the <c>Content-Type</c> and <c>Content-Language</c> headers.
    /// In the case of replacing an existing inventory item record, the <b>createOrReplaceInventoryItem</b> call will do a complete replacement of the existing inventory item record, so all fields that are currently defined for the inventory item record are required in that update action, regardless of whether their values changed. So, when replacing/updating an inventory item record, it is advised that the seller run a <see href="/develop/api/sell/inventory_api#sell-inventory_api-inventory_item-getinventoryitem">getInventoryItem</see> call to retrieve the full inventory item record and see all of its current values/settings before attempting to update the record. And if changes are made to an inventory item that is part of one or more active eBay listings, a successful call will automatically update these eBay listings.
    /// The key information that is set with the <b>createOrReplaceInventoryItem</b> call include:
    /// - The seller-defined SKU value for the product. Each seller product, including products within an item inventory group, must have their own SKU value. This SKU value is passed in at the end of the call URI.
    /// - The condition of the item.
    /// - Product details, including any product identifier(s), such as a UPC, ISBN, EAN, or Brand/Manufacturer Part Number pair, a product description, a product title, product/item aspects, and links to images. eBay will use any supplied eBay Product ID (ePID) or a GTIN (UPC, ISBN, or EAN) and attempt to match those identifiers to a product in the eBay Catalog, and if a product match is found, the product details for the inventory item will automatically be populated.
    /// - The quantity of the inventory item that is available for purchase.
    /// - Package weight and dimensions, which is required if the seller will be offering calculated shipping options. The package weight will also be required if the seller will be providing flat-rate shipping services, but charging a weight surcharge.
    /// In addition to the <c>authorization</c> header, which is required for all eBay REST API calls, the <b>createOrReplaceInventoryItem</b> call also requires the <c>Content-Language</c> header, that sets the natural language that will be used in the field values of the request payload. For US English, the code value passed in this header should be <c>en-US</c>. To view other supported <c>Content-Language</c> values, and to read more about all supported HTTP headers for eBay REST API calls, see the <see href="/api-docs/static/rest-request-components.html#HTTP">HTTP request headers</see> topic in the <b>Using eBay RESTful APIs</b> document.
    /// For those who prefer to create or update numerous inventory item records with one call (up to 25 at a time), the <see href="/develop/api/sell/inventory_api#sell-inventory_api-inventory_item-bulkcreateorreplaceinventoryitem">bulkCreateOrReplaceInventoryItem</see> method can be used.
    /// </summary>
    public Task<BaseResponse?> CreateOrReplaceInventoryItemAsync(
        string sku,
        InventoryItem body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Inventory_item[sku].PutAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call is used to delete an inventory item record associated with a specified SKU. A successful call will not only delete that inventory item record, but will also have the following effects:
    /// - Delete any and all unpublished offers associated with that SKU;
    /// - Delete any and all single-variation eBay listings associated with that SKU;
    /// - Automatically remove that SKU from a multiple-variation listing and remove that SKU from any and all inventory item groups in which that SKU was a member.
    /// The <c>authorization</c> header is the only required HTTP header for this call. See the <b>HTTP request headers</b> section for more information.
    /// </summary>
    public Task DeleteInventoryItemAsync(
        string sku,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Inventory_item[sku].DeleteAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call retrieves all inventory item records defined for the seller's account. The <b>limit</b> query parameter allows the seller to control how many records are returned per page, and the <b>offset</b> query parameter is used to retrieve a specific page of records. The seller can make multiple calls to scan through multiple pages of records. There is no request payload for this call.
    /// The <c>authorization</c> header is the only required HTTP header for this call, and it is required for all Inventory API calls. See the <b>HTTP request headers</b> section for more information.
    /// For those who prefer to retrieve numerous inventory item records by SKU value with one call (up to 25 at a time), the <b>bulkGetInventoryItem</b> method can be used.
    /// </summary>
    public Task<InventoryItems?> GetInventoryItemsAsync(
        string? limit = null,
        string? offset = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Inventory_item.GetAsync(cfg =>
        {
            if (limit is not null)
            {
                cfg.QueryParameters.Limit = limit;
            }
            if (offset is not null)
            {
                cfg.QueryParameters.Offset = offset;
            }
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call is used by the seller to retrieve the list of products that are compatible with the inventory item. The SKU value for the inventory item is passed into the call URI, and a successful call with return the compatible vehicle list associated with this inventory item. Product compatibility is currently only applicable to motor vehicle parts and accessory categories, but more categories may be supported in the future.
    /// </summary>
    public Task<Compatibility?> GetProductCompatibilityAsync(
        string sku,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Inventory_item[sku].Product_compatibility.GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call is used by the seller to create or replace a list of products that are compatible with the inventory item. The inventory item is identified with a SKU value in the URI. Product compatibility is currently only applicable to motor vehicle parts and accessory categories, but more categories may be supported in the future.
    /// <b>Note:</b> In addition to the <c>authorization</c> header, which is required for all Inventory API calls, this call also requires the <c>Content-Type</c> and <c>Content-Language</c> headers.
    /// </summary>
    public Task<BaseResponse?> CreateOrReplaceProductCompatibilityAsync(
        string sku,
        Compatibility body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Inventory_item[sku].Product_compatibility.PutAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call is used by the seller to delete the list of products that are compatible with the inventory item that is associated with the compatible product list. The inventory item is identified with a SKU value in the URI. Product compatibility is currently only applicable to motor vehicle parts and accessory categories, but more categories may be supported in the future.
    /// </summary>
    public Task DeleteProductCompatibilityAsync(
        string sku,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Inventory_item[sku].Product_compatibility.DeleteAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call retrieves the inventory item group for a given <b>inventoryItemGroupKey</b> value. The <b>inventoryItemGroupKey</b> value is passed in at the end of the call URI.
    /// </summary>
    public Task<InventoryItemGroup?> GetInventoryItemGroupAsync(
        string inventoryItemGroupKey,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Inventory_item_group[inventoryItemGroupKey].GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// <b>Note:</b> Each listing can be revised up to 250 times in one calendar day. If this revision threshold is reached, the seller will be blocked from revising the item until the next calendar day.
    /// This call creates a new inventory item group or updates an existing inventory item group. It is up to sellers whether they want to create a complete inventory item group record right from the start, or sellers can provide only some information with the initial <b>createOrReplaceInventoryItemGroup</b> call, and then make one or more additional <b>createOrReplaceInventoryItemGroup</b> calls to complete the inventory item group record. Upon first creating an inventory item group record, the only required elements are the <see href="/develop/api/sell/inventory_api#sell-inventory_api-inventory_item_group-createorreplaceinventoryitemgroup.inventoryitemgroup.subtitle">inventoryItemGroupKey</see> identifier in the call URI, and the members of the inventory item group specified through the <see href="/develop/api/sell/inventory_api#sell-inventory_api-inventory_item_group-createorreplaceinventoryitemgroup.inventoryitemgroup.variantskus">variantSKUs</see> array in the request payload.
    /// Important! </b>"&gt;<b>Important!</b>Publish offer note: Fields may be optional or conditionally required when calling this method, but become required when publishing the offer to create an active listing. For this method, see <see href="/api-docs/sell/static/inventory/publishing-offers.html#inventory_item_group">Inventory item group fields</see> for a list of fields required to publish an offer.
    /// <b>Note:</b> In addition to the <c>authorization</c> header, which is required for all Inventory API calls, this call also requires the <c>Content-Type</c> and <c>Content-Language</c> headers.
    /// In the case of updating/replacing an existing inventory item group, this call does a complete replacement of the existing inventory item group record, so all fields (including the member SKUs) that make up the inventory item group are required, regardless of whether their values changed. So, when replacing/updating an inventory item group record, it is advised that the seller run a <see href="/develop/api/sell/inventory_api#sell-inventory_api-inventory_item_group-getinventoryitemgroup">getInventoryItemGroup</see> call for that inventory item group to see all of its current values/settings/members before attempting to update the record. And if changes are made to an inventory item group that is part of a live, multiple-variation eBay listing, these changes automatically update the eBay listing. For example, if a SKU value is removed from the inventory item group, the corresponding product variation will be removed from the eBay listing as well.
    /// In addition to the required inventory item group identifier and member SKUs, other key information that is set with this call include:
    /// - Title and description of the inventory item group. The string values provided in these fields will actually become the listing title and listing description of the listing once the first SKU of the inventory item group is published successfully
    /// - Common aspects that inventory items in the group share
    /// - Product aspects that vary within each product variation
    /// - Links to images demonstrating the variations of the product, and these images should correspond to the product aspect that is set with the <see href="/develop/api/sell/inventory_api#sell-inventory_api-listing-bulkmigratelisting">variesBy.aspectsImageVariesBy</see> field
    /// <b>Note:</b> For more information, see <see href="/api-docs/sell/static/inventory/inventory-item-groups.html">Creating and managing inventory item groups</see>.
    /// </summary>
    public Task<BaseResponse?> CreateOrReplaceInventoryItemGroupAsync(
        string inventoryItemGroupKey,
        InventoryItemGroup body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Inventory_item_group[inventoryItemGroupKey].PutAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call deletes the inventory item group for a given <b>inventoryItemGroupKey</b> value.
    /// </summary>
    public Task DeleteInventoryItemGroupAsync(
        string inventoryItemGroupKey,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Inventory_item_group[inventoryItemGroupKey].DeleteAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call is used to convert existing eBay Listings to the corresponding Inventory API objects. If an eBay listing is successfully migrated to the Inventory API model, new Inventory Location, Inventory Item, and Offer objects are created. For a multiple-variation listing that is successfully migrated, in addition to the three new Inventory API objects just mentioned, an Inventory Item Group object will also be created. If the eBay listing is a motor vehicle part or accessory listing with a compatible vehicle list (<b>ItemCompatibilityList</b> container in Trading API's Add/Revise/Relist/Verify calls), a Product Compatibility object will be created.
    /// Migration Requirements
    /// To be eligible for migration, the active eBay listings must meet the following requirements:
    /// - Listing type is Fixed-Price
    /// <b>Note:</b> Auction listings are supported by the Inventory API, but the <b>bulkMigrateListing</b> method cannot be used to migrate auction listings.
    /// - The item(s) in the listings must have seller-defined SKU values associated with them, and in the case of a multiple-variation listing, each product variation must also have its own SKU value
    /// - Business Polices (Payment, Return Policy, and Shipping) must be used on the listing, as legacy payment, return policy, and shipping fields will not be accepted. With the Payment Policy associated with a listing, the immediate payment requirement must be enabled.
    /// - The postal/zip code (<b>PostalCode</b> field in Trading's <b>ItemType</b>) or city (<b>Location</b> field in Trading's <b>ItemType</b>) must be set in the listing; the country is also needed, but this value is required in Trading API, so it will always be set for every listing
    /// Unsupported Listing Features
    /// The following features are not yet available to be set or modified through the Inventory API, but they will remain on the active eBay listing, even after a successful migration to the Inventory model. The downside to this is that the seller will be completely blocked (in APIs or My eBay) from revising these features/settings once the migration takes place:
    /// - Any listing-level Buyer Requirements
    /// - Listing enhancements like a bold listing title or Gallery Plus
    /// Making the Call
    /// In the request payload of the <b>bulkMigrateListings</b> call, the seller will pass in an array of one to five eBay listing IDs (aka Item IDs). To save time and hassle, that seller should do a pre-check on each listing to make sure those listings meet the requirements to be migrated to the new Inventory model. This method also requires the <c>Content-Type</c> request header. There are no path or query parameters for this call.
    /// Call Response
    /// If an eBay listing is migrated successfully to the new Inventory model, the following will occur:
    /// - An Inventory Item object will be created for the item(s) in the listing, and this object will be accessible through the Inventory API
    /// - An Offer object will be created for the listing, and this object will be accessible through the Inventory API
    /// - An Inventory Location object will be created and associated with the Offer object, as an Inventory Location must be associated with a published Offer
    /// The response payload of the Bulk Migrate Listings call will show the results of each listing migration. These results include an HTTP status code to indicate the success or failure of each listing migration, the SKU value associated with each item, and if the migration is successful, an Offer ID value. The SKU value will be used in the Inventory API to manage the Inventory Item object, and the Offer ID value will be used in the Inventory API to manage the Offer object. Errors and/or warnings containers will be returned for each listing where an error and/or warning occurred with the attempted migration.
    /// If a multiple-variation listing is successfully migrated, along with the Offer and Inventory Location objects, an Inventory Item object will be created for each product variation within the listing, and an Inventory Item Group object will also be created, grouping those variations together in the Inventory API platform. For a motor vehicle part or accessory listing that has a specified list of compatible vehicles, in addition to the Inventory Item, Inventory Location, and Offer objects that are created, a Product Compatibility object will also be created in the Inventory API platform.
    /// </summary>
    public Task<BulkMigrateListingResponse?> BulkMigrateListingAsync(
        BulkMigrateListing body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Bulk_migrate_listing.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method allows sellers to retrieve the locations mapped to a specific SKU within a listing.
    /// The <b>listingId</b> and <b>sku</b> of the listing are passed in as path parameters. This method only retrieves location mappings for a single SKU value; if a seller wishes to retrieve the location mappings for all items in a multiple-variation listing, this method must be called for each variation in the listing.
    /// If there are fulfillment center locations mapped to the SKU, they will be returned in the <b>locations</b> array. If no locations are mapped to the SKU, status code <b>404 Not Found</b> will be returned.
    /// </summary>
    public Task<LocationMapping?> GetSkuLocationMappingAsync(
        string listingId,
        string sku,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Listing[listingId].Sku[sku].Locations.GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method allows sellers to map multiple fulfillment center locations to single-SKU listing, or to a single SKU within a multiple-variation listing. This allows eBay to leverage the location metadata associated with a seller's fulfillment centers to calculate more accurate estimated delivery dates on their listing.
    /// <b>Note:</b> While location mappings can be created for listings on any eBay marketplace, the improved delivery date estimate feature is currently only supported for US-based fulfillment centers shipping domestically within the US.
    /// The listing for which the locations will be mapped is specified through the <b>listingId</b> and <b>sku</b> values associated with the item. Note that only a single SKU value can be identified; if the seller wishes to map locations to multiple/all SKU values in a multiple-variation listing, this method must be called for each of those SKUs within the listing.
    /// <b>Note:</b> Sellers should keep track of <b>listingId</b>/<b>sku</b> pairs that have been used for location mapping, as there is no programmatic way to retrieve or delete these pairs at this time.
    /// In the case of replacing/updating existing location mappings, this method will do a complete replacement of the location mappings associated with a SKU. This means that each existing location mappings that the seller wants to continue to associate with the SKU are required in the update call, regardless of if they are affected by the update.
    /// This method is only supported for inventory locations that have <c>FULFILLMENT_CENTER</c> as one of their <b>locationTypes</b>. For more information on fulfillment center locations, see <see href="/api-docs/sell/static/inventory/multi-warehouse-program.html#create-location">Create a fulfillment center location</see>.
    /// For more information on location mapping features, see <see href="/api-docs/sell/static/inventory/multi-warehouse-program.html">Multi-warehouse program</see> in the Selling Integration Guide.
    /// <b>Note:</b> Only listings with SKU values are supported. Sellers using listings creating through the Trading API can add a SKU value to their single variation listing through the <see href="/Devzone/XML/docs/Reference/eBay/AddFixedPriceItem.html#Request.Item.SKU">Item.SKU</see> field during listing creation or by using the <b>ReviseItem</b> family of calls.
    /// </summary>
    public Task CreateOrReplaceSkuLocationMappingAsync(
        string listingId,
        string sku,
        LocationMapping body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Listing[listingId].Sku[sku].Locations.PutAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method allows sellers to remove all location mappings associated with a specific SKU within a listing.
    /// The <b>listingId</b> and <b>sku</b> of the listing are passed in as path parameters.
    /// Important! </b>"&gt;<b>Important!</b> To remove all location mappings from a multiple-variation listing, this method must be used for each individual SKU in the listing.
    /// </summary>
    public Task DeleteSkuLocationMappingAsync(
        string listingId,
        string sku,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Listing[listingId].Sku[sku].Locations.DeleteAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call creates multiple offers (up to 25) for specific inventory items on a specific eBay marketplace. Although it is not a requirement for the seller to create complete offers (with all necessary details) right from the start, eBay recommends that the seller provide all necessary details with this call since there is currently no bulk operation available to update multiple offers with one call. The following fields are always required in the request payload: <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-bulkcreateoffer.ebayofferdetailswithkeys.sku">sku</see>, <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-bulkcreateoffer.ebayofferdetailswithkeys.marketplaceid">marketplaceId</see>, and (listing) <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-bulkcreateoffer.ebayofferdetailswithkeys.format">format</see>.
    /// Important! </b>"&gt;<b>Important!</b>Publish offer note: Fields may be optional or conditionally required when calling this method, but become required when publishing the offer to create an active listing. For this method, see <see href="/api-docs/sell/static/inventory/publishing-offers.html#offer">Offer fields</see> for a list of fields required to publish an offer.
    /// Other information that will be required before a offer can be published are highlighted below:
    /// - Inventory location
    /// - Offer price
    /// - Available quantity
    /// - eBay listing category
    /// - Referenced listing policy profiles to set payment, return, and fulfillment values/settings
    /// <b>Note:</b> Though the <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-bulkcreateoffer.ebayofferdetailswithkeys.includecatalogproductdetails">includeCatalogProductDetails</see> parameter is not required to be submitted in the request, the parameter defaults to <c>true</c> if omitted.
    /// <b>Note:</b> In addition to the <c>authorization</c> header, which is required for all Inventory API calls, this call also requires the <c>Content-Type</c> and <c>Content-Language</c> headers.
    /// If the call is successful, unique <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-bulkcreateoffer.offerskuresponse.offerid">offerId</see> values are returned in the response for each successfully created offer. The <b>offerId</b> value will be required for many other offer-related calls. Note that this call only stages an offer for publishing. The seller must run either the <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-publishoffer">publishOffer</see>, <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-bulkpublishoffer">bulkPublishOffer</see>, or <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-publishofferbyinventoryitemgroup">publishOfferByInventoryItemGroup</see> call to convert offer(s) into an active single- or multiple-variation listing.
    /// For those who prefer to create a single offer per call, the <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-createoffer">createOffer</see> method can be used instead.
    /// </summary>
    public Task<BulkOfferResponse?> BulkCreateOfferAsync(
        BulkEbayOfferDetailsWithKeys body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Bulk_create_offer.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// <b>Note:</b> Each listing can be revised up to 250 times in one calendar day. If this revision threshold is reached, the seller will be blocked from revising the item until the next calendar day.
    /// This call is used to convert unpublished offers (up to 25) into published offers, or live eBay listings. The unique identifier (<see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-bulkpublishoffer.offerkeywithid.offerid">offerId</see>) of each offer to publish is passed into the request payload. It is possible that some unpublished offers will be successfully created into eBay listings, but others may fail. The response payload will show the results for each <b>offerId</b> value that is passed into the request payload. The <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-bulkpublishoffer.offerresponsewithlistingid.errors">errors</see> and <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-bulkpublishoffer.offerresponsewithlistingid.warnings">warnings</see> containers will be returned for an offer that had one or more issues being published.
    /// Important! </b>"&gt;<b>Important!</b>Publish offer note: Fields may be optional or conditionally required when calling the create or update methods, but become required when publishing the offer to create active listings. For this method, see <see href="/api-docs/sell/static/inventory/publishing-offers.html#offer">Offer fields</see> for a list of fields required to publish an offer.
    /// For those who prefer to publish one offer per call, the <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-publishoffer">publishOffer</see> method can be used instead. In the case of a multiple-variation listing, the <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-publishofferbyinventoryitemgroup">publishOfferByInventoryItemGroup</see> call should be used instead, as this call will convert all unpublished offers associated with an inventory item group into a multiple-variation listing.
    /// </summary>
    public Task<BulkPublishResponse?> BulkPublishOfferAsync(
        BulkOffer body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Bulk_publish_offer.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call retrieves all existing offers for the specified SKU value. The seller has the option of limiting the offers that are retrieved to a specific eBay marketplace, or to a listing format.
    /// <b>Note:</b> At this time, the same SKU value can not be offered across multiple eBay marketplaces, so the <b>marketplace_id</b> query parameter currently does not have any practical use for this call.
    /// <b>Note:</b> The same SKU can be offered through an auction and a fixed-price listing concurrently. If this is the case, <b>getOffers</b> will return two offers. Otherwise, only one offer will be returned.
    /// The <c>authorization</c> header is the only required HTTP header for this call. See the <b>HTTP request headers</b> section for more information.
    /// </summary>
    public Task<Offers?> GetOffersAsync(
        string sku,
        string? format = null,
        string? limit = null,
        string? marketplaceId = null,
        string? offset = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Offer.GetAsync(cfg =>
        {
            cfg.QueryParameters.Sku = sku;
            if (format is not null)
            {
                cfg.QueryParameters.Format = format;
            }
            if (limit is not null)
            {
                cfg.QueryParameters.Limit = limit;
            }
            if (marketplaceId is not null)
            {
                cfg.QueryParameters.MarketplaceId = marketplaceId;
            }
            if (offset is not null)
            {
                cfg.QueryParameters.Offset = offset;
            }
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call creates an offer for a specific inventory item on a specific eBay marketplace. It is up to the sellers whether they want to create a complete offer (with all necessary details) right from the start, or sellers can provide only some information with the initial <b>createOffer</b> call, and then make one or more subsequent <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-updateoffer">updateOffer</see> calls to complete the offer and prepare to publish the offer. Upon first creating an offer, the following fields are required in the request payload: <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-createoffer.ebayofferdetailswithkeys.sku">sku</see>, <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-createoffer.ebayofferdetailswithkeys.marketplaceid">marketplaceId</see>, and (listing) <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-createoffer.ebayofferdetailswithkeys.format">format</see>.
    /// Important! </b>"&gt;<b>Important!</b>Publish offer note: Fields may be optional or conditionally required when calling this method, but become required when publishing the offer to create an active listing. For this method, see <see href="/api-docs/sell/static/inventory/publishing-offers.html#offer">Offer fields</see> for a list of fields required to publish an offer.
    /// Other information that will be required before an offer can be published are highlighted below. These settings are either set with <b>createOffer</b>, or they can be set with a subsequent <b>updateOffer</b> call:
    /// - Inventory location
    /// - Offer price
    /// - Available quantity
    /// - eBay listing category
    /// - Referenced listing policy profiles to set payment, return, and fulfillment values/settings
    /// <b>Note:</b> Though the <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-bulkcreateoffer.ebayofferdetailswithkeys.includecatalogproductdetails">includeCatalogProductDetails</see> parameter is not required to be submitted in the request, the parameter defaults to <c>true</c> if omitted.
    /// <b>Note:</b> In addition to the <c>authorization</c> header, which is required for all Inventory API calls, this call also requires the <c>Content-Type</c> and <c>Content-Language</c> headers.
    /// If the call is successful, a unique <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-bulkcreateoffer.offerskuresponse.offerid">offerId</see> value is returned in the response. This value will be required for many other offer-related calls. Note that this call only stages an offer for publishing. The seller must run the <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-publishoffer">publishOffer</see> call to convert the offer to an active eBay listing.
    /// For those who prefer to create multiple offers (up to 25 at a time) with one call, the <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-bulkcreateoffer">bulkCreateOffer</see> method can be used.
    /// </summary>
    public Task<OfferResponse?> CreateOfferAsync(
        EbayOfferDetailsWithKeys body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Offer.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call retrieves a specific published or unpublished offer. The unique identifier of the offer (<b>offerId</b>) is passed in at the end of the call URI.
    /// The <c>authorization</c> header is the only required HTTP header for this call. See the <b>HTTP request headers</b> section for more information.
    /// </summary>
    public Task<EbayOfferDetailsWithAll?> GetOfferAsync(
        string offerId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Offer[offerId].GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call updates an existing offer. An existing offer may be in published state (active eBay listing), or in an unpublished state and yet to be published with the <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-publishoffer">publishOffer</see> call. The unique identifier (<b>offerId</b>) for the offer to update is passed in at the end of the call URI.
    /// The <b>updateOffer</b> call does a complete replacement of the existing offer object, so all fields that make up the current offer object are required, regardless of whether their values changed.
    /// Important! </b>"&gt;<b>Important!</b>Publish offer note: Fields may be optional or conditionally required when calling this method, but become required when publishing the offer to create an active listing. For this method, see <see href="/api-docs/sell/static/inventory/publishing-offers.html#offer">Offer fields</see> for a list of fields required to publish an offer.
    /// Other information that is required before an unpublished offer can be published or before a published offer can be revised include:
    /// - Inventory location
    /// - Offer price
    /// - Available quantity
    /// - eBay listing category
    /// - Referenced listing policy profiles to set payment, return, and fulfillment values/settings
    /// <b>Note:</b> Though the <b>includeCatalogProductDetails</b> parameter is not required to be submitted in the request, the parameter defaults to <c>true</c> if omitted from both the <b>updateOffer</b> and the <b>createOffer</b> calls. If a value is specified in the <b>updateOffer</b> call, this value will be used.
    /// <b>Note:</b> In addition to the <c>authorization</c> header, which is required for all Inventory API calls, this call also requires the <c>Content-Type</c> and <c>Content-Language</c> headers.
    /// <b>Note:</b> Each listing can be revised up to 250 times in one calendar day. If this revision threshold is reached, the seller will be blocked from revising the item until the next calendar day.
    /// For published offers, the <b>listingDescription</b> field is also required to update the offer/eBay listing. For unpublished offers, this field is not necessarily required unless it is already set for the unpublished offer.
    /// </summary>
    public Task<OfferResponse?> UpdateOfferAsync(
        string offerId,
        EbayOfferDetailsWithId body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Offer[offerId].PutAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// If used against an unpublished offer, this call will permanently delete that offer. In the case of a published offer (or live eBay listing), a successful call will either end the single-variation listing associated with the offer, or it will remove that product variation from the eBay listing and also automatically remove that product variation from the inventory item group. In the case of a multiple-variation listing, the <b>deleteOffer</b> will not remove the product variation from the listing if that variation has one or more sales. If that product variation has one or more sales, the seller can alternately just set the available quantity of that product variation to <c>0</c>, so it is not available in the eBay search or View Item page, and then the seller can remove that product variation from the inventory item group at a later time.
    /// </summary>
    public Task DeleteOfferAsync(
        string offerId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Offer[offerId].DeleteAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call is used to retrieve the expected listing fees for up to 250 unpublished offers. An array of one or more <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-getlistingfees.offerkeywithid.offerid">&gt;offerId</see> values are passed in under the <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-getlistingfees.offerkeyswithid.offers">offers</see> container.
    /// In the response payload, all listing fees are grouped by eBay marketplace, and listing fees per offer are not shown. A <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-getlistingfees.feesummary.fees">fees</see> container will be returned for each eBay marketplace where the seller is selling the products associated with the specified offers.
    /// Errors will occur if the seller passes in <b>offerIds</b> that represent published offers, so this call should be made before the seller publishes offers with the <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-publishoffer">publishOffer</see>.
    /// </summary>
    public Task<FeesSummaryResponse?> GetListingFeesAsync(
        OfferKeysWithId body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Offer.Get_listing_fees.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// <b>Note:</b> Each listing can be revised up to 250 times in one calendar day. If this revision threshold is reached, the seller will be blocked from revising the item until the next calendar day.
    /// This call is used to convert an unpublished offer into a published offer, or live eBay listing. The unique identifier of the offer (<b>offerId</b>) is passed in at the end of the call URI.
    /// Important! </b>"&gt;<b>Important!</b>Publish offer note: Fields may be optional or conditionally required when calling the create or update methods, but become required when publishing the offer to create active listings. For this method, see <see href="/api-docs/sell/static/inventory/publishing-offers.html#offer">Offer fields</see> for a list of fields required to publish an offer.
    /// For those who prefer to publish multiple offers (up to 25 at a time) with one call, the <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-bulkpublishoffer">bulkPublishOffer</see> method can be used. In the case of a multiple-variation listing, the <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-publishofferbyinventoryitemgroup">publishOfferByInventoryItemGroup</see> call should be used instead, as this call will convert all unpublished offers associated with an inventory item group into a multiple-variation listing.
    /// </summary>
    public Task<PublishResponse?> PublishOfferAsync(
        string offerId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Offer[offerId].Publish.PostAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// <b>Note:</b> Please note that any eBay listing created using the Inventory API cannot be revised or relisted using the Trading API calls.
    /// <b>Note:</b> Each listing can be revised up to 250 times in one calendar day. If this revision threshold is reached, the seller will be blocked from revising the item until the next calendar day.
    /// This call is used to convert all unpublished offers associated with an inventory item group into an active, multiple-variation listing.
    /// Important! </b>"&gt;<b>Important!</b>Publish offer note: Fields may be optional or conditionally required when calling the create or update methods, but become required when publishing the offer to create active listings. For this method, see <see href="/api-docs/sell/static/inventory/publishing-offers.html#offer">Offer fields</see> for a list of fields required to publish an offer.
    /// The unique identifier of the inventory item group (<see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-publishofferbyinventoryitemgroup.publishbyinventoryitemgrouprequest.inventoryitemgroupkey">inventoryItemGroupKey</see>) is passed in the request payload. All inventory items and their corresponding offers in the inventory item group must be valid (meet all requirements) for the <b>publishOfferByInventoryItemGroup</b> call to be completely successful. For any inventory items in the group that are missing required data or have no corresponding offers, the <b>publishOfferByInventoryItemGroup</b> will create a new multiple-variation listing, but any inventory items with missing required data/offers will not be in the newly-created listing. If any inventory items in the group to be published have invalid data, or one or more of the inventory items have conflicting data with one another, the <b>publishOfferByInventoryItemGroup</b> call will fail. Be sure to check for any error or warning messages in the call response for any applicable information about one or more inventory items/offers having issues.
    /// </summary>
    public Task<PublishResponse?> PublishOfferByInventoryItemGroupAsync(
        PublishByInventoryItemGroupRequest body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Offer.Publish_by_inventory_item_group.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call is used to end a single-variation listing that is associated with the specified offer. This call is used in place of the <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-deleteoffer">deleteOffer</see> call if the seller only wants to end the listing associated with the offer but does not want to delete the offer object. With this call, the offer object remains, but it goes into the unpublished state, and will require a <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-publishoffer">publishOffer</see> call to relist the offer.
    /// To end a multiple-variation listing that is associated with an inventory item group, the <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-withdrawofferbyinventoryitemgroup">withdrawOfferByInventoryItemGroup</see> method can be used. This call only ends the multiple-variation listing associated with an inventory item group but does not delete the inventory item group object, nor does it delete any of the offers associated with the inventory item group, but instead all of these offers go into the unpublished state.
    /// </summary>
    public Task<WithdrawResponse?> WithdrawOfferAsync(
        string offerId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Offer[offerId].Withdraw.PostAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call is used to end a multiple-variation eBay listing that is associated with the specified inventory item group. This call only ends multiple-variation eBay listing associated with the inventory item group but does not delete the inventory item group object. Similarly, this call also does not delete any of the offers associated with the inventory item group, but instead all of these offers go into the unpublished state. If the seller wanted to relist the multiple-variation eBay listing, they could use the <see href="/develop/api/sell/inventory_api#sell-inventory_api-offer-publishofferbyinventoryitemgroup">publishOfferByInventoryItemGroup</see> method.
    /// </summary>
    public Task WithdrawOfferByInventoryItemGroupAsync(
        WithdrawByInventoryItemGroupRequest body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Offer.Withdraw_by_inventory_item_group.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call retrieves all defined details of the inventory location that is specified by the <b>merchantLocationKey</b> path parameter.
    /// A successful call will return an HTTP status value of <c>200 OK</c>.
    /// </summary>
    public Task<InventoryLocationResponse?> GetInventoryLocationAsync(
        string merchantLocationKey,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Location[merchantLocationKey].GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// Use this call to create a new inventory location. In order to create and publish an offer (and create an eBay listing), a seller must have at least one location, as every offer must be associated with at least one location.
    /// Important! </b>"&gt;<b>Important!</b>Publish offer note: Fields may be optional or conditionally required when calling this method, but become required when publishing the offer to create an active listing. For this method, see <see href="/api-docs/sell/static/inventory/publishing-offers.html#location">Location fields</see> for a list of fields required to publish an offer.
    /// Upon first creating an inventory location, only a seller-defined location identifier and a physical location is required, and once set, these values can not be changed. The unique identifier value (<see href="/develop/api/sell/inventory_api#sell-inventory_api-location-getinventorylocations.inventorylocationresponse.merchantlocationkey">merchantLocationKey</see>) is passed in at the end of the call URI. This <b>merchantLocationKey</b> value will be used in other Inventory Location calls to identify the location to perform an action against.
    /// When creating an inventory location, the <see href="/develop/api/sell/inventory_api#sell-inventory_api-location-createinventorylocation.inventorylocationfull.locationtypes">locationTypes</see> can be specified to define the function of a location. At this time, the following <b>locationTypes</b> are supported:
    /// - <b>Fulfillment center</b> locations are used by sellers selling products through the Multi-warehouse program to get improved estimated delivery dates on their listings. A full address is required when creating a fulfillment center location, as well as the <b>fulfillmentCenterSpecifications</b> of the location. For more information on using the fulfillment center location type to get improved delivery dates, see <see href="/api-docs/sell/static/inventory/multi-warehouse-program.html">Multi-warehouse program</see>.
    /// - <b>Warehouse</b> locations are used for traditional shipping. A full street address is not needed, but the <b>postalCode</b> and <b>country</b> OR <b>city</b>, <b>stateOrProvince</b>, and <b>country</b> of the location must be provided.
    /// - <b>Store</b> locations are generally used by merchants selling product through the In-Store Pickup program. A full address is required when creating a store location.
    /// Note that all inventory locations are "enabled" by default when they are created, and you must specifically disable them (by passing in a value of <c>DISABLED</c> in the <b>merchantLocationStatus</b> field) if you want them to be set to the disabled state. The seller's inventory cannot be loaded to inventory locations in the disabled state.
    /// Unless one or more errors and/or warnings occur with the call, there is no response payload for this call. A successful call will return an HTTP status value of <c>204 No Content</c>.
    /// </summary>
    public Task CreateInventoryLocationAsync(
        string merchantLocationKey,
        InventoryLocationFull body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Location[merchantLocationKey].PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call deletes the inventory location that is specified in the <b>merchantLocationKey</b> path parameter. Note that deleting a location will not affect any active eBay listings associated with the deleted location, but the seller will not be able modify the offers associated with the location once it is deleted.
    /// <b>Note:</b> Deletion is not currently supported for fulfillment center locations, as location mappings will still be retained despite the location being deleted. Instead, fulfillment center locations should be disabled using the <see href="/develop/api/sell/inventory_api#sell-inventory_api-location-disableinventorylocation">disableInventoryLocation</see> method.
    /// Unless one or more errors and/or warnings occur with the call, there is no response payload for this call. A successful call will return an HTTP status value of <c>200 OK</c>.
    /// </summary>
    public Task DeleteInventoryLocationAsync(
        string merchantLocationKey,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Location[merchantLocationKey].DeleteAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call disables the inventory location that is specified in the <b>merchantLocationKey</b> path parameter. Sellers can not load/modify inventory to disabled locations. Note that disabling a location will not affect any active eBay listings associated with the disabled location, but the seller will not be able modify the offers associated with a disabled location.
    /// A successful call will return an HTTP status value of <c>200 OK</c>.
    /// </summary>
    public Task DisableInventoryLocationAsync(
        string merchantLocationKey,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Location[merchantLocationKey].Disable.PostAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call enables a disabled inventory location that is specified in the <b>merchantLocationKey</b> path parameter. Once a disabled location is enabled, sellers can start loading/modifying inventory to that location.
    /// A successful call will return an HTTP status value of <c>200 OK</c>.
    /// </summary>
    public Task EnableInventoryLocationAsync(
        string merchantLocationKey,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Location[merchantLocationKey].Enable.PostAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This call retrieves all defined details for every inventory location associated with the seller's account. There are no required parameters for this call and no request payload. However, there are two optional query parameters, <b>limit</b> and <b>offset</b>. The <b>limit</b> query parameter sets the maximum number of locations returned on one page of data, and the <b>offset</b> query parameter specifies the page of data to return. These query parameters are discussed more in the <b>URI parameters</b> table below.
    /// The <c>authorization</c> HTTP header is the only required request header for this call.
    /// A successful call will return an HTTP status value of <c>200 OK</c>.
    /// </summary>
    public Task<LocationResponse?> GetInventoryLocationsAsync(
        string? limit = null,
        string? offset = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Location.GetAsync(cfg =>
        {
            if (limit is not null)
            {
                cfg.QueryParameters.Limit = limit;
            }
            if (offset is not null)
            {
                cfg.QueryParameters.Offset = offset;
            }
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// Use this call to update location details for an existing inventory location. Specify the inventory location you want to update using the <b>merchantLocationKey</b> path parameter.
    /// You can update the following text-based fields: <b>name</b>, <b>phone</b>, <b>timeZoneId</b>, <b>geoCoordinates</b>, <b>fulfillmentCenterSpecifications</b>, <b>locationTypes</b>, <b>locationWebUrl</b>, <b>locationInstructions</b> and <b>locationAdditionalInformation</b> any number of times for any location type.
    /// For warehouse and store inventory locations, address fields can be updated any number of times. Address fields <b>cannot</b> be updated for fulfillment center locations. However, if any address fields were omitted during the <b>createInventoryLocation</b> call, they can be added through this method.
    /// <b>Note:</b> When updating a warehouse location to a fulfillment center, sellers can update any of the address fields a single time during the same call used to make this update. After this, they can no longer be updated.
    /// For store locations, the operating hours and/or the special hours can also be updated.
    /// Whatever text is passed in for these fields in an <b>updateInventoryLocation</b> call will replace the current text strings defined for these fields.
    /// Unless one or more errors and/or warnings occurs with the call, there is no response payload for this call. A successful call will return an HTTP status value of <c>204 No Content</c>.
    /// </summary>
    public Task UpdateInventoryLocationAsync(
        string merchantLocationKey,
        InventoryLocation body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Location[merchantLocationKey].Update_location_details.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));
}
