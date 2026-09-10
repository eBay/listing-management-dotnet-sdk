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
using EBay.ListingManagement.Generated.GraphQL;
using EBay.ListingManagement.Generated.Inventory;
using EBay.ListingManagement.Generated.Feed;
using EBay.ListingManagement.Generated.Media;
using EBay.ListingManagement.Generated.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace EBay.ListingManagement;

public sealed class ListingManagementClient : IDisposable
{
    // Named client keys used by ServiceCollectionExtensions
    internal const string InventoryClientName = "ListingManagementClient.Inventory";
    internal const string FeedClientName      = "ListingManagementClient.Feed";
    internal const string MediaClientName     = "ListingManagementClient.Media";
    internal const string StoresClientName    = "ListingManagementClient.Stores";

    private readonly HttpClient[] _ownedClients;

    public InventoryClient Inventory { get; }
    public FeedClient Feed { get; }
    public MediaClient Media { get; }
    public StoresClient Stores { get; }
    public ListingGraphQLClient GraphQL { get; }

    /// <summary>
    /// DI-friendly constructor: each HttpClient is a named client from IHttpClientFactory
    /// with DiagnosticLoggingHandler and BaseAddress already configured.
    /// </summary>
    internal ListingManagementClient(
        ListingManagementConfiguration config,
        HttpClient inventoryClient,
        HttpClient feedClient,
        HttpClient mediaClient,
        HttpClient storesClient,
        IListingManagementGraphQLClient ssGqlClient,
        ITokenContext tokenContext)
    {
        _ownedClients = [];

        var auth = new BaseBearerTokenAuthenticationProvider(new KiotaAuthProvider(config, tokenContext));

        Inventory = new InventoryClient(new InventoryApiClient(
            new HttpClientRequestAdapter(auth, httpClient: inventoryClient)), config, tokenContext);

        var feedAdapter = new HttpClientRequestAdapter(auth, httpClient: feedClient);
        Feed = new FeedClient(new FeedApiClient(feedAdapter), feedAdapter, config, tokenContext);

        var mediaAdapter = new HttpClientRequestAdapter(auth, httpClient: mediaClient);
        Media = new MediaClient(new MediaApiClient(mediaAdapter), mediaAdapter, config, tokenContext);

        Stores = new StoresClient(new StoresApiClient(
            new HttpClientRequestAdapter(auth, httpClient: storesClient)), config, tokenContext);

        GraphQL = new ListingGraphQLClient(
            ssGqlClient,
            new DynamicGraphQLClient(inventoryClient, config, tokenContext),
            tokenContext);
    }

    /// <summary>
    /// Standalone constructor for use outside DI (tests, scripts).
    /// Creates its own HttpClients with DiagnosticLoggingHandler and does not use StrawberryShake DI.
    /// Typed GraphQL operations are unavailable; use GraphQL.ExecuteRawUnsafeAsync instead.
    /// </summary>
    public ListingManagementClient(ListingManagementConfiguration config)
    {
        var tokenContext = new AsyncLocalTokenContext();
        var auth = new BaseBearerTokenAuthenticationProvider(new KiotaAuthProvider(config, tokenContext));

        var inventoryClient = BuildStandaloneHttpClient(config, config.InventoryBaseUrl);
        var feedClient      = BuildStandaloneHttpClient(config, config.FeedBaseUrl);
        var mediaClient     = BuildStandaloneHttpClient(config, config.MediaBaseUrl);
        var storesClient    = BuildStandaloneHttpClient(config, config.StoresBaseUrl);
        _ownedClients = [inventoryClient, feedClient, mediaClient, storesClient];

        Inventory = new InventoryClient(new InventoryApiClient(
            new HttpClientRequestAdapter(auth, httpClient: inventoryClient)), config, tokenContext);

        var feedAdapter = new HttpClientRequestAdapter(auth, httpClient: feedClient);
        Feed = new FeedClient(new FeedApiClient(feedAdapter), feedAdapter, config, tokenContext);

        var mediaAdapter = new HttpClientRequestAdapter(auth, httpClient: mediaClient);
        Media = new MediaClient(new MediaApiClient(mediaAdapter), mediaAdapter, config, tokenContext);

        Stores = new StoresClient(new StoresApiClient(
            new HttpClientRequestAdapter(auth, httpClient: storesClient)), config, tokenContext);

        GraphQL = new ListingGraphQLClient(
            BuildStandaloneGqlClient(config, tokenContext),
            new DynamicGraphQLClient(inventoryClient, config, tokenContext),
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

    public void Dispose()
    {
        foreach (var client in _ownedClients)
            client.Dispose();
    }
}
