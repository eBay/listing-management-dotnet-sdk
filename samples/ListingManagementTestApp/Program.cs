using ListingManagementTestApp;

// dotnet run -- smoke
// dotnet run -- live --env sandbox|production [--token <token>] [--limit 5] [--graphql-url <url>] [--di] [--verbose] [--write]
var command = args.FirstOrDefault()?.ToLowerInvariant();
var options = Options.Parse(args.Skip(1).ToArray());

switch (command)
{
    case "smoke":
        return await SmokeTests.RunAsync();
    case "live" when options.Token is null:
        Console.Error.WriteLine("No access token. Pass --token <token> or set EBAY_ACCESS_TOKEN.");
        return 2;
    case "live":
        return await LiveRun.RunAsync(options);
    default:
        Console.WriteLine("""
            eBay Listing Management SDK test app

              dotnet run -- smoke
                  Offline checks with canned eBay responses: no network, no token.

              dotnet run -- live --env sandbox|production [options]
                  Calls against eBay with your user access token: every read-only GraphQL query and REST GET
                  the SDK offers for which the account has data (ids found by one call feed the next).

                  --token <token>      user access token (default: $EBAY_ACCESS_TOKEN)
                  --limit <n>          how many listings / inventory items to fetch (default 5)
                  --graphql-url <url>  override the GraphQL endpoint (a non-default host is added to AllowedHosts)
                  --di                 build the client through AddListingManagementSdk instead of new()
                  --verbose            SDK diagnostic logging of failed HTTP calls to stderr
                  --write              sandbox only: also create, upload, validate/create/end a listing, and delete
                                       (everything under a fresh SDKTEST- SKU, cleaned up at the end)
            """);
        return command is null or "help" or "--help" ? 0 : 2;
}

internal sealed record Options(
    EbayEnvironment Environment,
    string? Token,
    int Limit,
    string? GraphQLUrl,
    bool UseDependencyInjection,
    bool Verbose,
    bool Write)
{
    public static Options Parse(string[] args)
    {
        string? Value(string name)
        {
            var i = Array.IndexOf(args, name);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }

        var environment = Value("--env")?.ToLowerInvariant() switch
        {
            null or "production" or "prod" => EbayEnvironment.Production,
            "sandbox" => EbayEnvironment.Sandbox,
            var other => throw new ArgumentException($"Unknown --env '{other}'. Use sandbox or production."),
        };
        return new Options(
            environment,
            Value("--token") ?? System.Environment.GetEnvironmentVariable("EBAY_ACCESS_TOKEN"),
            int.TryParse(Value("--limit"), out var limit) && limit > 0 ? limit : 5,
            Value("--graphql-url"),
            args.Contains("--di"),
            args.Contains("--verbose"),
            args.Contains("--write"));
    }
}
