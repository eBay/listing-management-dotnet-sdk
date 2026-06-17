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
using EBay.ListingManagement.Generated.GraphQL;
using Microsoft.Extensions.DependencyInjection;

namespace EBay.ListingManagement.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddListingManagementSdk(
        this IServiceCollection services,
        ListingManagementConfiguration config)
    {
        services.AddSingleton(config);
        services.AddSingleton<ITokenContext, AsyncLocalTokenContext>();

        // StrawberryShake: register generated client
        services.AddListingManagementGraphQLClient();

        // Wire auth into the named HTTP client that StrawberryShake uses
        services.AddTransient<BearerTokenHandler>();
        services
            .AddHttpClient("ListingManagementGraphQLClient", client =>
                client.BaseAddress = new Uri(config.GraphQLBaseUrl))
            .ConfigureHttpClient(c => c.Timeout = config.HttpTimeout)
            .AddHttpMessageHandler<BearerTokenHandler>();

        // Dynamic escape hatch
        services.AddScoped<DynamicGraphQLClient>(sp =>
            new DynamicGraphQLClient(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient("ListingManagementGraphQLClient"),
                sp.GetRequiredService<ListingManagementConfiguration>(),
                sp.GetRequiredService<ITokenContext>()));

        // Named REST clients — one per sub-API — with DiagnosticLoggingHandler wired in
        services.AddHttpClient(ListingManagementClient.InventoryClientName,
                (sp, c) => c.BaseAddress = new Uri(sp.GetRequiredService<ListingManagementConfiguration>().InventoryBaseUrl))
            .ConfigureHttpClient((sp, c) => c.Timeout = sp.GetRequiredService<ListingManagementConfiguration>().HttpTimeout)
            .AddHttpMessageHandler(sp => new DiagnosticLoggingHandler(sp.GetRequiredService<ListingManagementConfiguration>()));
        services.AddHttpClient(ListingManagementClient.FeedClientName,
                (sp, c) => c.BaseAddress = new Uri(sp.GetRequiredService<ListingManagementConfiguration>().FeedBaseUrl))
            .ConfigureHttpClient((sp, c) => c.Timeout = sp.GetRequiredService<ListingManagementConfiguration>().HttpTimeout)
            .AddHttpMessageHandler(sp => new DiagnosticLoggingHandler(sp.GetRequiredService<ListingManagementConfiguration>()));
        services.AddHttpClient(ListingManagementClient.MediaClientName,
                (sp, c) => c.BaseAddress = new Uri(sp.GetRequiredService<ListingManagementConfiguration>().MediaBaseUrl))
            .ConfigureHttpClient((sp, c) => c.Timeout = sp.GetRequiredService<ListingManagementConfiguration>().HttpTimeout)
            .AddHttpMessageHandler(sp => new DiagnosticLoggingHandler(sp.GetRequiredService<ListingManagementConfiguration>()));
        services.AddHttpClient(ListingManagementClient.StoreClientName,
                (sp, c) => c.BaseAddress = new Uri(sp.GetRequiredService<ListingManagementConfiguration>().StoreBaseUrl))
            .ConfigureHttpClient((sp, c) => c.Timeout = sp.GetRequiredService<ListingManagementConfiguration>().HttpTimeout)
            .AddHttpMessageHandler(sp => new DiagnosticLoggingHandler(sp.GetRequiredService<ListingManagementConfiguration>()));

        // Register facade
        services.AddScoped<ListingManagementClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new ListingManagementClient(
                sp.GetRequiredService<ListingManagementConfiguration>(),
                factory.CreateClient(ListingManagementClient.InventoryClientName),
                factory.CreateClient(ListingManagementClient.FeedClientName),
                factory.CreateClient(ListingManagementClient.MediaClientName),
                factory.CreateClient(ListingManagementClient.StoreClientName),
                sp.GetRequiredService<IListingManagementGraphQLClient>(),
                sp.GetRequiredService<ITokenContext>());
        });

        // Register sub-clients for direct injection
        services.AddScoped(sp => sp.GetRequiredService<ListingManagementClient>().Inventory);
        services.AddScoped(sp => sp.GetRequiredService<ListingManagementClient>().Feed);
        services.AddScoped(sp => sp.GetRequiredService<ListingManagementClient>().Media);
        services.AddScoped(sp => sp.GetRequiredService<ListingManagementClient>().Store);
        services.AddScoped(sp => sp.GetRequiredService<ListingManagementClient>().GraphQL);

        return services;
    }
}
