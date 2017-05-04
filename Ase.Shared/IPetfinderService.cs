using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ase.Shared
{
    public interface IPetfinderService
    {
        Task<IReadOnlyCollection<Tuple<int, string>>> GetAdoptablePetReferenceNumbers(string shelterID);
    }
}
