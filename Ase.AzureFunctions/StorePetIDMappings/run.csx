#r "..\Shared\Ase.Shared.dll"
#r "System.Configuration"

using Ase.Shared.AzureStorage;
using Ase.Shared.Petfinder;
using System;
using System.Configuration;
using System.Linq;

private static string petfinderApiKey => ConfigurationManager.AppSettings["PetfinderApiKey"];
private static string petfinderShelterID => ConfigurationManager.AppSettings["PetfinderShelterID"];
private static string azureStorageConnectionString => ConfigurationManager.AppSettings["AzureWebJobsStorage"];

public static async Task Run(TimerInfo myTimer, TraceWriter log)
{
    using (var retrievalService = new PetfinderPetIDMappingRetrievalService(petfinderApiKey, petfinderShelterID))
    {
        var storageService = new AzureTableStoragePetIDMappingStorageService(petfinderShelterID, azureStorageConnectionString, "PetIDMappings");

        log.Info($"Retrieving current mappings...");
        var mappings = await retrievalService.GetCurrentPetIDMappings();

        log.Info($"Storing {mappings.Count()} mappings...");
        await storageService.Upsert(mappings);
    }
}
