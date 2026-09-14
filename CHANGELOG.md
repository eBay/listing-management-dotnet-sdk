# ChangeLog

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
