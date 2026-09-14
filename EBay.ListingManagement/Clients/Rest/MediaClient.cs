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
using EBay.ListingManagement.Generated.Media.Models;
using EBay.ListingManagement.Generated.Media;
using Microsoft.Kiota.Abstractions;
using System.IO;

namespace EBay.ListingManagement.Clients.Rest;

public sealed class MediaClient
{
    private readonly MediaApiClient _kiota;
    private readonly ListingManagementConfiguration _config;
    private readonly ITokenContext _tokenContext;
    private readonly IRequestAdapter _adapter;

    internal MediaClient(MediaApiClient kiota, ListingManagementConfiguration config, ITokenContext tokenContext, IRequestAdapter adapter)
    {
        _kiota = kiota;
        _config = config;
        _tokenContext = tokenContext;
        _adapter = adapter;
    }

    /// <summary>
    /// This method uploads a picture file to eBay Picture Services (EPS) using multipart/form-data.
    /// All images must comply with eBay's picture requirements, such as dimension and file size restrictions. For more information, see <see href="https://www.ebay.com/help/policies/listing-policies/picture-policy?id=4370">Picture policy</see>. The image formats supported are <b>JPG</b>, <b>GIF</b>, <b>PNG</b>, <b>BMP</b>, <b>TIFF</b>, <b>AVIF</b>, <b>HEIC</b>, and <b>WEBP</b>. For more information, see <see href="/api-docs/sell/static/inventory/managing-image-media.html#image-requirements">Image requirements</see>.
    /// <b>Note:</b> Animated GIF, and multi-page PNG/TIFF files, are not supported. Any animation effect of supported formats will be lost upon upload.
    /// This call does not have a JSON Request payload but uploads the file as form-data. For example:
    /// image: "sample_picture.jpg"
    /// See <see href="/api-docs/commerce/media/resources/image/methods/createImageFromFile#h2-samples">Samples</see> for information.
    /// <b>Note:</b> You must use a <b>Content-Type</b> header with its value set to 'multipart/form-data'.
    /// When an EPS image is successfully created, the method returns the HTTP Status Code <c>201 Created</c>. The method also returns the getImage URI in the <b>Location</b> response header.
    /// Important! </b>"&gt;<b>Important!</b> Make sure to capture the image ID URI returned in the response <see href="/api-docs/commerce/media/resources/image/methods/createImageFromFile#h2-output">location header</see> provided in the following format:
    /// <c>https://apim.ebay.com/commerce/media/v1_beta/image/<i>{image_id}</i></c>
    /// You can capture the entire URI, or just save the <c>{image_id}</c> only. Pass the <c>{image_id}</c> as a path parameter in the <b><see href="/api-docs/commerce/media/resources/image/methods/getImage">getImage</see></b> method to return the value needed to associate an image to a listing using the Trading and Inventory APIs.
    /// See <see href="/api-docs/sell/static/inventory/managing-image-media.html">Managing images</see> for additional details.
    /// Important! </b>"&gt;<b>Important!</b>All POST methods in the Media API, including this method, are subject to short-duration rate limits at the user level: 50 requests per 5 seconds.
    /// </summary>
    public Task<ImageResponse?> CreateImageFromFileAsync(
        Stream imageContent,
        string fileName,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () =>
            {
                var requestInfo = _kiota.Image.Create_image_from_file.ToPostRequestInformation();
                RestInvocation.ApplyHeaders(requestInfo.Headers, _config, requestOptions);
                return RestInvocation.SendMultipartFileAsync(
                    _adapter,
                    requestInfo,
                    "image",
                    imageContent,
                    fileName,
                    ImageResponse.CreateFromDiscriminatorValue,
                    ct);
            });

