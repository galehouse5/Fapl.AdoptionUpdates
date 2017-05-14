#r "..\Shared\Ase.Shared.dll"
#r "System.Configuration"

using Ase.Shared;
using Ase.Shared.AzureStorage;
using Ase.Shared.AdoptionList;
using Ase.Shared.PetPoint;
using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

private static string Host => ConfigurationManager.AppSettings["SmtpHost"];
private static int Port => int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
private static bool EnableSsl => bool.Parse(ConfigurationManager.AppSettings["SmtpEnableSsl"] ?? "true");
private static string UserName => ConfigurationManager.AppSettings["SmtpUserName"];
private static string Password => ConfigurationManager.AppSettings["SmtpPassword"];
private static string FromAddress => ConfigurationManager.AppSettings["AdoptionListFromAddress"];
private static string ToAddress => ConfigurationManager.AppSettings["AdoptionListToAddress"];
private static string BccAddress => ConfigurationManager.AppSettings["AdoptionListBccAddress"];
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

public static async Task Run(TimerInfo myTimer, TraceWriter log)
{
    Model model = await CreateModel(DateTime.Today, log);
    EmailBuilder builder = new EmailBuilder
    {
        HeaderLogoUrl = HeaderLogoUrl,
        GetPetfinderPhotoUrl = (id, width, height)
            => $"{ApiBaseUrl}/petfinder-images/{id}/1/generate?width={width}&height={height}",
        GetNoPhotoUrl = (species, width, height)
            => $"{ApiBaseUrl}/placeholder-images/{species.Replace(" ", null)}/generate?width={width}&height={height}&background-color=e1e1e1"
    };

    using (SmtpClient client = new SmtpClient(Host, Port)
    {
        EnableSsl = EnableSsl,
        Credentials = new NetworkCredential(UserName, Password)
    })
    using (MailMessage message = new MailMessage(FromAddress, ToAddress))
    {
        if (!string.IsNullOrEmpty(BccAddress))
        {
            message.Bcc.Add(BccAddress);
        }

        builder.Populate(message, model);

        log.Info($"Sending email to {ToAddress}...");
        await client.SendMailAsync(message);
    }
}
