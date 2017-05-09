#r "..\Shared\Ase.Shared.dll"
#r "System.Drawing"

using Ase.Shared.Images;
using Ase.Shared.Petfinder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
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
    HttpRequestMessage req, int petID, int imageNumber, TraceWriter log)
{
    int? width = req.GetQueryNameValuePairs()
        .Where(q => q.Key.Equals("width", StringComparison.OrdinalIgnoreCase))
        .Select(q => (int?)int.Parse(q.Value)).FirstOrDefault();
    int? height = req.GetQueryNameValuePairs()
        .Where(q => q.Key.Equals("height", StringComparison.OrdinalIgnoreCase))
        .Select(q => (int?)int.Parse(q.Value)).FirstOrDefault();

    using (var service = new PetfinderPetImageRetrievalService())
    {
        var imageData = await service.GetImageData(petID, imageNumber);
        if (imageData == null) return CreateNotFoundResponse();

        using (Stream imageStream = new MemoryStream(imageData))
        using (Image inputImage = new Bitmap(imageStream))
        using (Image outputImage = inputImage.ResizeImageToCover(width, height))
        {
            Stream responseData = null;

            try
            {
                responseData = new MemoryStream();
                outputImage.Save(responseData, ImageFormat.Jpeg);
                responseData.Position = 0;

                return CreateOKResponse(responseData);
            }
            catch
            {
                responseData?.Dispose();
                throw;
            }
        }
    }
}
