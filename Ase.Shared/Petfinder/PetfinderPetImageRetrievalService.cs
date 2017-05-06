using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ase.Shared.Petfinder
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

        public async Task<IReadOnlyCollection<byte[]>> GetImageData(int petfinderPetID)
        {
            var imageData = new List<byte[]>();

            for (int i = 1; true; i++)
            {
                using (HttpResponseMessage response = await client.GetAsync($"{petfinderPetID}/{i}/"))
                {
                    if (response.StatusCode == HttpStatusCode.UnsupportedMediaType) break;

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        imageData.Add(await response.Content.ReadAsByteArrayAsync());
                        continue;
                    }

                    throw new Exception("Expected HTTP 200 (OK) or 415 (Unsupported Media Type) response.");
                }
            }

            return imageData;
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
