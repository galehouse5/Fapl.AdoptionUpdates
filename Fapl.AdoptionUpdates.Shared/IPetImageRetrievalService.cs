using System.Threading.Tasks;

namespace Fapl.AdoptionUpdates.Shared
{
    public interface IPetImageRetrievalService
    {
        Task<byte[]> GetImageData(int petfinderPetID, int imageNumber);
    }
}
