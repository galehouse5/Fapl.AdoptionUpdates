using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Ase.Shared.Petfinder
{
    public class PetfinderPetIDMappingRetrievalService : IPetIDMappingRetrievalService, IDisposable
    {
        private string shelterID;
        private string apiKey;
        private HttpClientHandler handler;
        private HttpClient client;
        private XmlSerializer serializer = new XmlSerializer(typeof(petfinder));

        public PetfinderPetIDMappingRetrievalService(string apiKey, string shelterID)
        {
            this.shelterID = shelterID;
            this.apiKey = apiKey;

            handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
            client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.petfinder.com/")
            };
        }

        public async Task<IReadOnlyCollection<PetIDMapping>> GetCurrentPetIDMappings()
        {
            using (HttpResponseMessage response = await client.GetAsync(string.Concat("shelter.getPets",
                $"?key={WebUtility.UrlEncode(apiKey)}",
                $"&id={WebUtility.UrlEncode(shelterID)}",
                // Don't get animals that are on hold, pending adoption, or adopted/removed. We only have access
                // rights to the adoptable animals for now.
                "&offset=0&count=1000&status=A")))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("Expected HTTP 200 (OK) response.");

                string content = await response.Content.ReadAsStringAsync();
                using (TextReader reader = new StringReader(content))
                {
                    petfinder container = (petfinder)serializer.Deserialize(reader);
                    petfinderPetRecordList pets = (petfinderPetRecordList)container.Item;

                    return pets.pet
                        .Where(p => !string.IsNullOrEmpty(p.shelterPetId))
                        .SelectMany(p => PetIDMapping.Create(p.shelterPetId, int.Parse(p.id)))
                        .ToArray();
                }
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
