using Ase.Shared.PetPoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Ase.Tests
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
