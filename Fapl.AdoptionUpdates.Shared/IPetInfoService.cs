using System.Threading.Tasks;

namespace Fapl.AdoptionUpdates.Shared
{
    public interface IPetInfoService
    {
        Task<IPetInfo> GetPetInfo(int id);
    }
}
