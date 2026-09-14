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

using EBay.ListingManagement;
using EBay.ListingManagement.Clients.GraphQL;
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

        services.AddListingManagementGraphQLClient();
        services.AddTransient<BearerTokenHandler>();
        services
            .AddHttpClient(ListingManagementClient.GraphQLClientName, client =>
                client.BaseAddress = new Uri(config.GraphQLBaseUrl))
            .ConfigureHttpClient(c => c.Timeout = config.HttpTimeout)
            .AddHttpMessageHandler<BearerTokenHandler>()
            .AddHttpMessageHandler(_ => new GraphQLHttpHandler(config));
        services.AddScoped<DynamicGraphQLClient>(sp =>
            new DynamicGraphQLClient(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(ListingManagementClient.GraphQLClientName),
                sp.GetRequiredService<ListingManagementConfiguration>(),
                sp.GetRequiredService<ITokenContext>()));

        foreach (var endpoint in ListingManagementClient.RestEndpointNames)
        {
            var id = endpoint.ApiId;
            services.AddHttpClient(endpoint.HttpClientName, (sp, c) =>
                    c.BaseAddress = new Uri(sp.GetRequiredService<ListingManagementConfiguration>().RestBaseUrl(id)))
                .ConfigureHttpClient((sp, c) =>
                    c.Timeout = sp.GetRequiredService<ListingManagementConfiguration>().HttpTimeout)
                .AddHttpMessageHandler(sp =>
                    new DiagnosticLoggingHandler(sp.GetRequiredService<ListingManagementConfiguration>()));
        }

        services.AddScoped<ListingManagementClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new ListingManagementClient(
                sp.GetRequiredService<ListingManagementConfiguration>(),
                factory,
                sp.GetRequiredService<IListingManagementGraphQLClient>(),
                sp.GetRequiredService<ITokenContext>());
        });

        services.AddScoped(sp => sp.GetRequiredService<ListingManagementClient>().Feed);
        services.AddScoped(sp => sp.GetRequiredService<ListingManagementClient>().Inventory);
        services.AddScoped(sp => sp.GetRequiredService<ListingManagementClient>().Media);
        services.AddScoped(sp => sp.GetRequiredService<ListingManagementClient>().Stores);
        services.AddScoped(sp => sp.GetRequiredService<ListingManagementClient>().GraphQL);
        return services;
    }
}
