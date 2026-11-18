using System.Net;
using System.Text;
using EBay.ListingManagement.Configuration;
using Xunit;

namespace EBay.ListingManagement.Tests;

public sealed class FileTransferTests
{
    [Fact]
    public async Task File_downloads_return_the_file_content()
    {
        // Regression: download methods returned Task and the file was discarded.
        var handler = new FileHandler(_ => new ByteArrayContent(Encoding.UTF8.GetBytes("sku,status\nSKU-1,OK\n")));
        using var client = new ListingManagementClient(new ListingManagementConfiguration { AccessToken = "t" }, handler);

        using var file = await client.Feed.GetResultFileAsync("task-1");

        using var reader = new StreamReader(file!);
        Assert.Equal("sku,status\nSKU-1,OK\n", await reader.ReadToEndAsync());
        Assert.EndsWith("/sell/feed/v1/task/task-1/download_result_file", handler.LastUri!.AbsolutePath);
    }

    [Fact]
    public async Task Uploads_send_multipart_and_leave_the_callers_stream_open()
    {
        // Regression: the multipart content was disposed and closed the caller's stream.
        var handler = new FileHandler(_ => new StringContent("""{"imageUrl":"https://i.ebayimg.com/x.jpg"}""", Encoding.UTF8, "application/json"));
        using var client = new ListingManagementClient(new ListingManagementConfiguration { AccessToken = "t" }, handler);
        using var image = new MemoryStream(Encoding.UTF8.GetBytes("image-bytes"));

        var created = await client.Media.CreateImageFromFileAsync(image, "photo.png");

        Assert.Equal("https://i.ebayimg.com/x.jpg", created?.ImageUrl);
        Assert.True(image.CanRead);
        Assert.StartsWith("multipart/form-data", handler.LastContentType);
        Assert.Contains("name=image; filename=photo.png", handler.LastBody);
        Assert.Contains("image-bytes", handler.LastBody);
    }

    private sealed class FileHandler(Func<HttpRequestMessage, HttpContent> respond) : HttpMessageHandler
    {
        public Uri? LastUri { get; private set; }
        public string? LastContentType { get; private set; }
        public string LastBody { get; private set; } = "";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastUri = request.RequestUri;
            LastContentType = request.Content?.Headers.ContentType?.ToString();
            LastBody = request.Content is null ? "" : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = respond(request), RequestMessage = request };
        }
    }
}
