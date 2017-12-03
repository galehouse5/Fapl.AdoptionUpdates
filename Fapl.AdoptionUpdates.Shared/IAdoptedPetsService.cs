using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fapl.AdoptionUpdates.Shared
{
    public interface IAdoptedPetsService
    {
        Task<IReadOnlyCollection<int>> GetAdoptedPetIDs(DateTime date);
    }
}
