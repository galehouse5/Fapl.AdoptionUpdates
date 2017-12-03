using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace Fapl.AdoptionUpdates.Shared.PetPoint
{
    public class PetPointAdoptedPetsService : IAdoptedPetsService, IDisposable
    {
        private HttpClientHandler handler;
        private HttpClient client;
        private string authKey;

        public PetPointAdoptedPetsService(string authKey)
        {
            this.authKey = authKey;

            handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
            client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://ws.petango.com/webservices/wsadoption.asmx/")
            };
        }

        public async Task<IReadOnlyCollection<int>> GetAdoptedPetIDs(DateTime date)
        {
            var petIDs = new List<int>();

            using (HttpResponseMessage response = await client.PostAsync("AdoptionList", new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("authKey", authKey),
                new KeyValuePair<string, string>("adoptionDate", date.ToString("yyyy-MM-dd")),
                new KeyValuePair<string, string>("siteID", string.Empty)
            })))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception("Expected HTTP 200 (OK) response.");

                using (Stream data = await response.Content.ReadAsStreamAsync())
                using (XmlReader reader = XmlReader.Create(data))
                {
                    while (reader.Read())
                    {
                        if (reader.Name.Equals("AnimalID"))
                        {
                            petIDs.Add(int.Parse(reader.ReadInnerXml()));
                        }
                    }
                }
            }

            return petIDs;
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
