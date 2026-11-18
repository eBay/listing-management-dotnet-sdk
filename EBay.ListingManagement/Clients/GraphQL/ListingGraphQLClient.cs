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
    private readonly GraphQLExecutor _executor;
    private readonly DynamicGraphQLClient _dynamic;
    private readonly ITokenContext _tokenContext;

    internal ListingGraphQLClient(
        GraphQLExecutor executor,
        DynamicGraphQLClient dynamic,
        ITokenContext tokenContext)
        => (_executor, _dynamic, _tokenContext) = (executor, dynamic, tokenContext);

    public Task<IBulkUpdateInventory_BulkUpdateInventory?> BulkUpdateInventoryAsync(
        BulkUpdateInventoryInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            var result = await GraphQLInvocation.ExecuteAsync(() => _executor.ExecuteAsync(
                BulkUpdateInventoryOperation.OperationName,
                BulkUpdateInventoryOperation.Document,
                writer => BulkUpdateInventoryOperation.WriteVariables(writer, input),
                BulkUpdateInventoryOperation.Parse,
                ct)).ConfigureAwait(false);
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.BulkUpdateInventory;
        });

    public Task<ICreateListing_CreateListing?> CreateListingAsync(
        CreateListingInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            var result = await GraphQLInvocation.ExecuteAsync(() => _executor.ExecuteAsync(
                CreateListingOperation.OperationName,
                CreateListingOperation.Document,
                writer => CreateListingOperation.WriteVariables(writer, input),
                CreateListingOperation.Parse,
                ct)).ConfigureAwait(false);
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.CreateListing;
        });

    public Task<ICreateSecondChanceListing_CreateSecondChanceListing?> CreateSecondChanceListingAsync(
        CreateSecondChanceListingInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            var result = await GraphQLInvocation.ExecuteAsync(() => _executor.ExecuteAsync(
                CreateSecondChanceListingOperation.OperationName,
                CreateSecondChanceListingOperation.Document,
                writer => CreateSecondChanceListingOperation.WriteVariables(writer, input),
                CreateSecondChanceListingOperation.Parse,
                ct)).ConfigureAwait(false);
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.CreateSecondChanceListing;
        });

    public Task<IEndListing_EndListing?> EndListingAsync(
        EndListingInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            var result = await GraphQLInvocation.ExecuteAsync(() => _executor.ExecuteAsync(
                EndListingOperation.OperationName,
                EndListingOperation.Document,
                writer => EndListingOperation.WriteVariables(writer, input),
                EndListingOperation.Parse,
                ct)).ConfigureAwait(false);
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.EndListing;
        });

    public Task<IListingPreviewsCreationTaskById_ListingPreviewsCreationTaskById?> ListingPreviewsCreationTaskByIdAsync(
        ListingPreviewsCreationTaskByIdInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            var result = await GraphQLInvocation.ExecuteAsync(() => _executor.ExecuteAsync(
                ListingPreviewsCreationTaskByIdOperation.OperationName,
                ListingPreviewsCreationTaskByIdOperation.Document,
                writer => ListingPreviewsCreationTaskByIdOperation.WriteVariables(writer, input),
                ListingPreviewsCreationTaskByIdOperation.Parse,
                ct)).ConfigureAwait(false);
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.ListingPreviewsCreationTaskById;
        });

    public Task<IRelistListing_RelistListing?> RelistListingAsync(
        RelistListingInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            var result = await GraphQLInvocation.ExecuteAsync(() => _executor.ExecuteAsync(
                RelistListingOperation.OperationName,
                RelistListingOperation.Document,
                writer => RelistListingOperation.WriteVariables(writer, input),
                RelistListingOperation.Parse,
                ct)).ConfigureAwait(false);
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.RelistListing;
        });

    public Task<ISellerListings_SellerListings?> SellerListingsAsync(
        SellerListingsInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            var result = await GraphQLInvocation.ExecuteAsync(() => _executor.ExecuteAsync(
                SellerListingsOperation.OperationName,
                SellerListingsOperation.Document,
                writer => SellerListingsOperation.WriteVariables(writer, input),
                SellerListingsOperation.Parse,
                ct)).ConfigureAwait(false);
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
            var result = await GraphQLInvocation.ExecuteAsync(() => _executor.ExecuteAsync(
                SellerListingSearchOperation.OperationName,
                SellerListingSearchOperation.Document,
                writer => SellerListingSearchOperation.WriteVariables(writer, activeListingsSearchInput, endedListingsSearchInput, scheduledListingsSearchInput),
                SellerListingSearchOperation.Parse,
                ct)).ConfigureAwait(false);
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.SellerListingSearch;
        });

    public Task<IStartListingPreviewsCreation_StartListingPreviewsCreation?> StartListingPreviewsCreationAsync(
        StartListingPreviewsCreationInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            var result = await GraphQLInvocation.ExecuteAsync(() => _executor.ExecuteAsync(
                StartListingPreviewsCreationOperation.OperationName,
                StartListingPreviewsCreationOperation.Document,
                writer => StartListingPreviewsCreationOperation.WriteVariables(writer, input),
                StartListingPreviewsCreationOperation.Parse,
                ct)).ConfigureAwait(false);
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.StartListingPreviewsCreation;
        });

    public Task<IUpdateListing_UpdateListing?> UpdateListingAsync(
        UpdateListingInput input,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            var result = await GraphQLInvocation.ExecuteAsync(() => _executor.ExecuteAsync(
                UpdateListingOperation.OperationName,
                UpdateListingOperation.Document,
                writer => UpdateListingOperation.WriteVariables(writer, input),
                UpdateListingOperation.Parse,
                ct)).ConfigureAwait(false);
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.UpdateListing;
        });

    public Task<IUserSavedSearches_UserSavedSearches?> UserSavedSearchesAsync(
        UserSavedSearchesInput? input = null,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            var result = await GraphQLInvocation.ExecuteAsync(() => _executor.ExecuteAsync(
                UserSavedSearchesOperation.OperationName,
                UserSavedSearchesOperation.Document,
                writer => UserSavedSearchesOperation.WriteVariables(writer, input),
                UserSavedSearchesOperation.Parse,
                ct)).ConfigureAwait(false);
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.UserSavedSearches;
        });

    public Task<IVideosByIds_VideosByIds?> VideosByIdsAsync(
        VideosByIdsInput? input = null,
        string? accessToken = null,
        CancellationToken ct = default)
        => _tokenContext.RunAsync(accessToken, async () =>
        {
            var result = await GraphQLInvocation.ExecuteAsync(() => _executor.ExecuteAsync(
                VideosByIdsOperation.OperationName,
                VideosByIdsOperation.Document,
                writer => VideosByIdsOperation.WriteVariables(writer, input),
                VideosByIdsOperation.Parse,
                ct)).ConfigureAwait(false);
            GraphQLInvocation.EnsureNoErrors(result);
            return result.Data?.VideosByIds;
        });

    public Task<JsonDocument> ExecuteRawUnsafeAsync(
        string query, object? variables = null, string? accessToken = null, CancellationToken ct = default)
        => _dynamic.ExecuteRawUnsafeAsync(query, variables, accessToken, ct);
}
