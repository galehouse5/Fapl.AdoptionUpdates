#r "..\Shared\Ase.Shared.dll"
#r "System.Drawing"

using Ase.Shared.Images;
using Ase.Shared.Petfinder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

private static readonly CacheControlHeaderValue CacheControl = new CacheControlHeaderValue
{
    Public = true,
    MaxAge = TimeSpan.FromDays(365)
};

private static HttpResponseMessage CreateNotFoundResponse()
{
    HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.NotFound);
    message.Headers.CacheControl = CacheControl;
    return message;
}

private static HttpResponseMessage CreateOKResponse(Stream data)
{
    HttpContent content = new StreamContent(data);
    content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

    HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    message.Headers.CacheControl = CacheControl;

    return message;
}

public static async Task<HttpResponseMessage> Run(
    HttpRequestMessage request, int petID, TraceWriter log)
{
    int? innerMargin = request.GetQueryNameValuePairs()
        .Where(q => q.Key.Equals("inner-margin", StringComparison.OrdinalIgnoreCase))
        .Select(q => (int?)int.Parse(q.Value)).FirstOrDefault();
    Color? innerBackgroundColor = request.GetQueryNameValuePairs()
        .Where(q => q.Key.Equals("inner-background-color", StringComparison.OrdinalIgnoreCase))
        .Select(q => (Color?)ColorTranslator.FromHtml($"#{q.Value}")).FirstOrDefault();
    int? width = request.GetQueryNameValuePairs()
        .Where(q => q.Key.Equals("width", StringComparison.OrdinalIgnoreCase))
        .Select(q => (int?)int.Parse(q.Value)).FirstOrDefault();
    int? height = request.GetQueryNameValuePairs()
        .Where(q => q.Key.Equals("height", StringComparison.OrdinalIgnoreCase))
        .Select(q => (int?)int.Parse(q.Value)).FirstOrDefault();

    using (var service = new PetfinderPetImageRetrievalService())
    {
        var imageData = await service.GetImageData(petID);
        if (!imageData.Any()) return CreateNotFoundResponse();

        Stream responseData = null;

        try
        {
            imageData.ProcessImageData(images =>
            {
                using (Image combined = images.CombineImages(
                    RecursiveShrinkingImageLayout.Generate,
                    margin: innerMargin ?? 0f,
                    backgroundColor: innerBackgroundColor))
                using (Image resized = combined.ResizeImageToCover(width, height))
                {
                    responseData = new MemoryStream();
                    resized.Save(responseData, ImageFormat.Jpeg);
                    responseData.Position = 0;
                }
            });

            return CreateOKResponse(responseData);
        }
        catch (Exception)
        {
            responseData?.Dispose();
            throw;
        }
    }
}
