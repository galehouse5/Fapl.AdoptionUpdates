using Ase.Shared.AzureStorage;
using Ase.Shared.DailyAdoptionEmail;
using Ase.Shared.PetPoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Linq;
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
        public async Task GetsDailyAdoptionEmailModel()
        {
            using (var adoptedPetsService = new PetPointAdoptedPetsService(PetPointAuthKey))
            using (var petInfoService = new PetPointPetInfoService(PetPointShelterID))
            {
                var petIDMappingService = new AzureTableStoragePetIDMappingStorageService(
                    PetfinderShelterID, AzureStorageConnectionString, "PetIDMappings");
                var factory = new DailyAdoptionEmailModelFactory(adoptedPetsService, petInfoService, petIDMappingService);

                await petInfoService.LogIn(PetPointUsername, PetPointPassword);
                var model = await factory.CreateModel(new DateTime(2017, 5, 5));

                Assert.IsNotNull(model);
                Assert.AreNotEqual(0, model.Pets);
                Assert.IsTrue(model.Pets.Any(p => p.PetfinderID.HasValue));
            }
        }
    }
}
