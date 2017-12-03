using Fapl.AdoptionUpdates.Shared.Images;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Fapl.AdoptionUpdates.Functions
{
    public static class GeneratePlaceholderImage
    {
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

        [FunctionName("GeneratePlaceholderImage")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "placeholder-images/{species}/generate")]HttpRequestMessage req,
            string species, TraceWriter log)
        {
            ImagePlaceholder placeholder;
            if (!Enum.TryParse<ImagePlaceholder>(species, true, out placeholder))
                return CreateNotFoundResponse();

            Color? backgroundColor = req.GetQueryNameValuePairs()
                .Where(q => q.Key.Equals("background-color", StringComparison.OrdinalIgnoreCase))
                .Select(q => (Color?)ColorTranslator.FromHtml($"#{q.Value}")).FirstOrDefault();
            int? width = req.GetQueryNameValuePairs()
                .Where(q => q.Key.Equals("width", StringComparison.OrdinalIgnoreCase))
                .Select(q => (int?)int.Parse(q.Value)).FirstOrDefault();
            int? height = req.GetQueryNameValuePairs()
                .Where(q => q.Key.Equals("height", StringComparison.OrdinalIgnoreCase))
                .Select(q => (int?)int.Parse(q.Value)).FirstOrDefault();

            using (var pass1 = placeholder.GenerateImage(backgroundColor ?? ColorTranslator.FromHtml("#222222")))
            using (var pass2 = pass1.ResizeImageToCover(width, height))
            {
                Stream responseData = null;

                try
                {
                    responseData = new MemoryStream();
                    pass2.Save(responseData, ImageFormat.Jpeg);
                    responseData.Position = 0;

                    return CreateOKResponse(responseData);
                }
                catch (Exception)
                {
                    responseData?.Dispose();
                    throw;
                }
            }
        }
    }
}
