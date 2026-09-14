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
using EBay.ListingManagement.Generated.GraphQL;
using System.Text.Json;

namespace EBay.ListingManagement.Clients.GraphQL;

public sealed class ListingGraphQLClient
{
    private readonly IListingManagementGraphQLClient? _client;
    private readonly DynamicGraphQLClient _dynamic;
    private readonly ITokenContext _tokenContext;

    internal ListingGraphQLClient(
        IListingManagementGraphQLClient? client,
        DynamicGraphQLClient dynamic,
        ITokenContext tokenContext)
        => (_client, _dynamic, _tokenContext) = (client, dynamic, tokenContext);

    private void EnsureTypedClientAvailable()
    {
        if (_client is null)
            throw new NotSupportedException(
                "Typed GraphQL operations require DI. Use ExecuteRawUnsafeAsync for standalone mode.");
    }

    public Task<IBulkUpdateInventory_BulkUpdateInventory?> BulkUpdateInventoryAsync(
        BulkUpdateInventoryInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await GraphQLInvocation.ExecuteAsync(() => _client!.BulkUpdateInventory.ExecuteAsync(input, ct));
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.BulkUpdateInventory;
        });

    public Task<ICreateListing_CreateListing?> CreateListingAsync(
        CreateListingInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await GraphQLInvocation.ExecuteAsync(() => _client!.CreateListing.ExecuteAsync(input, ct));
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.CreateListing;
        });

    public Task<ICreateSecondChanceListing_CreateSecondChanceListing?> CreateSecondChanceListingAsync(
        CreateSecondChanceListingInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await GraphQLInvocation.ExecuteAsync(() => _client!.CreateSecondChanceListing.ExecuteAsync(input, ct));
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.CreateSecondChanceListing;
        });

    public Task<IEndListing_EndListing?> EndListingAsync(
        EndListingInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await GraphQLInvocation.ExecuteAsync(() => _client!.EndListing.ExecuteAsync(input, ct));
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.EndListing;
        });

    public Task<IListingPreviewsCreationTaskById_ListingPreviewsCreationTaskById?> ListingPreviewsCreationTaskByIdAsync(
        ListingPreviewsCreationTaskByIdInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await GraphQLInvocation.ExecuteAsync(() => _client!.ListingPreviewsCreationTaskById.ExecuteAsync(input, ct));
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.ListingPreviewsCreationTaskById;
        });

    public Task<IRelistListing_RelistListing?> RelistListingAsync(
        RelistListingInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await GraphQLInvocation.ExecuteAsync(() => _client!.RelistListing.ExecuteAsync(input, ct));
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.RelistListing;
        });

    public Task<ISellerListings_SellerListings?> SellerListingsAsync(
        SellerListingsInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await GraphQLInvocation.ExecuteAsync(() => _client!.SellerListings.ExecuteAsync(input, ct));
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.SellerListings;
        });

    public Task<ISellerListingSearch_SellerListingSearch?> SellerListingSearchAsync(
        SellerActiveListingSearchInput? activeListingsSearchInput = null,
        SellerEndedListingSearchInput? endedListingsSearchInput = null,
        SellerScheduledListingSearchInput? scheduledListingsSearchInput = null,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await GraphQLInvocation.ExecuteAsync(() => _client!.SellerListingSearch.ExecuteAsync(activeListingsSearchInput, endedListingsSearchInput, scheduledListingsSearchInput, ct));
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.SellerListingSearch;
        });

    public Task<IStartListingPreviewsCreation_StartListingPreviewsCreation?> StartListingPreviewsCreationAsync(
        StartListingPreviewsCreationInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await GraphQLInvocation.ExecuteAsync(() => _client!.StartListingPreviewsCreation.ExecuteAsync(input, ct));
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.StartListingPreviewsCreation;
        });

    public Task<IUpdateListing_UpdateListing?> UpdateListingAsync(
        UpdateListingInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await GraphQLInvocation.ExecuteAsync(() => _client!.UpdateListing.ExecuteAsync(input, ct));
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.UpdateListing;
        });

    public Task<IUserSavedSearches_UserSavedSearches?> UserSavedSearchesAsync(
        UserSavedSearchesInput? input = null,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await GraphQLInvocation.ExecuteAsync(() => _client!.UserSavedSearches.ExecuteAsync(input, ct));
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.UserSavedSearches;
        });

    public Task<IVideosByIds_VideosByIds?> VideosByIdsAsync(
        VideosByIdsInput? input = null,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await GraphQLInvocation.ExecuteAsync(() => _client!.VideosByIds.ExecuteAsync(input, ct));
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.VideosByIds;
        });

    public Task<JsonDocument> ExecuteRawUnsafeAsync(
        string query, object? variables = null, string? accessToken = null, CancellationToken ct = default)
        => _dynamic.ExecuteRawUnsafeAsync(query, variables, accessToken, ct);
}
