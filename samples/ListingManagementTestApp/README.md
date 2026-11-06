# Listing Management SDK test app

A console app (net10.0, runs on macOS, Linux and Windows) for checking this branch of `EBay.ListingManagement`: the generated GraphQL client that replaced StrawberryShake, the trusted-hosts rules, the `(config, handler)` constructor and the single-client DI registration. It references the SDK project directly, so it always runs the code in this checkout.

```bash
cd samples/ListingManagementTestApp
dotnet run -- smoke
dotnet run -- live --env sandbox --token "v^1.1#..."
```

## `smoke`: offline checks

No network or token. The real client, serializers and auth pipeline run against canned eBay responses through `new ListingManagementClient(config, handler)`. It prints `PASS`/`FAIL` per check and exits with the number of failures:

- the full `live` flow below, in production and sandbox, standalone and through `AddListingManagementSdk`: every request goes to a configured base URL and carries the token
- GraphQL unions parse into the generated interfaces (`SellerListings`, `SingleSkuListing`/`VariationListing`, enums, `DateTimeOffset`)
- the GraphQL request: `operationName`, variables, unset input fields omitted, per-call token overriding the configured one
- GraphQL `errors` become `GraphQLException`; HTTP 401 becomes `UnauthorizedException` for GraphQL and REST
- a base URL on an unknown host is rejected unless it is in `AllowedHosts`
- the token is never sent to a GraphQL host changed after the client was created

## `live`: real eBay calls (read-only)

Nothing is created, changed or ended. The app calls, in order:

1. GraphQL `sellerListingSearch`: your active listings (first `--limit`)
2. GraphQL `sellerListings`: details for those listing ids (format, status, quantity, title; single vs. variation)
3. REST Inventory `getInventoryItems`
4. REST Stores `getStore`

Each call reports its own result, so one failure doesn't stop the rest.

| Option | |
| --- | --- |
| `--env sandbox\|production` | Default `production`. Sandbox points every API at `*.sandbox.ebay.com`. |
| `--token <token>` | User access token for that environment. Default: `$EBAY_ACCESS_TOKEN`. |
| `--limit <n>` | Listings / inventory items to fetch. Default 5. |
| `--graphql-url <url>` | Override the GraphQL endpoint; a host that isn't an SDK default is added to `AllowedHosts`. |
| `--di` | Build the client through `services.AddListingManagementSdk(config)` instead of `new ListingManagementClient(config)`. |
| `--verbose` | SDK diagnostic logging of failed HTTP calls to stderr. |

**Token:** a user access token (eBay developer portal → *User Tokens*, or your own OAuth flow) for the environment you pick. `getInventoryItems` needs `sell.inventory` (or `sell.inventory.readonly`) and `getStore` needs `sell.stores`; the GraphQL schema doesn't document scopes for the listing queries. A token for the wrong environment gives 401 on every call.

**Sandbox GraphQL:** `https://api.sandbox.ebay.com/graphql` returns 404. The sandbox GraphQL API is at `https://graphqlapi.sandbox.ebay.com/graphql` (verified with a sandbox user token). That host isn't one of the SDK's default trusted hosts yet, so the app adds it to `AllowedHosts`.

**REST error details:** the OpenAPI specs declare error responses as a single `Error` object, but eBay returns `{"errors":[...]}`. The SDK's REST exceptions therefore carry the HTTP status code but none of eBay's error text, so the app prints only the status. To see eBay's message, repeat the call with curl. For example, `getStore` in the sandbox returns 400 "The user must have an active store subscription." for an account without a store.
