using Fapl.AdoptionUpdates.Shared;
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
using System.Net.Mail;
using System.Threading.Tasks;

namespace Fapl.AdoptionUpdates.Functions
{
    public static class SendAdoptionListEmail
    {
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

        private static async Task Run(TraceWriter log, DateTime date)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Model model = await CreateModel(date, log);
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

        [FunctionName("SendAdoptionListEmail_Schedule1")]
        public static Task Run_Schedule1([TimerTrigger("0 30 17 * * 1,3,5,6")]TimerInfo myTimer, TraceWriter log)
            => Run(log, DateTime.Today);

        [FunctionName("SendAdoptionListEmail_Schedule2")]
        public static Task Run_Schedule2([TimerTrigger("0 30 19 * * 2,4")]TimerInfo myTimer, TraceWriter log)
            => Run(log, DateTime.Today);

        [FunctionName("SendAdoptionListEmail_Schedule3")]
        public static Task Run_Schedule3([TimerTrigger("0 0 16 * * 0")]TimerInfo myTimer, TraceWriter log)
            => Run(log, DateTime.Today);

        [FunctionName("SendAdoptionListEmail_Force")]
        public static async Task<HttpResponseMessage> Run_Force(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "adoption-list-email/{date:datetime}/force-send")]HttpRequestMessage req,
            DateTime date, TraceWriter log)
        {
            await Run(log, date);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
