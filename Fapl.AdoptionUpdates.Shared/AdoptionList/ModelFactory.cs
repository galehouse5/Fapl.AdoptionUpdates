using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fapl.AdoptionUpdates.Shared.AdoptionList
{
    public class ModelFactory
    {
        private IAdoptedPetsService adoptedPetsService;
        private IPetInfoService petInfoService;
        private IPetIDMappingStorageService petIDMappingService;

        public ModelFactory(
            IAdoptedPetsService adoptedPetsService,
            IPetInfoService petInfoService,
            IPetIDMappingStorageService petIDMappingService)
        {
            this.adoptedPetsService = adoptedPetsService;
            this.petInfoService = petInfoService;
            this.petIDMappingService = petIDMappingService;
        }

        public async Task<Model> CreateModel(DateTime date, Action<string> logger = null)
        {
            var pets = new List<IPetInfo>();

            logger?.Invoke("Getting adopted pet IDs...");
            foreach (int id in await adoptedPetsService.GetAdoptedPetIDs(date))
            {
                logger?.Invoke($"Getting info for pet {id}...");
                pets.Add(await petInfoService.GetPetInfo(id));
            }

            logger?.Invoke("Getting ID mappings for adopted pets...");
            var mappings = (await petIDMappingService
                .GetMappings(pets.Select(p => p.ReferenceNumber)))
                .ToDictionary(m => m.PetPointReferenceNumber, m => m.PetfinderID);

            return new Model
            {
                Date = date,
                Pets = (from pet in pets
                        let stay = pet.GetLastStayLength(date.AddDays(1))
                        orderby pet.Name, stay descending
                        select new Model.Pet
                        {
                            Species = pet.Species,
                            Name = pet.Name,
                            PetfinderID = mappings.ContainsKey(pet.ReferenceNumber) ?
                                 mappings[pet.ReferenceNumber] : (int?)null,
                            Age = DateTime.UtcNow - pet.Dob,
                            Stay = pet.GetLastStayLength(date.AddDays(1))
                        }).ToArray()
            };
        }
    }
}
