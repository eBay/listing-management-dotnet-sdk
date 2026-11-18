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

namespace EBay.ListingManagement.Configuration;

public sealed class ListingManagementConfiguration
{
    private string _feedBaseUrl = "https://api.ebay.com/sell/feed/v1";
    private string _inventoryBaseUrl = "https://api.ebay.com/sell/inventory/v1";
    private string _mediaBaseUrl = "https://apim.ebay.com/commerce/media/v1_beta";
    private string _storesBaseUrl = "https://api.ebay.com/sell/stores/v1";
    private string _graphQLBaseUrl = "https://api.ebay.com/graphql";

    public string FeedBaseUrl
    {
        get => _feedBaseUrl;
        set => _feedBaseUrl = RequireHttps(value);
    }

    public string InventoryBaseUrl
    {
        get => _inventoryBaseUrl;
        set => _inventoryBaseUrl = RequireHttps(value);
    }

    public string MediaBaseUrl
    {
        get => _mediaBaseUrl;
        set => _mediaBaseUrl = RequireHttps(value);
    }

    public string StoresBaseUrl
    {
        get => _storesBaseUrl;
        set => _storesBaseUrl = RequireHttps(value);
    }

    public string GraphQLBaseUrl
    {
        get => _graphQLBaseUrl;
        set => _graphQLBaseUrl = RequireHttps(value);
    }

    /// <summary>Fallback bearer token used when no per-call accessToken is supplied.</summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// When true, logs the HTTP method, URL, status code, and response reason phrase for all
    /// non-success responses to stderr.
    /// <para>
    /// <b>WARNING — PII risk.</b> Response bodies and headers may contain personally identifiable
    /// information. Do not enable in production without appropriate data-handling controls.
    /// </para>
    /// </summary>
    public bool DiagnosticLogging { get; set; }

    /// <summary>Timeout applied to all HTTP requests. Defaults to 30 seconds.</summary>
    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Default headers applied to every REST request. Per-call RequestOptions override these.</summary>
    public RequestOptions DefaultRequestOptions { get; set; } = new();

    internal string RestBaseUrl(string apiId) => apiId switch
    {
        "feed" => _feedBaseUrl,
        "inventory" => _inventoryBaseUrl,
        "media" => _mediaBaseUrl,
        "stores" => _storesBaseUrl,
        _ => throw new ArgumentOutOfRangeException(nameof(apiId), apiId, "Unknown REST API.")
    };

    /// <summary>eBay hosts that receive the access token by default.</summary>
    public static IReadOnlyList<string> DefaultAllowedHosts { get; } = Array.AsReadOnly(new[]
    {
        "api.ebay.com",
        "api.sandbox.ebay.com",
        "apim.ebay.com",
        "apim.sandbox.ebay.com",
        "apiz.ebay.com",
        "apiz.sandbox.ebay.com",
        "graphqlapi.sandbox.ebay.com"
    });

    /// <summary>
    /// Additional hosts the SDK trusts with the access token, for example <c>api.sandbox.ebay.com</c>.
    /// Every configured base URL must use a host from <see cref="DefaultAllowedHosts"/> or this list;
    /// the client constructor throws otherwise. Requests to any other host never carry the token.
    /// </summary>
    public IList<string> AllowedHosts { get; set; } = new List<string>();

    internal ISet<string> TrustedHosts() =>
        new HashSet<string>(DefaultAllowedHosts.Concat(AllowedHosts ?? new List<string>()), StringComparer.OrdinalIgnoreCase);

    internal bool IsTrustedHost(Uri? uri) =>
        uri is not null
        && string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
        && TrustedHosts().Contains(uri.Host);

    internal void ValidateTrustedHosts()
    {
        var trusted = TrustedHosts();
        foreach (var (name, url) in new (string Name, string Url)[]
        {
            ("FeedBaseUrl", FeedBaseUrl),
            ("InventoryBaseUrl", InventoryBaseUrl),
            ("MediaBaseUrl", MediaBaseUrl),
            ("StoresBaseUrl", StoresBaseUrl),
            ("GraphQLBaseUrl", GraphQLBaseUrl)
        })
        {
            var host = new Uri(url).Host;
            if (!trusted.Contains(host))
            {
                throw new InvalidOperationException(
                    $"{name} uses host '{host}', which is not trusted with the access token. Add it to ListingManagementConfiguration.AllowedHosts.");
            }
        }
    }

    private static string RequireHttps(string value)
    {
        if (!value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"URL must use HTTPS: '{value}'", nameof(value));
        }
        return value;
    }
}
