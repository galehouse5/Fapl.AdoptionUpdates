using Fapl.AdoptionUpdates.Shared;
using Fapl.AdoptionUpdates.Shared.PetPoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Fapl.AdoptionUpdates.Tests
{
    [TestClass]
    public class PetPointPetInfoServiceTests
    {
        private static string ShelterID => ConfigurationManager.AppSettings["PetPointShelterID"];
        private static string Username => ConfigurationManager.AppSettings["PetPointUsername"];
        private static string Password => ConfigurationManager.AppSettings["PetPointPassword"];

        [TestMethod]
        public async Task LogsIn()
        {
            using (var service = new PetPointPetInfoService(ShelterID))
            {
                await service.LogIn(Username, Password);
            }
        }

        [TestMethod]
        public async Task GetsPetInfo()
        {
            using (var service = new PetPointPetInfoService(ShelterID))
            {
                await service.LogIn(Username, Password);

                IPetInfo info = await service.GetPetInfo(29251923);
                Assert.IsNotNull(info);
                Assert.AreEqual(29251923, info.ID);
                Assert.AreEqual("17D03171", info.ReferenceNumber);
                Assert.AreEqual("Cheyanne", info.Name);
                Assert.AreEqual("Dog", info.Species);
                Assert.AreEqual("Hound/Bulldog, American", info.Breed);
                Assert.AreEqual(new DateTime(2013, 7, 31), info.Dob);
                Assert.AreEqual(30, info.GetLastStayLength(new DateTime(2017, 4, 30))?.Days);
            }
        }
    }
}
