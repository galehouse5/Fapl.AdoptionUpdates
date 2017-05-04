using System.Threading.Tasks;

namespace Ase.Shared
{
    public interface IPetInfoService
    {
        Task<IPetInfo> GetPetInfo(int id);
    }
}
