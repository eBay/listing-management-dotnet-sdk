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
using EBay.ListingManagement.Generated.Feed;
using EBay.ListingManagement.Generated.Feed.Customer_service_metric_task;
using EBay.ListingManagement.Generated.Feed.Inventory_task;
using EBay.ListingManagement.Generated.Feed.Models;
using EBay.ListingManagement.Generated.Feed.Order_task;
using EBay.ListingManagement.Generated.Feed.Schedule;
using EBay.ListingManagement.Generated.Feed.Schedule_template;
using EBay.ListingManagement.Generated.Feed.TaskNamespace;
using Microsoft.Kiota.Abstractions;
using System.IO;
using System.Net;
using System.Net.Http;
using SchedulePostResponse = EBay.ListingManagement.Generated.Feed.Schedule.SchedulePostResponse;

namespace EBay.ListingManagement.Clients.Rest;

public sealed class FeedClient
{
    private readonly FeedApiClient _kiota;
    private readonly IRequestAdapter _adapter;
    private readonly ListingManagementConfiguration _config;
    private readonly ITokenContext _tokenContext;

    internal FeedClient(FeedApiClient kiota, IRequestAdapter adapter, ListingManagementConfiguration config, ITokenContext tokenContext)
        => (_kiota, _adapter, _config, _tokenContext) = (kiota, adapter, config, tokenContext);

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

    // ── Tasks ─────────────────────────────────────────────────────────────────

    public Task<TaskCollection?> GetTasksAsync(
        Action<RequestConfiguration<TaskRequestBuilder.TaskRequestBuilderGetQueryParameters>>? configure = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Task.GetAsync(cfg =>
        {
            configure?.Invoke(cfg);
            ApplyHeaders(cfg.Headers, requestOptions);
        }, ct));

    public Task<TaskObject?> GetTaskAsync(string taskId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Task[taskId].GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task CreateTaskAsync(CreateTaskRequest body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Task.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<Stream?> GetInputFileAsync(string taskId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Task[taskId].Download_input_file.GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<Stream?> GetResultFileAsync(string taskId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Task[taskId].Download_result_file.GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task UploadFileAsync(string taskId, Stream fileContent, string fileName, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, async () =>
        {
            using var multipart = new MultipartFormDataContent();
            multipart.Add(new StreamContent(fileContent), "file", fileName);

            var memStream = new MemoryStream();
            await multipart.CopyToAsync(memStream, ct).ConfigureAwait(false);
            memStream.Position = 0;

            var requestInfo = _kiota.Task[taskId].Upload_file.ToPostRequestInformation();
            ApplyHeaders(requestInfo.Headers, requestOptions);
            requestInfo.SetStreamContent(memStream, multipart.Headers.ContentType!.ToString());

            await _adapter.SendNoContentAsync(requestInfo, default, ct).ConfigureAwait(false);
        });

    // ── Schedules ─────────────────────────────────────────────────────────────

    public Task<UserScheduleCollection?> GetSchedulesAsync(
        Action<RequestConfiguration<ScheduleRequestBuilder.ScheduleRequestBuilderGetQueryParameters>>? configure = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Schedule.GetAsync(cfg =>
        {
            configure?.Invoke(cfg);
            ApplyHeaders(cfg.Headers, requestOptions);
        }, ct));

    public Task<UserScheduleResponse?> GetScheduleAsync(string scheduleId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Schedule[scheduleId].GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<SchedulePostResponse?> CreateScheduleAsync(CreateUserScheduleRequest body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Schedule.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task UpdateScheduleAsync(string scheduleId, UpdateUserScheduleRequest body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Schedule[scheduleId].PutAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task DeleteScheduleAsync(string scheduleId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Schedule[scheduleId].DeleteAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<Stream?> GetLatestResultFileAsync(string scheduleId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Schedule[scheduleId].Download_result_file.GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    // ── Schedule Templates ────────────────────────────────────────────────────

    public Task<ScheduleTemplateCollection?> GetScheduleTemplatesAsync(
        Action<RequestConfiguration<Schedule_templateRequestBuilder.Schedule_templateRequestBuilderGetQueryParameters>>? configure = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Schedule_template.GetAsync(cfg =>
        {
            configure?.Invoke(cfg);
            ApplyHeaders(cfg.Headers, requestOptions);
        }, ct));

    public Task<ScheduleTemplateResponse?> GetScheduleTemplateAsync(string scheduleTemplateId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Schedule_template[scheduleTemplateId].GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    // ── Customer Service Metric Tasks ─────────────────────────────────────────

    public Task<CustomerServiceMetricTaskCollection?> GetCustomerServiceMetricTasksAsync(
        Action<RequestConfiguration<Customer_service_metric_taskRequestBuilder.Customer_service_metric_taskRequestBuilderGetQueryParameters>>? configure = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Customer_service_metric_task.GetAsync(cfg =>
        {
            configure?.Invoke(cfg);
            ApplyHeaders(cfg.Headers, requestOptions);
        }, ct));

    public Task<ServiceMetricsTask?> GetCustomerServiceMetricTaskAsync(string taskId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Customer_service_metric_task[taskId].GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task CreateCustomerServiceMetricTaskAsync(CreateServiceMetricsTaskRequest body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Customer_service_metric_task.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    // ── Inventory Tasks ───────────────────────────────────────────────────────

    public Task<InventoryTaskCollection?> GetInventoryTasksAsync(
        Action<RequestConfiguration<Inventory_taskRequestBuilder.Inventory_taskRequestBuilderGetQueryParameters>>? configure = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Inventory_task.GetAsync(cfg =>
        {
            configure?.Invoke(cfg);
            ApplyHeaders(cfg.Headers, requestOptions);
        }, ct));

    public Task<InventoryTask?> GetInventoryTaskAsync(string taskId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Inventory_task[taskId].GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task CreateInventoryTaskAsync(CreateInventoryTaskRequest body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Inventory_task.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    // ── Order Tasks ───────────────────────────────────────────────────────────

    public Task<OrderTaskCollection?> GetOrderTasksAsync(
        Action<RequestConfiguration<Order_taskRequestBuilder.Order_taskRequestBuilderGetQueryParameters>>? configure = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Order_task.GetAsync(cfg =>
        {
            configure?.Invoke(cfg);
            ApplyHeaders(cfg.Headers, requestOptions);
        }, ct));

    public Task<OrderTask?> GetOrderTaskAsync(string taskId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Order_task[taskId].GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task CreateOrderTaskAsync(CreateOrderTaskRequest body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Order_task.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));
}
