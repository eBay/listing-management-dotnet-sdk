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
using EBay.ListingManagement.Generated.Feed.Models;
using EBay.ListingManagement.Generated.Feed;
using Microsoft.Kiota.Abstractions;
using System.IO;

namespace EBay.ListingManagement.Clients.Rest;

public sealed class FeedClient
{
    private readonly FeedApiClient _kiota;
    private readonly ListingManagementConfiguration _config;
    private readonly ITokenContext _tokenContext;
    private readonly IRequestAdapter _adapter;

    internal FeedClient(FeedApiClient kiota, ListingManagementConfiguration config, ITokenContext tokenContext, IRequestAdapter adapter)
    {
        _kiota = kiota;
        _config = config;
        _tokenContext = tokenContext;
        _adapter = adapter;
    }

    /// <summary>
    /// This method returns the details and status for an array of order tasks. Rather than using a request payload, it does this through specified input in query parameters, such as <b>feed_type</b> or <b>schedule_id</b>. Specifying both <b>feed_type</b> and <b>schedule_id</b> results in an error. Since schedules are based on feed types, you can specify a schedule (<b>schedule_id</b>) that returns the needed <b>feed_type</b>.
    /// If specifying the <b>feed_type</b>, limit which order tasks are returned by specifying filters such as the creation date range or period of time using <b>look_back_days</b>.
    /// If specifying a <b>schedule_id</b>, the schedule template (that the <b>schedule_id</b> is based on) determines which order tasks are returned (see <b>schedule_id</b> for additional information). Each <b>schedule_id</b> applies to one <b>feed_type</b>.
    /// </summary>
    public Task<OrderTaskCollection?> GetOrderTasksAsync(
        string? dateRange = null,
        string? feedType = null,
        int? limit = null,
        int? lookBackDays = null,
        int? offset = null,
        string? scheduleId = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Order_task.GetAsync(cfg =>
        {
            if (dateRange is not null)
            {
                cfg.QueryParameters.DateRange = dateRange;
            }
            if (feedType is not null)
            {
                cfg.QueryParameters.FeedType = feedType;
            }
            if (limit is not null)
            {
                cfg.QueryParameters.Limit = limit;
            }
            if (lookBackDays is not null)
            {
                cfg.QueryParameters.LookBackDays = lookBackDays;
            }
            if (offset is not null)
            {
                cfg.QueryParameters.Offset = offset;
            }
            if (scheduleId is not null)
            {
                cfg.QueryParameters.ScheduleId = scheduleId;
            }
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method creates an order download task with filter criteria for the order report. When using this method, specify the <b>feedType</b>, <b>schemaVersion</b>, and <b>filterCriteria</b> for the report. The method returns the <b>location</b> response header containing the <b>getOrderTask</b> call URI to retrieve the order task you just created. The URL includes the eBay-assigned task ID, which you can use to reference the order task.
    /// To retrieve the status of the task, use the <b>getOrderTask</b> method to retrieve a single task ID or the <b>getOrderTasks</b> method to retrieve multiple order task IDs.
    /// <b>Note:</b> The scope depends on the feed type. An error message results when an unsupported scope or feed type is specified.
    /// The following contains this method's authorization scope and its corresponding feed type:
    /// - https://api.ebay.com/oauth/api_scope/sell.fulfillment: LMS_ORDER_REPORT
    /// For details about how this method is used, see <see href="/api-docs/sell/static/feed/general-feed-tasks.html">General feed types</see> in the Selling Integration Guide.
    /// <b>Note:</b> At this time, the <b>createOrderTask</b> method only supports order creation date filters and not modified order date filters. Do not include the <b>modifiedDateRange</b> filter in your request payload.
    /// </summary>
    public Task CreateOrderTaskAsync(
        CreateOrderTaskRequest body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Order_task.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method retrieves the task details and status of the specified task. The input is <b>task_id</b>.
    /// For details about how this method is used, see <see href="/api-docs/sell/static/orders/generating-and-retrieving-order-reports.html">Working with Order Feeds</see> in the Selling Integration Guide.
    /// </summary>
    public Task<OrderTask?> GetOrderTaskAsync(
        string taskId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Order_task[taskId].GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method searches for multiple tasks of a specific feed type and includes date filters and pagination.
    /// </summary>
    public Task<InventoryTaskCollection?> GetInventoryTasksAsync(
        string feedType,
        string? scheduleId = null,
        int? lookBackDays = null,
        string? dateRange = null,
        int? limit = null,
        int? offset = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Inventory_task.GetAsync(cfg =>
        {
            cfg.QueryParameters.FeedType = feedType;
            if (scheduleId is not null)
            {
                cfg.QueryParameters.ScheduleId = scheduleId;
            }
            if (lookBackDays is not null)
            {
                cfg.QueryParameters.LookBackDays = lookBackDays;
            }
            if (dateRange is not null)
            {
                cfg.QueryParameters.DateRange = dateRange;
            }
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
    /// This method creates an inventory-related download task for a specified feed type with optional filter criteria. When using this method, specify the <b>feedType</b>.
    /// This method returns the location response header containing the <b>getInventoryTask</b> call URI to retrieve the inventory task you just created. The URL includes the eBay-assigned task ID, which you can use to reference the inventory task.
    /// To retrieve the status of the task, use the <b>getInventoryTask</b> method to retrieve a single task ID or the <b>getInventoryTasks</b> method to retrieve multiple task IDs.
    /// <b>Note:</b> The scope depends on the feed type. An error message results when an unsupported scope or feed type is specified.
    /// Presently, this method supports Active Inventory Report. The <b>ActiveInventoryReport</b> returns a report that contains price and quantity information for all of the active listings for a specific seller. A seller can use this information to maintain their inventory on eBay.
    /// </summary>
    public Task CreateInventoryTaskAsync(
        CreateInventoryTaskRequest body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Inventory_task.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method retrieves the task details and status of the specified inventory-related task. The input is <b>task_id</b>.
    /// </summary>
    public Task<InventoryTask?> GetInventoryTaskAsync(
        string taskId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Inventory_task[taskId].GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method retrieves an array containing the details and status of all schedules based on the specified <b>feed_type</b>. Use this method to find a schedule if you do not know the <b>schedule_id</b>.
    /// </summary>
    public Task<UserScheduleCollection?> GetSchedulesAsync(
        string feedType,
        int? limit = null,
        int? offset = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Schedule.GetAsync(cfg =>
        {
            cfg.QueryParameters.FeedType = feedType;
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
    /// This method creates a schedule, which is a subscription to the specified schedule template. A schedule periodically generates a report for the <b>feedType</b> specified by the template. Specify the same <b>feedType</b> as the <b>feedType</b> of the associated schedule template. When creating the schedule, if available from the template, you can specify a preferred trigger hour, day of the week, or day of the month. These and other fields are conditionally available as specified by the template.
    /// <b>Note:</b> Make sure to include all fields required by the schedule template (<b>scheduleTemplateId</b>). Call the <b>getScheduleTemplate</b> method (or the <b>getScheduleTemplates</b> method), to find out which fields are required or optional. If a field is optional and a default value is provided by the template, the default value will be used if omitted from the payload.
    /// A successful call returns the location response header containing the <b>getSchedule</b> call URI to retrieve the schedule you just created. The URL includes the eBay-assigned schedule ID, which you can use to reference the schedule task.
    /// To retrieve the details of the create schedule task, use the <b>getSchedule</b> method for a single schedule ID or the <b>getSchedules</b> method to retrieve all schedule details for the specified <b>feed_type</b>. The number of schedules for each feedType is limited. Error code 160031 is returned when you have reached this maximum.
    /// <b>Note:</b> Except for schedules with a HALF-HOUR frequency, all schedules will ideally run at the start of each hour ('00' minutes). Actual start time may vary due to load and other factors.
    /// </summary>
    public Task CreateScheduleAsync(
        CreateUserScheduleRequest body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Schedule.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method retrieves schedule details and status of the specified schedule. Specify the schedule to retrieve using the <b>schedule_id</b>. Use the <b>getSchedules</b> method to find a schedule if you do not know the <b>schedule_id</b>.
    /// </summary>
    public Task<UserScheduleResponse?> GetScheduleAsync(
        string scheduleId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Schedule[scheduleId].GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method updates an existing schedule. Specify the schedule to update using the <b>schedule_id</b> path parameter. If the schedule template has changed after the schedule was created or updated, the input will be validated using the changed template.
    /// <b>Note:</b> Make sure to include all fields required by the schedule template (<b>scheduleTemplateId</b>). Call the <b>getScheduleTemplate</b> method (or the <b>getScheduleTemplates</b> method), to find out which fields are required or optional. If you do not know the <b>scheduleTemplateId</b>, call the <b>getSchedule</b> method to find out.
    /// </summary>
    public Task UpdateScheduleAsync(
        string scheduleId,
        UpdateUserScheduleRequest body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Schedule[scheduleId].PutAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method deletes an existing schedule. Specify the schedule to delete using the <b>schedule_id</b> path parameter.
    /// </summary>
    public Task DeleteScheduleAsync(
        string scheduleId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Schedule[scheduleId].DeleteAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method downloads the latest Order Report generated by the schedule. The response of this call is a compressed or uncompressed CSV, XML, or JSON file, with the applicable file extension (for example: csv.gz). Specify the <b>schedule_id</b> path parameter to download its last generated file.
    /// </summary>
    public Task GetLatestResultFileAsync(
        string scheduleId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Schedule[scheduleId].Download_result_file.GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method retrieves the details of the specified template. Specify the template to retrieve using the <b>schedule_template_id</b> path parameter. Use the <b>getScheduleTemplates</b> method to find a schedule template if you do not know the <b>schedule_template_id</b>.
    /// </summary>
    public Task<ScheduleTemplateResponse?> GetScheduleTemplateAsync(
        string scheduleTemplateId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Schedule_template[scheduleTemplateId].GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method retrieves an array containing the details and status of all schedule templates based on the specified <b>feed_type</b>. Use this method to find a schedule template if you do not know the <b>schedule_template_id</b>.
    /// </summary>
    public Task<ScheduleTemplateCollection?> GetScheduleTemplatesAsync(
        string feedType,
        int? limit = null,
        int? offset = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Schedule_template.GetAsync(cfg =>
        {
            cfg.QueryParameters.FeedType = feedType;
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
    /// This method returns the details and status for an array of tasks based on a specified <b>feed_type</b> or <b>schedule_id</b>. Specifying both <b>feed_type</b> and <b>schedule_id</b> results in an error. Since schedules are based on feed types, you can specify a schedule (<b>schedule_id</b>) that returns the needed <b>feed_type</b>.
    /// If specifying the <b>feed_type</b>, limit which tasks are returned by specifying filters, such as the creation date range or period of time using <b>look_back_days</b>. Also, by specifying the <b>feed_type</b>, both on-demand and scheduled reports are returned.
    /// If specifying a <b>schedule_id</b>, the schedule template (that the schedule ID is based on) determines which tasks are returned (see <b>schedule_id</b> for additional information). Each <b>scheduledId</b> applies to one <b>feed_type</b>.
    /// </summary>
    public Task<TaskCollection?> GetTasksAsync(
        string? dateRange = null,
        string? feedType = null,
        int? limit = null,
        int? lookBackDays = null,
        int? offset = null,
        string? scheduleId = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Task.GetAsync(cfg =>
        {
            if (dateRange is not null)
            {
                cfg.QueryParameters.DateRange = dateRange;
            }
            if (feedType is not null)
            {
                cfg.QueryParameters.FeedType = feedType;
            }
            if (limit is not null)
            {
                cfg.QueryParameters.Limit = limit;
            }
            if (lookBackDays is not null)
            {
                cfg.QueryParameters.LookBackDays = lookBackDays;
            }
            if (offset is not null)
            {
                cfg.QueryParameters.Offset = offset;
            }
            if (scheduleId is not null)
            {
                cfg.QueryParameters.ScheduleId = scheduleId;
            }
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method creates an upload task or a download task without filter criteria. When using this method, specify the <b>feedType</b> and the feed file <b> schemaVersion</b>. The feed type specified sets the task as a download or an upload task.
    /// For details about the upload and download flows, see <see href="/api-docs/sell/static/orders/generating-and-retrieving-order-reports.html">Working with Order Feeds</see> in the Selling Integration Guide.
    /// <b>Note:</b> The scope depends on the feed type. An error message results when an unsupported scope or feed type is specified.
    /// The following list contains this method's authorization scopes and their corresponding feed types:
    /// - https://api.ebay.com/oauth/api_scope/sell.inventory: See <see href="/api-docs/sell/static/feed/lms-feeds-quick-reference.html#Available">LMS FeedTypes</see>
    /// - https://api.ebay.com/oauth/api_scope/sell.fulfillment: LMS_ORDER_ACK (specify for upload tasks). Also see <see href="/api-docs/sell/static/feed/lms-feeds-quick-reference.html#Available">LMS FeedTypes</see>
    /// - https://api.ebay.com/oauth/api_scope/sell.marketing: None*
    /// - https://api.ebay.com/oauth/api_scope/commerce.catalog.readonly: None*
    /// * Reserved for future release
    /// </summary>
    public Task CreateTaskAsync(
        string xEBAYCMARKETPLACEID,
        CreateTaskRequest body,
        string? acceptLanguage = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Task.PostAsync(body, cfg =>
        {
            cfg.Headers.TryAdd("X-EBAY-C-MARKETPLACE-ID", xEBAYCMARKETPLACEID);
            if (acceptLanguage is not null)
            {
                cfg.Headers.TryAdd("Accept-Language", acceptLanguage);
            }
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method downloads a file previously uploaded using <b>uploadFile</b>. Specify the <b>task_id</b> from the <b>uploadFile</b> call.
    /// <b>Note:</b> With respect to LMS, this method applies to all feed types except <c>LMS_ORDER_REPORT</c> and <c>LMS_ACTIVE_INVENTORY_REPORT</c>. See <see href="/api-docs/sell/static/feed/lms-feeds.html">LMS API Feeds</see> in the Selling Integration Guide.
    /// </summary>
    public Task GetInputFileAsync(
        string taskId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Task[taskId].Download_input_file.GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method retrieves the generated file that is associated with the specified task ID. The response of this call is a compressed or uncompressed CSV, XML, or JSON file, with the applicable file extension (for example: csv.gz).
    /// For details about how this method is used, see <see href="/api-docs/sell/static/orders/generating-and-retrieving-order-reports.html">Working with Order Feeds</see> in the Selling Integration Guide.
    /// <b>Note:</b> The status of the task to retrieve must be in the <c>COMPLETED</c> or <c>COMPLETED_WITH_ERROR</c> state before this method can retrieve the file. You can use the <b>getTask</b> or <b>getTasks</b> method to retrieve the status of the task.
    /// </summary>
    public Task GetResultFileAsync(
        string taskId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Task[taskId].Download_result_file.GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method retrieves the details and status of the specified task. The input is <b>task_id</b>.
    /// For details of how this method is used, see <see href="/api-docs/sell/static/orders/generating-and-retrieving-order-reports.html">Working with Order Feeds</see> in the Selling Integration Guide.
    /// </summary>
    public Task<TaskObject?> GetTaskAsync(
        string taskId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Task[taskId].GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method associates the specified file with the specified task ID and uploads the input file. After the file has been uploaded, the processing of the file begins.
    /// Reports often take time to generate and it's common for this method to return an HTTP status of 202, which indicates the report is being generated. Use the <b>getTask</b> with the task ID or <b>getTasks</b> to determine the status of a report.
    /// The status flow is <c>QUEUED</c> &gt; <c>IN_PROCESS</c> &gt; <c>COMPLETED</c> or <c>COMPLETED_WITH_ERROR</c>. When the status is <c>COMPLETED</c> or <c>COMPLETED_WITH_ERROR</c>, this indicates the file has been processed and the order report can be downloaded. If there are errors, they will be indicated in the report file.
    /// For details of how this method is used in the upload flow, see <see href="/api-docs/sell/static/orders/generating-and-retrieving-order-reports.html">Working with Order Feeds</see> in the Selling Integration Guide.
    /// This call does not have a JSON Request payload but uploads the file as form-data. For example:
    /// fileName: "AddFixedPriceItem_Macbook.xml"
    /// name: "file"
    /// type: "form-data"
    /// file: @"/C:/Users/.../AddFixedPriceItem_Macbook.7z"
    /// See <b>Samples</b> for information.
    /// <b>Note:</b> This method applies to all <see href="/api-docs/sell/static/feed/fx-feeds-quick-reference.html#availabl">Seller Hub feed types</see>, and to all <see href="/api-docs/sell/static/feed/lms-feeds-quick-reference.html#Availabl">LMS feed types</see> except <c>LMS_ORDER_REPORT</c> and <c>LMS_ACTIVE_INVENTORY_REPORT</c>.
    /// <b>Note:</b> You must use a <b>Content-Type</b> header with its value set to "<b>multipart/form-data</b>". See <b>Samples</b> for information.
    /// <b>Note:</b> For LMS feed types, upload a regular XML file or an XML file in zipped format (both formats are allowed).
    /// </summary>
    public Task UploadFileAsync(
        string taskId,
        Stream fileContent,
        string fileName,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () =>
            {
                var requestInfo = _kiota.Task[taskId].Upload_file.ToPostRequestInformation(new MultipartBody());
                RestInvocation.ApplyHeaders(requestInfo.Headers, _config, requestOptions);
                return RestInvocation.SendMultipartFileAsync(
                    _adapter,
                    requestInfo,
                    "file",
                    fileContent,
                    fileName,
                    ct);
            });

    /// <summary>
    /// Use this method to return an array of customer service metric tasks. You can limit the tasks returned by specifying a date range.
    /// <b>Note:</b> You can pass in either the <c>look_back_days</c> or <c>date_range</c>, but not both.
    /// </summary>
    public Task<CustomerServiceMetricTaskCollection?> GetCustomerServiceMetricTasksAsync(
        string feedType,
        string? dateRange = null,
        int? limit = null,
        int? lookBackDays = null,
        int? offset = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Customer_service_metric_task.GetAsync(cfg =>
        {
            cfg.QueryParameters.FeedType = feedType;
            if (dateRange is not null)
            {
                cfg.QueryParameters.DateRange = dateRange;
            }
            if (limit is not null)
            {
                cfg.QueryParameters.Limit = limit;
            }
            if (lookBackDays is not null)
            {
                cfg.QueryParameters.LookBackDays = lookBackDays;
            }
            if (offset is not null)
            {
                cfg.QueryParameters.Offset = offset;
            }
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// Use this method to create a customer service metrics download task with filter criteria for the customer service metrics report. When using this method, specify the <b>feedType</b> and <b>filterCriteria</b> including both <b>evaluationMarketplaceId</b> and <b>customerServiceMetricType</b> for the report. The method returns the location response header containing the call URI to use with <b>getCustomerServiceMetricTask</b> to retrieve status and details on the task.
    /// Only CURRENT Customer Service Metrics reports can be generated with the Sell Feed API. PROJECTED reports are not supported at this time. See the <see href="/api-docs/sell/analytics/resources/customer_service_metric/methods/getCustomerServiceMetric">getCustomerServiceMetric</see> method document in the Analytics API for more information about these two types of reports.
    /// <b>Note:</b> Before calling this API, retrieve the summary of the seller's performance and rating for the customer service metric by calling <b>getCustomerServiceMetric</b> (part of the <see href="/develop/api/sell/analytics_api">Analytics API</see>). You can then populate the create task request fields with the values from the response. This technique eliminates failed tasks that request a report for a <b>customerServiceMetricType</b> and <b>evaluationMarketplaceId</b> that are without evaluation.
    /// </summary>
    public Task CreateCustomerServiceMetricTaskAsync(
        string acceptLanguage,
        CreateServiceMetricsTaskRequest body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Customer_service_metric_task.PostAsync(body, cfg =>
        {
            cfg.Headers.TryAdd("Accept-Language", acceptLanguage);
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// Use this method to retrieve customer service metric task details for the specified task. The input is <b>task_id</b>.
    /// </summary>
    public Task<ServiceMetricsTask?> GetCustomerServiceMetricTaskAsync(
        string taskId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Customer_service_metric_task[taskId].GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));
}
