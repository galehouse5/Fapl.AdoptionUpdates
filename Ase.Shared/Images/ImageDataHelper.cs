using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Ase.Shared.Images
{
    public static class ImageDataHelper
    {
        public static void ProcessImageData(this IEnumerable<byte[]> imageData,
            Action<IReadOnlyCollection<Image>> processor)
        {
            var disposables = new List<IDisposable>();

            try
            {
                var images = new List<Image>();

                foreach (var data in imageData)
                {
                    var stream = new MemoryStream(data);
                    disposables.Add(stream);

                    Image image = new Bitmap(stream);
                    disposables.Add(image);

                    images.Add(image);
                }

                processor(images);
            }
            finally
            {
                foreach (IDisposable disposable in disposables)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
