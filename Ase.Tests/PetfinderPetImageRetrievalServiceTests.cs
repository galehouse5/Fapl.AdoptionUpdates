using Ase.Shared.Petfinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Ase.Tests
{
    [TestClass]
    public class PetfinderPetImageRetrievalServiceTests
    {
        [TestMethod]
        public async Task GetsImageData()
        {
            using (var service = new PetfinderPetImageRetrievalService())
            {
                var imageData = await service.GetImageData(37935665, 1);

                Assert.IsNotNull(imageData);
                Assert.AreNotEqual(0, imageData.Length);
            }
        }
    }
}
