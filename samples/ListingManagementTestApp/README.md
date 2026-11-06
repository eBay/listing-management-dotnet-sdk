# Listing Management SDK test app

A console app (net10.0, runs on macOS, Linux and Windows) for checking this branch of `EBay.ListingManagement`: the generated GraphQL client that replaced StrawberryShake, the trusted-hosts rules, the `(config, handler)` constructor and the single-client DI registration. It references the SDK project directly, so it always runs the code in this checkout.

```bash
cd samples/ListingManagementTestApp
dotnet run -- smoke
dotnet run -- live --env sandbox --token "v^1.1#..."
```

## `smoke`: offline checks

No network or token. The real client, serializers and auth pipeline run against a fake eBay (`FakeEbay.cs`) through `new ListingManagementClient(config, handler)`. It prints `PASS`/`FAIL` per check and exits with the number of failures; `SMOKE_VERBOSE=1` shows the inner runs:

- the full `live` flow below, in production and sandbox, standalone and through `AddListingManagementSdk`, and with `--write` in the sandbox: every call is made, goes to a configured base URL and carries the token; ids flow from one call to the next; `createListing` runs validate then execute; unset input fields are not sent
- `--write` refuses to run in production and sends nothing
- GraphQL unions parse into the generated interfaces (`SellerListings`, `SingleSkuListing`/`VariationListing`, enums, `DateTimeOffset`)
- the GraphQL request: `operationName`, variables, unset input fields omitted, per-call token overriding the configured one
- GraphQL `errors` become `GraphQLException`; HTTP 401 becomes `UnauthorizedException` for GraphQL and REST
- a base URL on an unknown host is rejected unless it is in `AllowedHosts`
- the token is never sent to a GraphQL host changed after the client was created

## `live`: real eBay calls

Without `--write` nothing is created, changed or deleted. The app makes every read call the SDK offers for which the account has data, feeding ids from one call to the next:

| API | Calls |
| --- | --- |
| GraphQL | `sellerListingSearch` (active, ended, scheduled) → `sellerListings`; `userSavedSearches`; `videosByIds`; a raw query via `ExecuteRawUnsafeAsync` |
| Inventory | `getInventoryItems` → `getInventoryItem` (per-call token), `bulkGetInventoryItem`, `getOffers` → `getOffer` → `getSkuLocationMapping`, `getProductCompatibility`, `getInventoryItemGroup`; `getInventoryLocations` → `getInventoryLocation` |
| Feed | `getTasks` → `getTask` → `getResultFile`; `getInventoryTasks`, `getOrderTasks`, `getCustomerServiceMetricTasks`; `getScheduleTemplates` → `getScheduleTemplate`; `getSchedules` → `getSchedule` |
| Media | `getVideo`; `getImage` with an unknown id (must fail with 400/404: checks the error path and the `apim` host) |
| Stores | `getStore`, `getStoreCategories`, `getStoreTasks` |

`--write` (**sandbox only**; refused for production) then adds, all under a fresh `SDKTEST-<timestamp>` SKU:

1. Inventory `createOrReplaceInventoryItem`, then `getInventoryItem` to check the round trip
2. Media `createImageFromFile`: multipart upload of a generated 800×800 PNG
3. GraphQL `createListing` in validate-only mode; only if eBay accepts it, `createListing` for real and `endListing` straight after. Otherwise `endListing` on the test SKU, which exercises the mutation and its error union without touching real data
4. Inventory `deleteInventoryItem` (cleanup), then `getInventoryItem` must return 404

Each line is `OK`, `N/A` (eBay says the data doesn't exist for this account, or the token/account has no permission for that API), `SKIP` (nothing to call it with, e.g. no offers) or `FAIL`. The exit code is the number of `FAIL`s.

| Option | |
| --- | --- |
| `--env sandbox\|production` | Default `production`. Sandbox points every API at `*.sandbox.ebay.com`. |
| `--token <token>` | User access token for that environment. Default: `$EBAY_ACCESS_TOKEN`. |
| `--limit <n>` | Listings / inventory items to fetch. Default 5. |
| `--graphql-url <url>` | Override the GraphQL endpoint; a host that isn't an SDK default is added to `AllowedHosts`. |
| `--di` | Build the client through `services.AddListingManagementSdk(config)` instead of `new ListingManagementClient(config)`. |
| `--verbose` | SDK diagnostic logging of failed HTTP calls to stderr. |
| `--write` | Sandbox only: also run the write calls above. |

**Token:** a user access token (eBay developer portal → *User Tokens*, or your own OAuth flow) for the environment you pick. `getInventoryItems` needs `sell.inventory` (or `sell.inventory.readonly`) and `getStore` needs `sell.stores`; the GraphQL schema doesn't document scopes for the listing queries. A token for the wrong environment gives 401 on every call.

**Sandbox GraphQL:** `https://api.sandbox.ebay.com/graphql` returns 404. The sandbox GraphQL API is at `https://graphqlapi.sandbox.ebay.com/graphql`, which the SDK trusts by default.

**REST error details:** the OpenAPI specs declare error responses as a single `Error` object, but eBay returns `{"errors":[...]}`. The SDK's REST exceptions therefore carry the HTTP status code but none of eBay's error text, so the app prints only the status. To see eBay's message, repeat the call with curl. For example, `getStore` in the sandbox returns 400 "The user must have an active store subscription." for an account without a store.

## Known SDK issues this app works around

These exist on `main` as well; they are not caused by the StrawberryShake replacement. (File downloads and upload streams are fixed on this branch: `getResultFile` reads the file, and the upload step fails if the SDK closes the caller's stream.)

- **REST error details are lost** (above).
- **Feed `FrequencyEnum` doesn't match eBay's values** in the sandbox: eBay sends `1-HOUR`/`4-HOURS`, the spec says `ONE_HOUR`/`FOUR_HOURS`, so those templates show no frequency.
