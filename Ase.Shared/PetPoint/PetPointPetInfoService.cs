using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ase.Shared.PetPoint
{
    public class PetPointPetInfoService : IPetInfoService, IDisposable
    {
        private string shelterID;
        private HttpClientHandler handler;
        private HttpClient client;

        public PetPointPetInfoService(string shelterID)
        {
            this.shelterID = shelterID;

            handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
            client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://sms.petpoint.com/sms3/")
            };
        }

        public async Task LogIn(string username, string password)
        {
            using (HttpResponseMessage response = await client.PostAsync("forms/signinout.aspx", new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("hetvege", Guid.NewGuid().ToString()),
                new KeyValuePair<string, string>("__VIEWSTATE", string.Empty),
                new KeyValuePair<string, string>("ctl00$cphSearchArea$txtShelterPetFinderId", shelterID),
                new KeyValuePair<string, string>("ctl00$cphSearchArea$txtUserName", username),
                new KeyValuePair<string, string>("ctl00$cphSearchArea$txtPassword", password),
                new KeyValuePair<string, string>("ctl00$cphSearchArea$btn_SignIn", "Sign+in...")
            })))
            {
                if (response.StatusCode != HttpStatusCode.Found)
                    throw new Exception("Expected HTTP 302 (Found) response.");

                if (!new Uri("/sms3/default.aspx", UriKind.Relative).Equals(response.Headers.Location))
                    throw new Exception("Expected HTTP redirect to `/sms3/default.aspx`.");
            }
        }

        public async Task<IPetInfo> GetPetInfo(int id)
        {
            using (HttpResponseMessage response = await client.GetAsync($"embeddedreports/animalviewreport.aspx?AnimalID={id}"))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("Expected HTTP 200 (OK) response.");

                string content = await response.Content.ReadAsStringAsync();
                return PetPointPetInfo.Create(content);
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
