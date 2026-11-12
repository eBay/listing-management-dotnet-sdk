using Xunit;

namespace EBay.ListingManagement.Tests;

public sealed class PublicApiTests
{
    [Fact]
    public void Only_sdk_namespaces_are_public()
    {
        // Regression: PolySharp 1.17 generated a public System.Runtime.CompilerServices.IsClosedTypeAttribute.
        // Compiler polyfills must stay internal so they never clash with the consumer's framework types.
        var leaked = typeof(ListingManagementClient).Assembly.GetExportedTypes()
            .Where(type => type.Namespace is null || !type.Namespace.StartsWith("EBay.", StringComparison.Ordinal))
            .Select(type => type.FullName);

        Assert.Empty(leaked);
    }
}
