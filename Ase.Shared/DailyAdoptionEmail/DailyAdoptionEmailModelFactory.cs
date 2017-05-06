using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ase.Shared.DailyAdoptionEmail
{
    public class DailyAdoptionEmailModelFactory
    {
        private IAdoptedPetsService adoptedPetsService;
        private IPetInfoService petInfoService;
        private IPetIDMappingStorageService petIDMappingService;

        public DailyAdoptionEmailModelFactory(
            IAdoptedPetsService adoptedPetsService,
            IPetInfoService petInfoService,
            IPetIDMappingStorageService petIDMappingService)
        {
            this.adoptedPetsService = adoptedPetsService;
            this.petInfoService = petInfoService;
            this.petIDMappingService = petIDMappingService;
        }

        public async Task<DailyAdoptionEmailModel> CreateModel(DateTime date)
        {
            var pets = new List<IPetInfo>();

            foreach (int id in await adoptedPetsService.GetAdoptedPetIDs(date))
            {
                pets.Add(await petInfoService.GetPetInfo(id));
            }

            var mappings = (await petIDMappingService
                .GetMappings(pets.Select(p => p.ReferenceNumber)))
                .ToDictionary(m => m.PetPointReferenceNumber, m => m.PetfinderID);

            return new DailyAdoptionEmailModel
            {
                Date = date,
                Pets = pets.Select(p => new DailyAdoptionEmailModel.Pet
                {
                    Species = p.Species,
                    Name = p.Name,
                    PetfinderID = mappings.ContainsKey(p.ReferenceNumber) ?
                        mappings[p.ReferenceNumber] : (int?)null,
                    Age = DateTime.UtcNow - p.Dob,
                    Stay = p.GetLastStayLength(date.AddDays(1))
                }).ToArray()
            };
        }
    }
}
