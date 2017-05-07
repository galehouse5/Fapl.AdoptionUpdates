using Ase.Shared.AzureStorage;
using Ase.Shared.DailyAdoptionEmail;
using Ase.Shared.PetPoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ase.Tests
{
    [TestClass]
    public class DailyAdoptionEmailTests
    {
        private static string PetPointAuthKey => ConfigurationManager.AppSettings["PetPointAuthKey"];
        private static string PetPointShelterID => ConfigurationManager.AppSettings["PetPointShelterID"];
        private static string PetPointUsername => ConfigurationManager.AppSettings["PetPointUsername"];
        private static string PetPointPassword => ConfigurationManager.AppSettings["PetPointPassword"];
        private static string PetfinderShelterID => ConfigurationManager.AppSettings["PetfinderShelterID"];
        private static string AzureStorageConnectionString => ConfigurationManager.ConnectionStrings["AzureStorage"].ConnectionString;

        [TestMethod]
        public async Task CreatesModel()
        {
            using (var adoptedPetsService = new PetPointAdoptedPetsService(PetPointAuthKey))
            using (var petInfoService = new PetPointPetInfoService(PetPointShelterID))
            {
                var petIDMappingService = new AzureTableStoragePetIDMappingStorageService(
                    PetfinderShelterID, AzureStorageConnectionString, "PetIDMappings");
                var factory = new DailyAdoptionModelFactory(adoptedPetsService, petInfoService, petIDMappingService);

                await petInfoService.LogIn(PetPointUsername, PetPointPassword);
                var model = await factory.CreateModel(new DateTime(2017, 5, 5));

                Assert.IsNotNull(model);
                Assert.AreNotEqual(0, model.Pets);
                Assert.IsTrue(model.Pets.Any(p => p.PetfinderID.HasValue));
            }
        }

        [TestMethod]
        public void GeneratesHtmlBody()
        {
            using (var data = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Ase.Tests.DailyAdoptionModel.json"))
            using (TextReader reader = new StreamReader(data))
            {
                DailyAdoptionModel model = JsonConvert.DeserializeObject<DailyAdoptionModel>(reader.ReadToEnd());
                DailyAdoptionEmail email = new DailyAdoptionEmail
                {
                    HeaderLogoUrl = "https://asestg.azureedge.net/public/friendship-apl-logo.png",
                    GetPetfinderPhotoUrl = (id, width, height)
                        => $"https://ase-fns.azureedge.net/api/petfinder-pets/{id}/image?width={width}&height={height}",
                    GetNoPhotoUrl = (species, width, height)
                        => $"?"
                };

                string htmlBody = email.GenerateHtmlBody(model);

                Assert.IsNotNull(htmlBody);
            }
        }
    }
}
