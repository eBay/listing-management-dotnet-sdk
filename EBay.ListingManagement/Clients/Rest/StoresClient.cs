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
using EBay.ListingManagement.Generated.Stores.Models;
using EBay.ListingManagement.Generated.Stores;
using Microsoft.Kiota.Abstractions;

namespace EBay.ListingManagement.Clients.Rest;

public sealed class StoresClient
{
    private readonly StoresApiClient _kiota;
    private readonly ListingManagementConfiguration _config;
    private readonly ITokenContext _tokenContext;
    private readonly IRequestAdapter _adapter;

    internal StoresClient(StoresApiClient kiota, ListingManagementConfiguration config, ITokenContext tokenContext, IRequestAdapter adapter)
    {
        _kiota = kiota;
        _config = config;
        _tokenContext = tokenContext;
        _adapter = adapter;
    }

    /// <summary>
    /// This method is used to retrieve the category hierarchy for an eBay user's store.
    /// <b>Note:</b> Three levels of store categories are supported.
    /// </summary>
    public Task<GetStoreCategoriesResponseType?> GetStoreCategoriesAsync(
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Store.Categories.GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method is used to add a single new custom category to a user's eBay store through an asynchronous request. A successful call returns the <b>getStoreTask</b> URI in the Location response header. Call <b>getStoreTask</b> (or <b>getStoreTasks</b>) method to retrieve the status of the add category operation.
    /// <b>Note:</b> Three levels of store categories are supported.
    /// Important! </b>"&gt;<b>Important!</b> If you initiate a category change, you cannot make additional category changes until the previous change request has completed. Use getStoreTask (or getStoreTasks) method to get latest status of your last request.
    /// </summary>
    public Task AddStoreCategoryAsync(
        AddStoreCategoryRequestType body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Store.Categories.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method is used to rename the single category of a user's eBay store through a synchronous request.
    /// </summary>
    public Task RenameStoreCategoryAsync(
        string categoryId,
        RenameStoreCategoryRequestType body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Store.Categories[categoryId].PutAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method is used to delete one custom category of a user's eBay store through an asynchronous request. A successful call returns the <b>getStoreTask</b> URI in the Location response header. Call <b>getStoreTask</b> (or <b>getStoreTasks</b>) method to retrieve the status of the delete category operation.
    /// Important! </b>"&gt;<b>Important!</b> If you initiate a category change, you cannot make additional category changes until the previous change request has completed. Use getStoreTask (or getStoreTasks) method to get latest status of your last request.
    /// </summary>
    public Task DeleteStoreCategoryAsync(
        string categoryId,
        DeleteStoreCategoryRequestType body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Store.Categories[categoryId].DeleteAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method is used to retrieve information for an eBay user's store such as store name, store URL, and description.
    /// </summary>
    public Task<GetStoreResponseType?> GetStoreAsync(
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Store.GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method retrieves the current status of a recent store operation. The unique identifier of the task is passed in as a path parameter.
    /// </summary>
    public Task<GetStoreTaskResponseType?> GetStoreTaskAsync(
        string taskId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Store.Tasks[taskId].GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method retrieves the status of all async store tasks for a store. Every task is set as FAILED or COMPLETED once it's execution time reaches 24 hours.
    /// </summary>
    public Task<GetStoreTasksResponseType?> GetStoreTasksAsync(
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Store.Tasks.GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method is used to move an existing user's eBay store custom category through an asynchronous request. A successful call returns the <b>getStoreTask</b> URI in the Location response header. The user calls <b>getStoreTask</b> to retrieve the status of the move category operation.
    /// Important! </b>"&gt;<b>Important!</b> If you initiate a category change, you cannot make additional category changes until the previous change request has completed. Use getStoreTask (or getStoreTasks) method to get latest status of your last request.
    /// </summary>
    public Task MoveStoreCategoryAsync(
        MoveStoreCategoryRequestType body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Store.Categories.Move_category.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));
}
