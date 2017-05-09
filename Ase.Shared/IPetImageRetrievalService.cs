using System.Threading.Tasks;

namespace Ase.Shared
{
    public interface IPetImageRetrievalService
    {
        Task<byte[]> GetImageData(int petfinderPetID, int imageNumber);
    }
}
