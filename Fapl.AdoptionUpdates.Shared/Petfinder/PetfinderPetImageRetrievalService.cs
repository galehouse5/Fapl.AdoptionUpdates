using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fapl.AdoptionUpdates.Shared.Petfinder
{
    public class PetfinderPetImageRetrievalService : IPetImageRetrievalService, IDisposable
    {
        private HttpClientHandler handler;
        private HttpClient client;

        public PetfinderPetImageRetrievalService()
        {
            handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
            client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://photos.petfinder.com/photos/pets/")
            };
        }

        public async Task<byte[]> GetImageData(int petfinderPetID, int imageNumber)
        {
            using (HttpResponseMessage response = await client.GetAsync($"{petfinderPetID}/{imageNumber}/"))
            {
                if (response.StatusCode == HttpStatusCode.UnsupportedMediaType)
                    return null;

                if (response.StatusCode == HttpStatusCode.OK)
                    return await response.Content.ReadAsByteArrayAsync();

                throw new Exception("Expected HTTP 200 (OK) or 415 (Unsupported Media Type) response.");
            }
        }

        public void Dispose()
        {
            handler?.Dispose();
            handler = null;

            client?.Dispose();
            client = null;
        }
    }
}
