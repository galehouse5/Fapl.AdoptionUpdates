using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fapl.AdoptionUpdates.Shared
{
    public interface IPetIDMappingStorageService
    {
        Task<IReadOnlyCollection<PetIDMapping>> GetMappings(IEnumerable<string> petPointReferenceNumbers);
        Task Upsert(IEnumerable<PetIDMapping> mappings);
    }
}
