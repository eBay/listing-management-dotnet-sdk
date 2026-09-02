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


using EBay.ListingManagement.Clients.Rest;
using EBay.ListingManagement.Configuration;
using EBay.ListingManagement.Generated.Feed;
using EBay.ListingManagement.Generated.Inventory;
using EBay.ListingManagement.Generated.Media;
using EBay.ListingManagement.Generated.Stores;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace EBay.ListingManagement;

public partial class ListingManagementClient
{
    public InventoryClient Inventory { get; private set; } = null!;
    public FeedClient Feed { get; private set; } = null!;
    public MediaClient Media { get; private set; } = null!;
    public StoresClient Stores { get; private set; } = null!;

    private void InitializeRestClients(
        IAuthenticationProvider auth,
        HttpClient inventoryClient,
        HttpClient feedClient,
        HttpClient mediaClient,
        HttpClient storesClient,
        ListingManagementConfiguration config,
        ITokenContext tokenContext)
    {
        Inventory = new InventoryClient(new InventoryApiClient(
            new HttpClientRequestAdapter(auth, httpClient: inventoryClient)), config, tokenContext);

        var feedAdapter = new HttpClientRequestAdapter(auth, httpClient: feedClient);
        Feed = new FeedClient(new FeedApiClient(feedAdapter), feedAdapter, config, tokenContext);

        var mediaAdapter = new HttpClientRequestAdapter(auth, httpClient: mediaClient);
        Media = new MediaClient(new MediaApiClient(mediaAdapter), mediaAdapter, config, tokenContext);

        Stores = new StoresClient(new StoresApiClient(
            new HttpClientRequestAdapter(auth, httpClient: storesClient)), config, tokenContext);
    }
}
