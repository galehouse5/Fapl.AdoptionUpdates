using Fapl.AdoptionUpdates.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Fapl.AdoptionUpdates.Tests
{
    [TestClass]
    public class PetIDMappingTests
    {
        [TestMethod]
        public void CreatesFromSimplePetPointReferenceNumber()
        {
            var mappings1 = PetIDMapping.Create("13C04119R", 0);
            Assert.AreEqual(1, mappings1.Count());
            Assert.AreEqual("13C04119R", mappings1.Single().PetPointReferenceNumber);

            var mappings2 = PetIDMapping.Create(" 13C04119R ", 0);
            Assert.AreEqual(1, mappings2.Count());
            Assert.AreEqual("13C04119R", mappings2.Single().PetPointReferenceNumber);
        }

        [TestMethod]
        public void CreatesFromPetPointReferenceNumberList()
        {
            var mappings1 = PetIDMapping.Create("17C03065 & 17C03066", 0);
            Assert.AreEqual(2, mappings1.Count());
            Assert.AreEqual("17C03065", mappings1.ElementAt(0).PetPointReferenceNumber);
            Assert.AreEqual("17C03066", mappings1.ElementAt(1).PetPointReferenceNumber);

            var mappings2 = PetIDMapping.Create("17C04144 17C04145 & 17C04142", 0);
            Assert.AreEqual(3, mappings2.Count());
            Assert.AreEqual("17C04144", mappings2.ElementAt(0).PetPointReferenceNumber);
            Assert.AreEqual("17C04145", mappings2.ElementAt(1).PetPointReferenceNumber);
            Assert.AreEqual("17C04142", mappings2.ElementAt(2).PetPointReferenceNumber);
        }
    }
}
