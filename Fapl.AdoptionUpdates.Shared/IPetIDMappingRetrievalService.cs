using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fapl.AdoptionUpdates.Shared
{
    public interface IPetIDMappingRetrievalService
    {
        Task<IReadOnlyCollection<PetIDMapping>> GetCurrentPetIDMappings();
    }
}
