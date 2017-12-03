using Fapl.AdoptionUpdates.Shared.Images;
using Fapl.AdoptionUpdates.Shared.Petfinder;
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
using System.Threading.Tasks;

namespace Fapl.AdoptionUpdates.Functions
{
    public static class GeneratePetfinderImage
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

        [FunctionName("GeneratePetfinderImage")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "petfinder-images/{petID:int}/{imageNumber:int}/generate")]HttpRequestMessage req,
            int petID, int imageNumber, TraceWriter log)
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
    }
}
