# ChangeLog

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
| `CreateCustomerServiceMetricTaskAsync` | `(CreateServiceMetricsTaskRequest, string?, RequestOptions?, CancellationToken)` | `(string, CreateServiceMetricsTaskRequest, string?, RequestOptions?, CancellationToken)` |
| `CreateTaskAsync` | `(CreateTaskRequest, string?, RequestOptions?, CancellationToken)` | `(string, CreateTaskRequest, string?, string?, RequestOptions?, CancellationToken)` |
| `GetCustomerServiceMetricTasksAsync` | `(Action<RequestConfiguration<Customer_service_metric_taskRequestBuilder.Customer_service_metric_taskRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` | `(string, string?, int?, int?, int?, string?, RequestOptions?, CancellationToken)` |
| `GetInventoryTasksAsync` | `(Action<RequestConfiguration<Inventory_taskRequestBuilder.Inventory_taskRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` | `(string, string?, int?, string?, int?, int?, string?, RequestOptions?, CancellationToken)` |
| `GetOrderTasksAsync` | `(Action<RequestConfiguration<Order_taskRequestBuilder.Order_taskRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` | `(string?, string?, int?, int?, int?, string?, string?, RequestOptions?, CancellationToken)` |
| `GetSchedulesAsync` | `(Action<RequestConfiguration<ScheduleRequestBuilder.ScheduleRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` | `(string, int?, int?, string?, RequestOptions?, CancellationToken)` |
| `GetScheduleTemplatesAsync` | `(Action<RequestConfiguration<Schedule_templateRequestBuilder.Schedule_templateRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` | `(string, int?, int?, string?, RequestOptions?, CancellationToken)` |
| `GetTasksAsync` | `(Action<RequestConfiguration<TaskRequestBuilder.TaskRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` | `(string?, string?, int?, int?, int?, string?, string?, RequestOptions?, CancellationToken)` |

`InventoryClient`:

| Operation | Previous parameters | Current parameters |
| --- | --- | --- |
| `GetInventoryItemsAsync` | `(Action<RequestConfiguration<Inventory_itemRequestBuilder.Inventory_itemRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` | `(string?, string?, string?, RequestOptions?, CancellationToken)` |
| `GetInventoryLocationsAsync` | `(Action<RequestConfiguration<LocationRequestBuilder.LocationRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` | `(string?, string?, string?, RequestOptions?, CancellationToken)` |
| `GetOffersAsync` | `(Action<RequestConfiguration<OfferRequestBuilder.OfferRequestBuilderGetQueryParameters>>?, string?, RequestOptions?, CancellationToken)` | `(string, string?, string?, string?, string?, string?, RequestOptions?, CancellationToken)` |

`MediaClient`:

| Operation | Previous parameters | Current parameters |
| --- | --- | --- |
| `UploadVideoAsync` | `(string, Stream, string?, RequestOptions?, CancellationToken)` | `(string, string?, string?, string?, RequestOptions?, CancellationToken)` |

### Added

`ListingGraphQLClient`:

- `UserSavedSearchesAsync`

`MediaClient`:

- `DownloadPostOrderDocumentAsync(string, string?, RequestOptions?, CancellationToken)`
- `RemovePostOrderDocumentAsync(string, string?, RequestOptions?, CancellationToken)`
- `UploadPostOrderDocumentAsync(Stream, string, string?, RequestOptions?, CancellationToken)`
