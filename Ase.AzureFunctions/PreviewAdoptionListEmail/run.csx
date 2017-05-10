#r "..\Shared\Ase.Shared.dll"

using Ase.Shared.AzureStorage;
using Ase.Shared.AdoptionList;
using Ase.Shared.PetPoint;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http.Headers;

private static string PetfinderShelterID => ConfigurationManager.AppSettings["PetfinderShelterID"];
private static string PetPointAuthKey => ConfigurationManager.AppSettings["PetPointAuthKey"];
private static string PetPointShelterID => ConfigurationManager.AppSettings["PetPointShelterID"];
private static string PetPointPassword => ConfigurationManager.AppSettings["PetPointPassword"];
private static string PetPointUsername => ConfigurationManager.AppSettings["PetPointUsername"];
private static string AzureStorageConnectionString => ConfigurationManager.AppSettings["AzureWebJobsStorage"];
private static string ApiBaseUrl => ConfigurationManager.AppSettings["ApiBaseUrl"];
private static string HeaderLogoUrl => ConfigurationManager.AppSettings["HeaderLogoUrl"];

private static async Task<Model> CreateModel(DateTime date)
{
    using (var adoptedPetsService = new PetPointAdoptedPetsService(PetPointAuthKey))
    using (var petInfoService = new PetPointPetInfoService(PetPointShelterID))
    {
        var petIDMappingService = new AzureTableStoragePetIDMappingStorageService(
            PetfinderShelterID, AzureStorageConnectionString, "PetIDMappings");
        var modelFactory = new ModelFactory(adoptedPetsService, petInfoService, petIDMappingService);

        await petInfoService.LogIn(PetPointUsername, PetPointPassword);
        return await modelFactory.CreateModel(date);
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

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, DateTime date, TraceWriter log)
{
    Model model = await CreateModel(date);
    string htmlBody = GenerateHtmlBody(model);

    HttpContent content = new StringContent(htmlBody);
    content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

    return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
}
