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
using EBay.ListingManagement.Generated.Stores;
using EBay.ListingManagement.Generated.Stores.Models;
using Microsoft.Kiota.Abstractions;
using System.Net;

namespace EBay.ListingManagement.Clients.Rest;

public sealed class StoresClient
{
    private readonly StoresApiClient _kiota;
    private readonly ListingManagementConfiguration _config;
    private readonly ITokenContext _tokenContext;

    internal StoresClient(StoresApiClient kiota, ListingManagementConfiguration config, ITokenContext tokenContext)
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

    // ── Store ─────────────────────────────────────────────────────────────────

    public Task<GetStoreResponseType?> GetStoreAsync(string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Store.GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    // ── Store Categories ──────────────────────────────────────────────────────

    public Task<GetStoreCategoriesResponseType?> GetStoreCategoriesAsync(string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Store.Categories.GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task AddStoreCategoryAsync(AddStoreCategoryRequestType body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Store.Categories.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task DeleteStoreCategoryAsync(string categoryId, DeleteStoreCategoryRequestType body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Store.Categories[categoryId].DeleteAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task RenameStoreCategoryAsync(string categoryId, RenameStoreCategoryRequestType body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Store.Categories[categoryId].PutAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task MoveStoreCategoryAsync(MoveStoreCategoryRequestType body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Store.Categories.Move_category.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    // ── Store Tasks ───────────────────────────────────────────────────────────

    public Task<GetStoreTasksResponseType?> GetStoreTasksAsync(string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Store.Tasks.GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<GetStoreTaskResponseType?> GetStoreTaskAsync(string taskId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Store.Tasks[taskId].GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));
}
