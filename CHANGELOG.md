# ChangeLog

## 0.4.0 (09/25/2026)

### Added

- Target frameworks `net8.0` and `net9.0`, in addition to `net10.0`.

## 0.3.0 (09/14/2026)

### Breaking changes

#### Renamed operations

| Client | Previous | Current |
| --- | --- | --- |
| `ListingGraphQLClient` | `GetSellerListingsAsync` | `SellerListingsAsync` |
| `ListingGraphQLClient` | `GetSellerListingSearchAsync` | `SellerListingSearchAsync` |
| `ListingGraphQLClient` | `GetVideosByIdsAsync` | `VideosByIdsAsync` |

#### Changed signatures

`FeedClient`:

| Operation | Previous parameters | Current parameters |
| --- | --- | --- |
| `CreateCustomerServiceMetricTaskAsync` | `(CreateServiceMetricsTaskRequest body, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` | `(string acceptLanguage, CreateServiceMetricsTaskRequest body, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` |
| `CreateTaskAsync` | `(CreateTaskRequest body, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` | `(string xEBAYCMARKETPLACEID, CreateTaskRequest body, string? acceptLanguage, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` |
| `GetCustomerServiceMetricTasksAsync` | `(Action<RequestConfiguration<Customer_service_metric_taskRequestBuilder.Customer_service_metric_taskRequestBuilderGetQueryParameters>>? configure, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` | `(string feedType, string? dateRange, int? limit, int? lookBackDays, int? offset, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` |
| `GetInventoryTasksAsync` | `(Action<RequestConfiguration<Inventory_taskRequestBuilder.Inventory_taskRequestBuilderGetQueryParameters>>? configure, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` | `(string feedType, string? scheduleId, int? lookBackDays, string? dateRange, int? limit, int? offset, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` |
| `GetOrderTasksAsync` | `(Action<RequestConfiguration<Order_taskRequestBuilder.Order_taskRequestBuilderGetQueryParameters>>? configure, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` | `(string? dateRange, string? feedType, int? limit, int? lookBackDays, int? offset, string? scheduleId, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` |
| `GetSchedulesAsync` | `(Action<RequestConfiguration<ScheduleRequestBuilder.ScheduleRequestBuilderGetQueryParameters>>? configure, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` | `(string feedType, int? limit, int? offset, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` |
| `GetScheduleTemplatesAsync` | `(Action<RequestConfiguration<Schedule_templateRequestBuilder.Schedule_templateRequestBuilderGetQueryParameters>>? configure, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` | `(string feedType, int? limit, int? offset, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` |
| `GetTasksAsync` | `(Action<RequestConfiguration<TaskRequestBuilder.TaskRequestBuilderGetQueryParameters>>? configure, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` | `(string? dateRange, string? feedType, int? limit, int? lookBackDays, int? offset, string? scheduleId, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` |

`InventoryClient`:

| Operation | Previous parameters | Current parameters |
| --- | --- | --- |
| `GetInventoryItemsAsync` | `(Action<RequestConfiguration<Inventory_itemRequestBuilder.Inventory_itemRequestBuilderGetQueryParameters>>? configure, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` | `(string? limit, string? offset, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` |
| `GetInventoryLocationsAsync` | `(Action<RequestConfiguration<LocationRequestBuilder.LocationRequestBuilderGetQueryParameters>>? configure, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` | `(string? limit, string? offset, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` |
| `GetOffersAsync` | `(Action<RequestConfiguration<OfferRequestBuilder.OfferRequestBuilderGetQueryParameters>>? configure, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` | `(string sku, string? format, string? limit, string? marketplaceId, string? offset, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` |

`MediaClient`:

| Operation | Previous parameters | Current parameters |
| --- | --- | --- |
| `UploadVideoAsync` | `(string videoId, Stream body, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` | `(string videoId, string? contentLength, string? contentRange, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)` |

### Added

`ListingGraphQLClient`:

- `UserSavedSearchesAsync`

`MediaClient`:

- `DownloadPostOrderDocumentAsync(string documentId, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)`
- `RemovePostOrderDocumentAsync(string documentId, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)`
- `UploadPostOrderDocumentAsync(Stream fileContent, string fileName, string? accessToken, RequestOptions? requestOptions, CancellationToken ct)`
