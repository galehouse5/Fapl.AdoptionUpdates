﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ase.Shared
{
    public interface IPetImageRetrievalService
    {
        Task<IReadOnlyCollection<byte[]>> GetImageData(int petfinderPetID);
    }
}
