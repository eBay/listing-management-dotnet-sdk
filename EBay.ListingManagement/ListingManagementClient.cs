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

using EBay.ListingManagement.Clients.GraphQL;
using EBay.ListingManagement.Clients.Rest;
using EBay.ListingManagement.Configuration;
using EBay.ListingManagement.Extensions;
using EBay.ListingManagement.Generated.Feed;
using EBay.ListingManagement.Generated.GraphQL;
using EBay.ListingManagement.Generated.Inventory;
using EBay.ListingManagement.Generated.Media;
using EBay.ListingManagement.Generated.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace EBay.ListingManagement;

public class ListingManagementClient : IDisposable
{
    internal const string FeedClientName = "ListingManagementClient.Feed";
    internal const string InventoryClientName = "ListingManagementClient.Inventory";
    internal const string MediaClientName = "ListingManagementClient.Media";
    internal const string StoresClientName = "ListingManagementClient.Stores";
    internal const string GraphQLClientName = "ListingManagementGraphQLClient";

    private readonly HttpClient[] _ownedClients;

    public FeedClient Feed { get; private set; } = null!;
    public InventoryClient Inventory { get; private set; } = null!;
    public MediaClient Media { get; private set; } = null!;
    public StoresClient Stores { get; private set; } = null!;

    public ListingGraphQLClient GraphQL { get; }

    /// <summary>
    /// DI-friendly constructor. Named HttpClients come from <see cref="IHttpClientFactory"/>.
    /// </summary>
    internal ListingManagementClient(
        ListingManagementConfiguration config,
        IHttpClientFactory factory,
        IListingManagementGraphQLClient graphQLClient,
        ITokenContext tokenContext)
    {
        _ownedClients = [];
        var auth = new BaseBearerTokenAuthenticationProvider(new KiotaAuthProvider(config, tokenContext));
        InitializeRestClients(auth, config, tokenContext, endpoint => factory.CreateClient(endpoint.HttpClientName));
        GraphQL = new ListingGraphQLClient(
            graphQLClient,
            new DynamicGraphQLClient(factory.CreateClient(GraphQLClientName), config, tokenContext),
            tokenContext);
    }

    /// <summary>
    /// Standalone constructor for use outside DI (tests, scripts).
    /// Creates its own HttpClients with DiagnosticLoggingHandler.
    /// Typed GraphQL operations are unavailable; use GraphQL.ExecuteRawUnsafeAsync instead.
    /// </summary>
    public ListingManagementClient(ListingManagementConfiguration config)
    {
        var tokenContext = new AsyncLocalTokenContext();
        var auth = new BaseBearerTokenAuthenticationProvider(new KiotaAuthProvider(config, tokenContext));
        var owned = new List<HttpClient>();
        InitializeRestClients(auth, config, tokenContext, endpoint =>
        {
            var client = BuildStandaloneHttpClient(config, endpoint.BaseUrl);
            owned.Add(client);
            return client;
        });
        var graphQLHttp = BuildStandaloneHttpClient(config, config.GraphQLBaseUrl);
        owned.Add(graphQLHttp);
        _ownedClients = [.. owned];
        GraphQL = new ListingGraphQLClient(
            BuildStandaloneGqlClient(config, tokenContext),
            new DynamicGraphQLClient(graphQLHttp, config, tokenContext),
            tokenContext);
    }

    private static IListingManagementGraphQLClient BuildStandaloneGqlClient(
        ListingManagementConfiguration config, ITokenContext tokenContext)
    {
        var services = new ServiceCollection();
        services.AddListingManagementSdk(config);
        services.AddSingleton<ITokenContext>(tokenContext);
        return services.BuildServiceProvider().GetRequiredService<IListingManagementGraphQLClient>();
    }

    private static HttpClient BuildStandaloneHttpClient(ListingManagementConfiguration config, string baseUrl)
    {
        var handler = new DiagnosticLoggingHandler(config)
        {
            InnerHandler = new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) }
        };
        return new HttpClient(handler) { BaseAddress = new Uri(baseUrl), Timeout = config.HttpTimeout };
    }

    internal static IReadOnlyList<(string ApiId, string HttpClientName)> RestEndpointNames { get; } =
    [
        ("feed", FeedClientName),
        ("inventory", InventoryClientName),
        ("media", MediaClientName),
        ("stores", StoresClientName)
    ];

    internal static IReadOnlyList<RestEndpoint> RestEndpoints(ListingManagementConfiguration config) =>
        [.. RestEndpointNames.Select(name => new RestEndpoint(name.ApiId, name.HttpClientName, config.RestBaseUrl(name.ApiId)))];

    private void InitializeRestClients(
        IAuthenticationProvider auth,
        ListingManagementConfiguration config,
        ITokenContext tokenContext,
        RestHttpClientProvider httpClientFor)
    {
        var endpoints = RestEndpoints(config);
        var feedAdapter = new HttpClientRequestAdapter(auth, httpClient: httpClientFor(endpoints.First(endpoint => endpoint.ApiId == "feed")));
        Feed = new FeedClient(new FeedApiClient(feedAdapter), config, tokenContext, feedAdapter);
        var inventoryAdapter = new HttpClientRequestAdapter(auth, httpClient: httpClientFor(endpoints.First(endpoint => endpoint.ApiId == "inventory")));
        Inventory = new InventoryClient(new InventoryApiClient(inventoryAdapter), config, tokenContext, inventoryAdapter);
        var mediaAdapter = new HttpClientRequestAdapter(auth, httpClient: httpClientFor(endpoints.First(endpoint => endpoint.ApiId == "media")));
        Media = new MediaClient(new MediaApiClient(mediaAdapter), config, tokenContext, mediaAdapter);
        var storesAdapter = new HttpClientRequestAdapter(auth, httpClient: httpClientFor(endpoints.First(endpoint => endpoint.ApiId == "stores")));
        Stores = new StoresClient(new StoresApiClient(storesAdapter), config, tokenContext, storesAdapter);
    }

    public void Dispose()
    {
        foreach (var client in _ownedClients)
        {
            client.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}

internal sealed record RestEndpoint(string ApiId, string HttpClientName, string BaseUrl);

internal delegate HttpClient RestHttpClientProvider(RestEndpoint endpoint);
