using Fapl.AdoptionUpdates.Shared.AdoptionList;
using Fapl.AdoptionUpdates.Shared.AzureStorage;
using Fapl.AdoptionUpdates.Shared.PetPoint;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Fapl.AdoptionUpdates.Functions
{
    public static class PreviewAdoptionListEmail
    {
        private static string PetfinderShelterID => ConfigurationManager.AppSettings["PetfinderShelterID"];
        private static string PetPointAuthKey => ConfigurationManager.AppSettings["PetPointAuthKey"];
        private static string PetPointShelterID => ConfigurationManager.AppSettings["PetPointShelterID"];
        private static string PetPointPassword => ConfigurationManager.AppSettings["PetPointPassword"];
        private static string PetPointUsername => ConfigurationManager.AppSettings["PetPointUsername"];
        private static string AzureStorageConnectionString => ConfigurationManager.AppSettings["AzureWebJobsStorage"];
        private static string ApiBaseUrl => ConfigurationManager.AppSettings["ApiBaseUrl"];
        private static string HeaderLogoUrl => ConfigurationManager.AppSettings["HeaderLogoUrl"];

        private static async Task<Model> CreateModel(DateTime date, TraceWriter log)
        {
            using (var adoptedPetsService = new PetPointAdoptedPetsService(PetPointAuthKey))
            using (var petInfoService = new PetPointPetInfoService(PetPointShelterID))
            {
                var petIDMappingService = new AzureTableStoragePetIDMappingStorageService(
                    PetfinderShelterID, AzureStorageConnectionString, "PetIDMappings");
                var modelFactory = new ModelFactory(adoptedPetsService, petInfoService, petIDMappingService);

                log.Info("Logging in to PetPoint...");
                await petInfoService.LogIn(PetPointUsername, PetPointPassword);
                return await modelFactory.CreateModel(date, m => log.Info(m));
            }
        }

        private static string GenerateHtmlBody(Model model)
        {
            EmailBuilder builder = new EmailBuilder
            {
                HeaderLogoUrl = HeaderLogoUrl,
                GetPetfinderPhotoUrl = (id, width, height)
                    => $"{ApiBaseUrl}/petfinder-images/{id}/1/generate?width={width}&height={height}",
                GetNoPhotoUrl = (species, width, height)
                    => $"{ApiBaseUrl}/placeholder-images/{species.Replace(" ", null)}/generate?width={width}&height={height}&background-color=e1e1e1"
            };

            return builder.GenerateHtmlBody(model);
        }

        [FunctionName("PreviewAdoptionListEmail")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "adoption-list-email/{date:datetime}/preview")]HttpRequestMessage req,
            DateTime date, TraceWriter log)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Model model = await CreateModel(date, log);
            string htmlBody = GenerateHtmlBody(model);

            HttpContent content = new StringContent(htmlBody);
            content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        }
    }
}
