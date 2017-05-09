﻿using Ase.Shared.AzureStorage;
using Ase.Shared.AdoptionList;
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
    public class AdoptionListEmailTests
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
                var factory = new ModelFactory(adoptedPetsService, petInfoService, petIDMappingService);

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
                .GetManifestResourceStream("Ase.Tests.AdoptionListModel.json"))
            using (TextReader reader = new StreamReader(data))
            {
                Model model = JsonConvert.DeserializeObject<Model>(reader.ReadToEnd());
                EmailBuilder email = new EmailBuilder
                {
                    HeaderLogoUrl = "https://asestg.azureedge.net/public/friendship-apl-logo.png",
                    GetPetfinderPhotoUrl = (id, width, height)
                        => $"https://ase-fns.azureedge.net/api/petfinder-images/{id}/1/generate?width={width}&height={height}",
                    GetNoPhotoUrl = (species, width, height)
                        => $"https://ase-fns.azureedge.net/api/placeholder-images/{species}/generate?width={width}&height={height}&background-color=e0e0e0"
                };

                string htmlBody = email.GenerateHtmlBody(model);

                Assert.IsNotNull(htmlBody);
            }
        }
    }
}