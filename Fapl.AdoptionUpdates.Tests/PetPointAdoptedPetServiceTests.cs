using Fapl.AdoptionUpdates.Shared.PetPoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Fapl.AdoptionUpdates.Tests
{
    [TestClass]
    public class PetPointAdoptedPetServiceTests
    {
        private static string AuthKey => ConfigurationManager.AppSettings["PetPointAuthKey"];

        [TestMethod]
        public async Task GetsAdoptedPetIDs()
        {
            using (var service = new PetPointAdoptedPetsService(AuthKey))
            {
                var ids = await service.GetAdoptedPetIDs(new DateTime(2017, 5, 5));

                Assert.AreNotEqual(0, ids.Count);
            }
        }
    }
}
