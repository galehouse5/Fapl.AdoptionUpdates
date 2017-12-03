using Fapl.AdoptionUpdates.Shared.AzureStorage;
using Fapl.AdoptionUpdates.Shared.Petfinder;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Fapl.AdoptionUpdates.Functions
{
    public static class StorePetIDMappings
    {
        private static string PetfinderApiKey => ConfigurationManager.AppSettings["PetfinderApiKey"];
        private static string PetfinderShelterID => ConfigurationManager.AppSettings["PetfinderShelterID"];
        private static string AzureStorageConnectionString => ConfigurationManager.AppSettings["AzureWebJobsStorage"];

        [FunctionName("StorePetIDMappings")]
        public static async Task Run([TimerTrigger("0 0 */1 * * *")]TimerInfo myTimer, TraceWriter log)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var retrievalService = new PetfinderPetIDMappingRetrievalService(PetfinderApiKey, PetfinderShelterID))
            {
                var storageService = new AzureTableStoragePetIDMappingStorageService(PetfinderShelterID, AzureStorageConnectionString, "PetIDMappings");

                log.Info($"Retrieving current mappings...");
                var mappings = await retrievalService.GetCurrentPetIDMappings();

                log.Info($"Storing {mappings.Count()} mappings...");
                await storageService.Upsert(mappings);
            }
        }
    }
}
