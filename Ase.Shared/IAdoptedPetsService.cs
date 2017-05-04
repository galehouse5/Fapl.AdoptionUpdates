using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ase.Shared
{
    public interface IAdoptedPetsService
    {
        Task<IReadOnlyCollection<int>> GetAdoptedPetIDs(DateTime date);
    }
}
