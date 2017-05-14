using Ase.Shared;
using Ase.Shared.AzureStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Ase.Tests
{
    [TestClass]
    public class AzureTableStoragePetIDMappingStorageServiceTests
    {
        private static string ShelterID => ConfigurationManager.AppSettings["PetfinderShelterID"];
        private static string ConnectionString => ConfigurationManager.ConnectionStrings["AzureStorage"].ConnectionString;

        private PetIDMapping[] mappingData = new[]
        {
            new PetIDMapping { PetPointReferenceNumber = "A", PetfinderID = 1 },
            new PetIDMapping { PetPointReferenceNumber = "C", PetfinderID = 3 },
            new PetIDMapping { PetPointReferenceNumber = "B", PetfinderID = 2 },
            new PetIDMapping { PetPointReferenceNumber = "E", PetfinderID = 5 },
            new PetIDMapping { PetPointReferenceNumber = "D", PetfinderID = 4 },
            new PetIDMapping { PetPointReferenceNumber = "G", PetfinderID = 7 },
            new PetIDMapping { PetPointReferenceNumber = "F", PetfinderID = 6 },
            new PetIDMapping { PetPointReferenceNumber = "I", PetfinderID = 9 },
            new PetIDMapping { PetPointReferenceNumber = "H", PetfinderID = 8 }
        };

        private IPetIDMappingStorageService service;

        public AzureTableStoragePetIDMappingStorageServiceTests()
        {
            service = new AzureTableStoragePetIDMappingStorageService(ShelterID, ConnectionString, "PetIDMappings");
        }

        [TestMethod]
        public async Task Upserts()
        {
            await service.Upsert(mappingData);
        }

        [TestMethod]
        public async Task GetsPetfinderIDs()
        {
            await service.Upsert(mappingData);

            var actualMappings = await service.GetMappings(new[] { "H", "H2", "I", "F", "F2", "G", "D", "E", "E2" });
            CollectionAssert.AreEquivalent(new[]
            {
                new PetIDMapping { PetPointReferenceNumber = "H", PetfinderID = 8 },
                new PetIDMapping { PetPointReferenceNumber = "I", PetfinderID = 9 },
                new PetIDMapping { PetPointReferenceNumber = "F", PetfinderID = 6 },
                new PetIDMapping { PetPointReferenceNumber = "G", PetfinderID = 7 },
                new PetIDMapping { PetPointReferenceNumber = "D", PetfinderID = 4 },
                new PetIDMapping { PetPointReferenceNumber = "E", PetfinderID = 5 }
            }, actualMappings.ToArray());
        }
    }
}
