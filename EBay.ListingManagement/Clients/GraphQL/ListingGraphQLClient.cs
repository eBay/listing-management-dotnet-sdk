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
using EBay.ListingManagement.Generated.GraphQL;
using StrawberryShake;
using System.Net;
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

    // ── Inventory ─────────────────────────────────────────────────────────────

    public Task<IBulkUpdateInventory_BulkUpdateInventory?> BulkUpdateInventoryAsync(
        BulkUpdateInventoryInput input, string? accessToken = null, CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await ExecuteAsync(() => _client!.BulkUpdateInventory.ExecuteAsync(input, ct));
            EnsureNoErrors(result);
            return result.Data?.BulkUpdateInventory;
        });

    // ── Listings ──────────────────────────────────────────────────────────────

    public Task<ICreateListing_CreateListing?> CreateListingAsync(
        CreateListingInput input, string? accessToken = null, CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await ExecuteAsync(() => _client!.CreateListing.ExecuteAsync(input, ct));
            EnsureNoErrors(result);
            return result.Data?.CreateListing;
        });

    public Task<ICreateSecondChanceListing_CreateSecondChanceListing?> CreateSecondChanceListingAsync(
        CreateSecondChanceListingInput input, string? accessToken = null, CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await ExecuteAsync(() => _client!.CreateSecondChanceListing.ExecuteAsync(input, ct));
            EnsureNoErrors(result);
            return result.Data?.CreateSecondChanceListing;
        });

    public Task<IEndListing_EndListing?> EndListingAsync(
        EndListingInput input, string? accessToken = null, CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await ExecuteAsync(() => _client!.EndListing.ExecuteAsync(input, ct));
            EnsureNoErrors(result);
            return result.Data?.EndListing;
        });

    public Task<IRelistListing_RelistListing?> RelistListingAsync(
        RelistListingInput input, string? accessToken = null, CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await ExecuteAsync(() => _client!.RelistListing.ExecuteAsync(input, ct));
            EnsureNoErrors(result);
            return result.Data?.RelistListing;
        });

    public Task<IUpdateListing_UpdateListing?> UpdateListingAsync(
        UpdateListingInput input, string? accessToken = null, CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await ExecuteAsync(() => _client!.UpdateListing.ExecuteAsync(input, ct));
            EnsureNoErrors(result);
            return result.Data?.UpdateListing;
        });

    public Task<ISellerListings_SellerListings?> GetSellerListingsAsync(
        SellerListingsInput input, string? accessToken = null, CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await ExecuteAsync(() => _client!.SellerListings.ExecuteAsync(input, ct));
            EnsureNoErrors(result);
            return result.Data?.SellerListings;
        });

    public Task<ISellerListingSearch_SellerListingSearch?> GetSellerListingSearchAsync(
        SellerActiveListingSearchInput? activeListings = null,
        SellerEndedListingSearchInput? endedListings = null,
        SellerScheduledListingSearchInput? scheduledListings = null,
        string? accessToken = null, CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await ExecuteAsync(() => _client!.SellerListingSearch.ExecuteAsync(activeListings, endedListings, scheduledListings, ct));
            EnsureNoErrors(result);
            return result.Data?.SellerListingSearch;
        });

    // ── Listing Previews ──────────────────────────────────────────────────────

    public Task<IStartListingPreviewsCreation_StartListingPreviewsCreation?> StartListingPreviewsCreationAsync(
        StartListingPreviewsCreationInput input, string? accessToken = null, CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await ExecuteAsync(() => _client!.StartListingPreviewsCreation.ExecuteAsync(input, ct));
            EnsureNoErrors(result);
            return result.Data?.StartListingPreviewsCreation;
        });

    public Task<IListingPreviewsCreationTaskById_ListingPreviewsCreationTaskById?> ListingPreviewsCreationTaskByIdAsync(
        ListingPreviewsCreationTaskByIdInput input, string? accessToken = null, CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await ExecuteAsync(() => _client!.ListingPreviewsCreationTaskById.ExecuteAsync(input, ct));
            EnsureNoErrors(result);
            return result.Data?.ListingPreviewsCreationTaskById;
        });

    // ── Videos ───────────────────────────────────────────────────────────────

    public Task<IVideosByIds_VideosByIds?> GetVideosByIdsAsync(
        VideosByIdsInput? input = null, string? accessToken = null, CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            EnsureTypedClientAvailable();
            var result = await ExecuteAsync(() => _client!.VideosByIds.ExecuteAsync(input, ct));
            EnsureNoErrors(result);
            return result.Data?.VideosByIds;
        });

    // ── Dynamic escape hatch ──────────────────────────────────────────────────

    /// <inheritdoc cref="DynamicGraphQLClient.ExecuteRawUnsafeAsync"/>
    public Task<JsonDocument> ExecuteRawUnsafeAsync(
        string query, object? variables = null, string? accessToken = null, CancellationToken ct = default)
        => _dynamic.ExecuteRawUnsafeAsync(query, variables, accessToken, ct);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task<IOperationResult<T>> ExecuteAsync<T>(Func<Task<IOperationResult<T>>> call)
        where T : class
    {
        try { return await call(); }
        catch (Exception ex) when (ex is not SdkException) { throw WrapException(ex); }
    }

    private static SdkException WrapException(Exception ex) => ex switch
    {
        HttpRequestException { StatusCode: HttpStatusCode.Unauthorized } => new UnauthorizedException(ex),
        HttpRequestException { StatusCode: { } code } http =>
            new SdkException($"GraphQL request failed with HTTP {(int)code}: {http.Message}", ex),
        HttpRequestException http =>
            new SdkException($"GraphQL request failed (network error): {http.Message} — check that GraphQLBaseUrl is reachable.", ex),
        TaskCanceledException =>
            new SdkException("GraphQL request timed out — check that GraphQLBaseUrl is reachable and the network is accessible.", ex),
        InvalidOperationException { Message: var msg } when msg.Contains("execution attempt") =>
            new SdkException("GraphQL request returned no data; the endpoint may be unreachable or returned an unexpected response — check GraphQLBaseUrl.", ex),
        _ => new SdkException($"GraphQL request failed: {ex.Message}", ex)
    };

    private static void EnsureNoErrors<T>(IOperationResult<T> result)
        where T : class
    {
        if (result.Errors is { Count: > 0 })
        {
            var messages = result.Errors
                .Select(e => e.Message ?? e.Code ?? "unknown error")
                .ToArray();
            throw new GraphQLException(messages);
        }
    }
}
