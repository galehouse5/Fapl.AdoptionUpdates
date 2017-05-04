using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ase.Shared
{
    public interface IPetIDMappingStorageService
    {
        Task<IReadOnlyCollection<PetIDMapping>> GetMappings(IEnumerable<string> petPointReferenceNumbers);
        Task Upsert(IEnumerable<PetIDMapping> mappings);
    }
}
