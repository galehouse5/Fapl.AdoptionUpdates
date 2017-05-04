using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ase.Shared
{
    public interface IPetIDMappingRetrievalService
    {
        Task<IReadOnlyCollection<PetIDMapping>> GetCurrentPetIDMappings();
    }
}
