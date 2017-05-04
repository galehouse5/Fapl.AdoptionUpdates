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
        private string key;
        private HttpClientHandler handler;
        private HttpClient client;
        private XmlSerializer serializer = new XmlSerializer(typeof(petfinder));

        public PetfinderPetIDMappingRetrievalService(string key, string shelterID)
        {
            this.shelterID = shelterID;
            this.key = key;

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
                $"?key={WebUtility.UrlEncode(key)}",
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
                        .Select(p => new PetIDMapping
                        {
                            PetfinderID = int.Parse(p.id),
                            PetPointReferenceNumber = p.shelterPetId
                        }).ToArray();
                }
            }
        }

        public void Dispose()
        {
            if (handler != null)
            {
                handler.Dispose();
                handler = null;
            }

            if (client != null)
            {
                client.Dispose();
                client = null;
            }
        }
    }
}
