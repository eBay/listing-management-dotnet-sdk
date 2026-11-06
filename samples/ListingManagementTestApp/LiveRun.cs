using EBay.ListingManagement;
using EBay.ListingManagement.Exceptions;
using Microsoft.Kiota.Abstractions;

namespace ListingManagementTestApp;

/// <summary>
/// Calls eBay through the SDK. Read steps (<see cref="RunReadsAsync"/>) never change anything; write steps
/// (<see cref="RunWritesAsync"/>) only run in the sandbox. Each call feeds ids to the next (SKU → offer → listing,
/// task → result file, ...) and reports OK, N/A (eBay says the data doesn't exist for this account), SKIP (nothing
/// to call it with) or FAIL. The exit code is the number of FAILs.
/// </summary>
internal sealed partial class LiveRun
{
    private readonly ListingManagementClient _client;
    private readonly Options _options;
    private int _ok, _notAvailable, _skipped, _failed;

    private LiveRun(ListingManagementClient client, Options options)
    {
        _client = client;
        _options = options;
    }

    public static async Task<int> RunAsync(Options options, HttpMessageHandler? handler = null)
    {
        if (options.Write && options.Environment != EbayEnvironment.Sandbox)
        {
            Console.Error.WriteLine("--write only runs in the sandbox (--env sandbox). Nothing was called.");
            return 2;
        }

        var config = SdkFactory.Configure(options.Environment, options.Token, options.GraphQLUrl, options.Verbose);
        var (client, owner) = SdkFactory.Create(config, options.UseDependencyInjection, handler);
        using var _ = owner;

        Console.WriteLine($"Environment: {options.Environment}   client: {(options.UseDependencyInjection ? "AddListingManagementSdk" : "new ListingManagementClient")}   mode: {(options.Write ? "read + write" : "read-only")}");
        Console.WriteLine($"GraphQL {config.GraphQLBaseUrl} | Inventory {config.InventoryBaseUrl} | Media {config.MediaBaseUrl}");
        Console.WriteLine();

        var run = new LiveRun(client, options);
        await run.RunReadsAsync();
        if (options.Write)
        {
            Console.WriteLine();
            await run.RunWritesAsync();
        }

        Console.WriteLine();
        Console.WriteLine($"{run._ok} OK, {run._notAvailable} N/A, {run._skipped} skipped, {run._failed} failed");
        return run._failed;
    }

    /// <summary>Runs one call. <paramref name="notAvailable"/> lists HTTP statuses that mean "no such data for this account".</summary>
    private async Task Step(string area, string name, Func<Task<string>> call, params int[] notAvailable)
    {
        try
        {
            Report("OK", area, name, await call());
        }
        catch (Exception ex) when (StatusOf(ex) is { } status && notAvailable.Contains(status))
        {
            Report("N/A", area, name, $"HTTP {status}");
        }
        catch (Exception ex) when (IsPermissionDenied(ex))
        {
            Report("N/A", area, name, "no permission: the token lacks the scope or the account isn't enabled for this API");
        }
        catch (Exception ex)
        {
            Report("FAIL", area, name, Describe(ex));
        }
    }

    /// <summary>403s and GraphQL permission errors say the token or account can't use the API, not that the SDK failed.</summary>
    private static bool IsPermissionDenied(Exception ex) =>
        StatusOf(ex) == 403
        || ex is GraphQLException graphQL && graphQL.Errors.Any(e => e.Contains("permission", StringComparison.OrdinalIgnoreCase));

    /// <summary>A call that must fail with one of <paramref name="expected"/>: checks the SDK's error path.</summary>
    private async Task ExpectError(string area, string name, Func<Task> call, params int[] expected)
    {
        try
        {
            await call();
            Report("FAIL", area, name, "expected an error, the call succeeded");
        }
        catch (Exception ex) when (StatusOf(ex) is { } status && expected.Contains(status))
        {
            Report("OK", area, name, $"failed as expected with HTTP {status} ({ex.GetType().Name})");
        }
        catch (Exception ex)
        {
            Report("FAIL", area, name, Describe(ex));
        }
    }

    private void Skip(string area, string name, string reason) => Report("SKIP", area, name, reason);

    private void Report(string status, string area, string name, string summary)
    {
        _ = status switch
        {
            "OK" => _ok++,
            "N/A" => _notAvailable++,
            "SKIP" => _skipped++,
            _ => _failed++,
        };
        Console.WriteLine($"{status,-5} {area,-9} {name,-36} {summary}");
    }

    private static int? StatusOf(Exception ex) => ex switch
    {
        ApiException api => api.ResponseStatusCode,
        UnauthorizedException => 401,
        _ => null,
    };

    private static string Describe(Exception ex) => ex switch
    {
        UnauthorizedException => "401 Unauthorized: the token is missing, expired, for the other environment, or lacks the scope",
        GraphQLException graphQL => "GraphQL errors: " + string.Join("; ", graphQL.Errors),
        // The specs declare error bodies as a single Error, but eBay sends {"errors":[...]}, so the mapped
        // exception has no details; only the status code is reliable. --verbose logs the request.
        ApiException api => $"HTTP {api.ResponseStatusCode} ({api.GetType().Name}); no error details, see README",
        _ => $"{ex.GetType().Name}: {ex.Message}",
    };

    private static string Join<T>(IEnumerable<T> values, int max = 3)
    {
        var list = values.ToList();
        return list.Count == 0 ? "none" : string.Join(", ", list.Take(max)) + (list.Count > max ? $", … ({list.Count})" : "");
    }
}
