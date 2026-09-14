# ChangeLog

## 2.0.0 (09/14/2026)

**Breaking changes**
- The following operations changed signature on `FeedClient`:
  - `GetCustomerServiceMetricTasksAsync`: `(string? dateRange, string? feedType, string? limit, string? lookBackDays, string? offset, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` → `(string feedType, string? dateRange, int? limit, int? lookBackDays, int? offset, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)`
  - `GetInventoryTasksAsync`: `(string?, string?, string?, string?, string?, string?, string?, RequestOptions?, CancellationToken)` → `(string, string?, int?, string?, int?, int?, string?, RequestOptions?, CancellationToken)`
  - `GetOrderTasksAsync`: `(string?, string?, string?, string?, string?, string?, string?, RequestOptions?, CancellationToken)` → `(string?, string?, int?, int?, int?, string?, string?, RequestOptions?, CancellationToken)`
  - `GetSchedulesAsync`: `(string, string?, string?, string?, RequestOptions?, CancellationToken)` → `(string, int?, int?, string?, RequestOptions?, CancellationToken)`
  - `GetScheduleTemplatesAsync`: `(string, string?, string?, string?, RequestOptions?, CancellationToken)` → `(string, int?, int?, string?, RequestOptions?, CancellationToken)`
  - `GetTasksAsync`: `(string?, string?, string?, string?, string?, string?, string?, RequestOptions?, CancellationToken)` → `(string?, string?, int?, int?, int?, string?, string?, RequestOptions?, CancellationToken)`
- The following operations changed signature on `InventoryClient`:
  - `GetOffersAsync`: `(string? format, string? limit, string? marketplaceId, string? offset, string? sku, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` → `(string sku, string? format, string? limit, string? marketplaceId, string? offset, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)`
- The following operations changed signature on `MediaClient`:
  - `UploadVideoAsync`: `(string videoId, Stream body, string? contentLength, string? contentRange, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` → `(string videoId, string? contentLength, string? contentRange, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)`

- The following operations were added to `MediaClient`:
  - `DownloadPostOrderDocumentAsync(string, string?, RequestOptions?, CancellationToken)`
  - `RemovePostOrderDocumentAsync(string, string?, RequestOptions?, CancellationToken)`
  - `UploadPostOrderDocumentAsync(Stream, string, string?, RequestOptions?, CancellationToken)`

## 1.0.0 (09/14/2026)

**Breaking changes**
- The following operations were removed from `ListingGraphQLClient`:
  - `GetSellerListingsAsync`
  - `GetSellerListingSearchAsync`
  - `GetVideosByIdsAsync`
- The following operations changed signature on `FeedClient`:
  - `CreateCustomerServiceMetricTaskAsync`: `(CreateServiceMetricsTaskRequest, string?, RequestOptions?, CancellationToken)` → `(string, CreateServiceMetricsTaskRequest, string?, RequestOptions?, CancellationToken)`
  - `CreateTaskAsync`: `(CreateTaskRequest, string?, RequestOptions?, CancellationToken)` → `(string, CreateTaskRequest, string?, string?, RequestOptions?, CancellationToken)`
  - `GetCustomerServiceMetricTasksAsync`: `(Action<RequestConfiguration<Customer_service_metric_taskRequestBuilder.Customer_service_metric_taskRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` → `(string?, string?, string?, string?, string?, string?, RequestOptions?, CancellationToken)`
  - `GetInventoryTasksAsync`: `(Action<RequestConfiguration<Inventory_taskRequestBuilder.Inventory_taskRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` → `(string?, string?, string?, string?, string?, string?, string?, RequestOptions?, CancellationToken)`
  - `GetOrderTasksAsync`: `(Action<RequestConfiguration<Order_taskRequestBuilder.Order_taskRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` → `(string?, string?, string?, string?, string?, string?, string?, RequestOptions?, CancellationToken)`
  - `GetSchedulesAsync`: `(Action<RequestConfiguration<ScheduleRequestBuilder.ScheduleRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` → `(string, string?, string?, string?, RequestOptions?, CancellationToken)`
  - `GetScheduleTemplatesAsync`: `(Action<RequestConfiguration<Schedule_templateRequestBuilder.Schedule_templateRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` → `(string, string?, string?, string?, RequestOptions?, CancellationToken)`
  - `GetTasksAsync`: `(Action<RequestConfiguration<TaskRequestBuilder.TaskRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` → `(string?, string?, string?, string?, string?, string?, string?, RequestOptions?, CancellationToken)`
- The following operations changed signature on `InventoryClient`:
  - `GetInventoryItemsAsync`: `(Action<RequestConfiguration<Inventory_itemRequestBuilder.Inventory_itemRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` → `(string?, string?, string?, RequestOptions?, CancellationToken)`
  - `GetInventoryLocationsAsync`: `(Action<RequestConfiguration<LocationRequestBuilder.LocationRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` → `(string?, string?, string?, RequestOptions?, CancellationToken)`
  - `GetOffersAsync`: `(Action<RequestConfiguration<OfferRequestBuilder.OfferRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` → `(string?, string?, string?, string?, string?, string?, RequestOptions?, CancellationToken)`
- The following operations changed signature on `MediaClient`:
  - `UploadVideoAsync`: `(string, Stream, string?, RequestOptions?, CancellationToken)` → `(string, Stream, string?, string?, string?, RequestOptions?, CancellationToken)`

- The following operations were added to `ListingGraphQLClient`:
  - `SellerListingsAsync`
  - `SellerListingSearchAsync`
  - `UserSavedSearchesAsync`
  - `VideosByIdsAsync`
