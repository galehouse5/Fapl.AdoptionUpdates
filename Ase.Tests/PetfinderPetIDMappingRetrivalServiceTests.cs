using Ase.Shared.Petfinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Threading.Tasks;

namespace Ase.Tests
{
    [TestClass]
    public class PetfinderPetIDMappingRetrivalServiceTests
    {
        private static string ApiKey => ConfigurationManager.AppSettings["PetfinderApiKey"];
        private static string ShelterID => ConfigurationManager.AppSettings["PetfinderShelterID"];

        [TestMethod]
        public async Task GetsCurrentPetIDMappings()
        {
            using (var service = new PetfinderPetIDMappingRetrievalService(ApiKey, ShelterID))
            {
                var mappings = await service.GetCurrentPetIDMappings();

                Assert.IsNotNull(mappings);
                Assert.AreNotEqual(0, mappings.Count);
            }
        }
    }
}