    /// <summary>
    /// This method uploads a picture to eBay Picture Services (EPS) from the specified URL. Specify the location of the picture on an external web server through the <b>imageUrl</b> field.
    /// All images must comply with eBay’s picture requirements, such as dimension and file size restrictions. For more information, see <see href="https://www.ebay.com/help/policies/listing-policies/picture-policy?id=4370">Picture policy</see>. The image formats supported are <b>JPG</b>, <b>GIF</b>, <b>PNG</b>, <b>BMP</b>, <b>TIFF</b>, <b>AVIF</b>, <b>HEIC</b>, and <b>WEBP</b>. In addition, the provided URL must be secured using HTTPS (HTTP is not permitted). For more information, see <see href="/api-docs/sell/static/inventory/managing-image-media.html#image-requirements">Image requirements</see>.
    /// <b>Note:</b> Animated GIF, and multi-page PNG/TIFF files, are not supported. Any animation effect of supported formats will be lost upon upload.
    /// When an EPS image is successfully created, the method returns the HTTP Status Code <c>201 Created</c>. The method also returns the getImage URI in the <b>Location</b> response header.
    /// Important! </b>"&gt;<b>Important!</b> Make sure to capture the image ID URI returned in the response <see href="/api-docs/commerce/media/resources/image/methods/createImageFromUrl#h2-output">location header</see> provided in the following format:
    /// <c>https://apim.ebay.com/commerce/media/v1_beta/image/<i>{image_id}</i></c>
    /// You can capture the entire URI, or just save the <c>{image_id}</c> only. Pass the <c>{image_id}</c> as a path parameter in the <b><see href="/api-docs/commerce/media/resources/image/methods/getImage">getImage</see></b> method to return the value needed to associate an image to a listing using the Trading and Inventory APIs.
    /// See <see href="/api-docs/sell/static/inventory/managing-image-media.html">Managing images</see> for additional details.
    /// Important! </b>"&gt;<b>Important!</b>All POST methods in the Media API, including this method, are subject to short-duration rate limits at the user level: 50 requests per 5 seconds.
    /// </summary>
    public Task<ImageResponse?> CreateImageFromUrlAsync(
        CreateImageFromUrlRequest body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Image.Create_image_from_url.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method retrieves an EPS image URL and its expiration details for the unique identifier specified in the path parameter <b>image_id</b>. Use the retrieved EPS image URL to add the image to a listing through the <b>Inventory API</b> or the <b>Trading API</b>. See <see href="/api-docs/sell/static/inventory/managing-image-media.html">Managing images</see> for additional details.
    /// <b>Note:</b> If a user inputs a valid <b>image_id</b> as a path parameter but the EPS image associated with that ID has expired, the call will fail and a <b>404 Not Found</b> status code will be returned.
    /// </summary>
    public Task<ImageResponse?> GetImageAsync(
        string imageId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Image[imageId].GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method creates a video resource. When using this method, specify the <b>title</b>, <b>size</b>, and <b>classification</b> of the video resource to be created. <b>Description</b> is an optional field for this method.
    /// <b>Tip:</b> See <see href="https://www.ebay.com/help/selling/listings/creating-managing-listings/add-video-to-listing?id=5272#section3">Adding a video to your listing</see> in the eBay Seller Center for details about video formatting requirements and restrictions, or visit the relevant eBay site help pages for the region in which the listings will be posted.
    /// When a video resource is successfully created, the method returns the HTTP Status Code <c>201 Created.</c>The method also returns the location response header containing the <b>video ID</b>, which you can use to retrieve the video.
    /// <b>Note:</b> There is no ability to edit metadata on videos at this time. There is also no method to delete videos.
    /// To upload a created video to a created video resource, use the <see href="/api-docs/commerce/media/resources/video/methods/uploadVideo">uploadVideo</see> method.
    /// Important! </b>"&gt;<b>Important!</b>All POST methods in the Media API, including this method, are subject to short-duration rate limits at the user level: 50 requests per 5 seconds.
    /// </summary>
    public Task CreateVideoAsync(
        CreateVideoRequest body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Video.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method retrieves a video's metadata and content given a specified <b>video ID</b>. The method returns the <b>title</b>, <b>size</b>, <b>classification</b>, <b>description</b>, <b>video ID</b>, <b>playList</b>, <b>status</b>, <b>status message</b> (if any), <b>expiration date</b>, and <b>thumbnail</b> image of the retrieved video.
    /// The video's <b>title</b>, <b>size</b>, <b>classification</b>, and <b>description</b> are set using the <see href="/api-docs/commerce/media/resources/video/methods/createVideo">createVideo</see> method.
    /// The video's <b>playList</b> contains two URLs that link to instances of the streaming video based on the supported protocol.
    /// The <b>status</b> field contains the current status of the video. After a video upload is successfully completed, the video's <b>status</b> will show as <c>PROCESSING</c> until the video reaches one of the terminal states of <c>LIVE</c>, <c>BLOCKED</c> or <c>PROCESSING_FAILED</c>.
    /// If a video's processing fails, it could be because the file is corrupted, is too large, or its size doesn't match what was provided in the metadata. Refer to the error messages to determine the cause of the video's failure to upload.
    /// The <b>status message</b> will indicate why a video was blocked from uploading.
    /// If a video is not being used on an active listing, its <b>expiration date</b> is automatically set to 30 days after the video's initial upload.
    /// The video's <b>thumbnail</b> image is automatically generated when the video is created.
    /// </summary>
    public Task<Video?> GetVideoAsync(
        string videoId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Video[videoId].GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method associates the specified file with the specified <b>video ID</b> and uploads the input file. After the file has been uploaded the processing of the file begins.
    /// <b>Note:</b> The size of the video to be uploaded must exactly match the size of the video's input stream that was set in the <see href="/api-docs/commerce/media/resources/video/methods/createVideo">createVideo</see> method. If the sizes do not match, the video will not upload successfully.
    /// When a video is successfully uploaded, it returns the HTTP Status Code <c>200 OK</c>.
    /// The status flow is <c>PENDING_UPLOAD</c> &gt; <c>PROCESSING</c> &gt; <c>LIVE</c>, <c>PROCESSING_FAILED</c>, or <c>BLOCKED</c>. After a video upload is successfully completed, the status will show as <c>PROCESSING</c> until the video reaches one of the terminal states of <c>LIVE</c>, <c>BLOCKED</c>, or <c>PROCESSING_FAILED</c>. If the size information (in bytes) provided is incorrect, the API will throw an error.
    /// <b>Tip:</b> See <see href="https://www.ebay.com/help/selling/listings/creating-managing-listings/add-video-to-listing?id=5272#section3">Adding a video to your listing</see> in the eBay Seller Center for details about video formatting requirements and restrictions, or visit the relevant eBay site help pages for the region in which the listings will be posted.
    /// To retrieve an uploaded video, use the <see href="/api-docs/commerce/media/resources/video/methods/getVideo">getVideo</see> method.
    /// Important! </b>"&gt;<b>Important!</b>All POST methods in the Media API, including this method, are subject to short-duration rate limits at the user level: 50 requests per 5 seconds.
    /// </summary>
    public Task<Stream?> UploadVideoAsync(
        string videoId,
        Stream body,
        string? contentLength = null,
        string? contentRange = null,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Video[videoId].Upload.PostAsync(body, cfg =>
        {
            if (contentLength is not null)
            {
                cfg.Headers.TryAdd("Content-Length", contentLength);
            }
            if (contentRange is not null)
            {
                cfg.Headers.TryAdd("Content-Range", contentRange);
            }
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method stages a document to be uploaded, and requires the type of document to be uploaded, and the language(s) that the document contains. A successful call returns a <b>documentId</b> value that is then used as a path parameter in an <see href="/api-docs/commerce/media/resources/document/methods/uploadDocument">uploadDocument</see> call.
    /// When a document is successfully created, the method returns the HTTP Status Code <c>201 Created.</c> The method returns <b>documentId</b> in the response payload, which you can use to retrieve the document resource. This ID is also returned in the <b>location</b> header, for convenience.
    /// Important! </b>"&gt;<b>Important!</b> Make sure to capture the document ID value returned in the response payload. This value is required to use the other methods in the <b>document</b> resource, and also needed to associate a document to a listing using the Trading and Inventory APIs.
    /// To upload a created document, use the document ID returned from this method's response with the <see href="/api-docs/commerce/media/resources/document/methods/uploadDocument">uploadDocument</see> method. See <see href="/api-docs/sell/static/inventory/managing-document-media.html">Managing documents</see> for information on creating, uploading, and adding documents to listings.
    /// Important! </b>"&gt;<b>Important!</b>All POST methods in the Media API, including this method, are subject to short-duration rate limits at the user level: 50 requests per 5 seconds.
    /// </summary>
    public Task<CreateDocumentResponse?> CreateDocumentAsync(
        CreateDocumentRequest body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Document.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method downloads a document from the provided URL and adds that document to the user's account. This method requires the URL of the document, the type of document to be uploaded, and the language(s) that the document contains.
    /// When a document is successfully created, the method returns the HTTP Status Code <c>201 Created.</c> The method returns <b>documentId</b> in the response payload, which you can use to retrieve the document resource. This ID is also returned in the <b>location</b> header, for convenience.
    /// Important! </b>"&gt;<b>Important!</b> Make sure to capture the document ID value returned in the response payload. This value is required to use the other methods in the <b>document</b> resource, and also needed to associate a document to a listing using the Trading and Inventory APIs.
    /// After creating a document using this method, a <see href="/api-docs/commerce/media/resources/document/methods/getDocument">getDocument</see> call should be made to check for a <b>documentStatus</b> of <c>ACCEPTED</c>. Only documents with this status can be added to a listing. See <see href="/api-docs/sell/static/inventory/managing-document-media.html">Managing documents</see> for more information on creating, uploading, and adding documents to listings.
    /// Important! </b>"&gt;<b>Important!</b>All POST methods in the Media API, including this method, are subject to short-duration rate limits at the user level: 50 requests per 5 seconds.
    /// </summary>
    public Task<CreateDocumentResponse?> CreateDocumentFromUrlAsync(
        CreateDocumentFromUrlRequest body,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Document.Create_document_from_url.PostAsync(body, cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method retrieves the current <b>status</b> and metadata of the specified document.
    /// Important! </b>"&gt;<b>Important!</b> The document ID value returned in the response payload of the <see href="/api-docs/commerce/media/resources/document/methods/createDocument">createDocument</see> method is a required input path parameter for this method.
    /// See <see href="/api-docs/sell/static/inventory/managing-document-media.html">Managing documents</see> for additional information.
    /// </summary>
    public Task<DocumentResponse?> GetDocumentAsync(
        string documentId,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () => _kiota.Document[documentId].GetAsync(cfg =>
        {
            RestInvocation.ApplyHeaders(cfg.Headers, _config, requestOptions);
        }, ct));

    /// <summary>
    /// This method associates the specified file with the specified document ID and uploads the input file. After the file has been uploaded, the processing of the file begins. Supported file types include .PDF, .JPEG/.JPG, and .PNG, with a maximum file size of 10 MB (10485760 bytes).
    /// <b>Note:</b> Animated and multi-page PNG files are not currently supported.
    /// <b>Note:</b> The document ID value returned in the response of the <see href="/api-docs/commerce/media/resources/document/methods/createDocument">createDocument</see> method is a required input path parameter for this method. This value is also returned in the <b>location</b> header of the <b>createDocument</b> response payload.
    /// A successful upload returns the HTTP Status Code <c>200 OK</c>.
    /// See <see href="/api-docs/sell/static/inventory/managing-document-media.html">Managing documents</see> for additional information.
    /// <b>Note:</b> You must use a <b>Content-Type</b> header with its value set to <b>multipart/form-data</b>.
    /// This call does not have a JSON Request payload but uploads the file as form-data. For example:
    /// file: @"/C:/Users/.../drone_user_warranty.pdf"
    /// See <see href="/api-docs/commerce/media/resources/document/methods/uploadDocument#h2-samples">Samples</see> for information.
    /// Important! </b>"&gt;<b>Important!</b>All POST methods in the Media API, including this method, are subject to short-duration rate limits at the user level: 50 requests per 5 seconds.
    /// </summary>
    public Task<DocumentResponse?> UploadDocumentAsync(
        string documentId,
        Stream fileContent,
        string fileName,
        string? accessToken = null,
        RequestOptions? requestOptions = null,
        CancellationToken ct = default)
        => RestInvocation.CallAsync(
            _tokenContext,
            accessToken,
            () =>
            {
                var requestInfo = _kiota.Document[documentId].Upload.ToPostRequestInformation();
                RestInvocation.ApplyHeaders(requestInfo.Headers, _config, requestOptions);
                return RestInvocation.SendMultipartFileAsync(
                    _adapter,
                    requestInfo,
                    "file",
                    fileContent,
                    fileName,
                    DocumentResponse.CreateFromDiscriminatorValue,
                    ct);
            });
}
