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


using EBay.ListingManagement.Configuration;
using EBay.ListingManagement.Exceptions;
using EBay.ListingManagement.Generated.Media;
using EBay.ListingManagement.Generated.Media.Models;
using Microsoft.Kiota.Abstractions;
using System.IO;
using System.Net;
using System.Net.Http;

namespace EBay.ListingManagement.Clients.Rest;

public sealed class MediaClient
{
    private readonly MediaApiClient _kiota;
    private readonly IRequestAdapter _adapter;
    private readonly ListingManagementConfiguration _config;
    private readonly ITokenContext _tokenContext;

    internal MediaClient(MediaApiClient kiota, IRequestAdapter adapter, ListingManagementConfiguration config, ITokenContext tokenContext)
        => (_kiota, _adapter, _config, _tokenContext) = (kiota, adapter, config, tokenContext);

    private void ApplyHeaders(RequestHeaders headers, RequestOptions? callOptions)
    {
        foreach (var (k, v) in _config.DefaultRequestOptions.Headers)
            headers.TryAdd(k, v);
        if (callOptions is null) return;
        foreach (var (k, v) in callOptions.Headers)
            headers[k] = new[] { v };
    }

    private async Task<T> CallAsync<T>(string? accessToken, Func<Task<T>> op)
    {
        try { return await _tokenContext.RunAsync(accessToken, op); }
        catch (ApiException ex) when (ex.ResponseStatusCode == (int)HttpStatusCode.Unauthorized)
        { throw new UnauthorizedException(ex); }
    }

    // ── Documents ─────────────────────────────────────────────────────────────

    public Task<CreateDocumentResponse?> CreateDocumentAsync(CreateDocumentRequest body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Document.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<CreateDocumentResponse?> CreateDocumentFromUrlAsync(CreateDocumentFromUrlRequest body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Document.Create_document_from_url.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<DocumentResponse?> GetDocumentAsync(string documentId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Document[documentId].GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<DocumentResponse?> UploadDocumentAsync(string documentId, Stream fileContent, string fileName, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, async () =>
        {
            using var multipart = new MultipartFormDataContent();
            multipart.Add(new StreamContent(fileContent), "file", fileName);

            var memStream = new MemoryStream();
            await multipart.CopyToAsync(memStream, ct).ConfigureAwait(false);
            memStream.Position = 0;

            var requestInfo = _kiota.Document[documentId].Upload.ToPostRequestInformation();
            ApplyHeaders(requestInfo.Headers, requestOptions);
            requestInfo.SetStreamContent(memStream, multipart.Headers.ContentType!.ToString());

            return await _adapter.SendAsync(
                requestInfo,
                DocumentResponse.CreateFromDiscriminatorValue,
                default,
                ct).ConfigureAwait(false);
        });

    // ── Images ────────────────────────────────────────────────────────────────

    public Task<ImageResponse?> CreateImageFromFileAsync(Stream imageContent, string fileName, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, async () =>
        {
            using var multipart = new MultipartFormDataContent();
            multipart.Add(new StreamContent(imageContent), "image", fileName);

            var memStream = new MemoryStream();
            await multipart.CopyToAsync(memStream, ct).ConfigureAwait(false);
            memStream.Position = 0;

            var requestInfo = _kiota.Image.Create_image_from_file.ToPostRequestInformation();
            ApplyHeaders(requestInfo.Headers, requestOptions);
            requestInfo.SetStreamContent(memStream, multipart.Headers.ContentType!.ToString());

            return await _adapter.SendAsync(
                requestInfo,
                ImageResponse.CreateFromDiscriminatorValue,
                default,
                ct).ConfigureAwait(false);
        });

    public Task<ImageResponse?> CreateImageFromUrlAsync(CreateImageFromUrlRequest body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Image.Create_image_from_url.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<ImageResponse?> GetImageAsync(string imageId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Image[imageId].GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    // ── Videos ───────────────────────────────────────────────────────────────

    public async Task CreateVideoAsync(CreateVideoRequest body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
    {
        try { await _tokenContext.RunAsync(accessToken, () => _kiota.Video.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct)); }
        catch (ApiException ex) when (ex.ResponseStatusCode == (int)HttpStatusCode.Unauthorized)
        { throw new UnauthorizedException(ex); }
    }

    public Task<Video?> GetVideoAsync(string videoId, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Video[videoId].GetAsync(cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));

    public Task<Stream?> UploadVideoAsync(string videoId, Stream body, string? accessToken = null, RequestOptions? requestOptions = null, CancellationToken ct = default)
        => CallAsync(accessToken, () => _kiota.Video[videoId].Upload.PostAsync(body, cfg => ApplyHeaders(cfg.Headers, requestOptions), ct));
}
