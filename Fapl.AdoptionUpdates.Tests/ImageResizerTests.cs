using Fapl.AdoptionUpdates.Shared.Images;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

namespace Fapl.AdoptionUpdates.Tests
{
    [TestClass]
    public class ImageResizerTests
    {
        [TestMethod]
        public void ResizesSquareImageToCover()
        {
            using (var image = new Bitmap(8, 8))
            {
                using (var resized = ImageResizer.ResizeImageToCover(image, width: 4))
                {
                    Assert.AreEqual(4, resized.Width);
                    Assert.AreEqual(4, resized.Height);
                }

                using (var resized = ImageResizer.ResizeImageToCover(image, height: 4))
                {
                    Assert.AreEqual(4, resized.Width);
                    Assert.AreEqual(4, resized.Height);
                }

                using (var resized = ImageResizer.ResizeImageToCover(image, width: 4, height: 4))
                {
                    Assert.AreEqual(4, resized.Width);
                    Assert.AreEqual(4, resized.Height);
                }

                using (var resized = ImageResizer.ResizeImageToCover(image, width: 2, height: 4))
                {
                    Assert.AreEqual(2, resized.Width);
                    Assert.AreEqual(4, resized.Height);
                }

                using (var resized = ImageResizer.ResizeImageToCover(image, width: 4, height: 2))
                {
                    Assert.AreEqual(4, resized.Width);
                    Assert.AreEqual(2, resized.Height);
                }
            }
        }

        [TestMethod]
        public void ResizesTallImageToCover()
        {
            using (var image = new Bitmap(4, 8))
            {
                using (var resized = ImageResizer.ResizeImageToCover(image, width: 4))
                {
                    Assert.AreEqual(4, resized.Width);
                    Assert.AreEqual(8, resized.Height);
                }

                using (var resized = ImageResizer.ResizeImageToCover(image, height: 4))
                {
                    Assert.AreEqual(2, resized.Width);
                    Assert.AreEqual(4, resized.Height);
                }

                using (var resized = ImageResizer.ResizeImageToCover(image, width: 4, height: 4))
                {
                    Assert.AreEqual(4, resized.Width);
                    Assert.AreEqual(4, resized.Height);
                }

                using (var resized = ImageResizer.ResizeImageToCover(image, width: 2, height: 4))
                {
                    Assert.AreEqual(2, resized.Width);
                    Assert.AreEqual(4, resized.Height);
                }

                using (var resized = ImageResizer.ResizeImageToCover(image, width: 4, height: 2))
                {
                    Assert.AreEqual(4, resized.Width);
                    Assert.AreEqual(2, resized.Height);
                }
            }
        }

        [TestMethod]
        public void ResizesWideImageToCover()
        {
            using (var image = new Bitmap(8, 4))
            {
                using (var resized = ImageResizer.ResizeImageToCover(image, width: 4))
                {
                    Assert.AreEqual(4, resized.Width);
                    Assert.AreEqual(2, resized.Height);
                }

                using (var resized = ImageResizer.ResizeImageToCover(image, height: 4))
                {
                    Assert.AreEqual(8, resized.Width);
                    Assert.AreEqual(4, resized.Height);
                }

                using (var resized = ImageResizer.ResizeImageToCover(image, width: 4, height: 4))
                {
                    Assert.AreEqual(4, resized.Width);
                    Assert.AreEqual(4, resized.Height);
                }

                using (var resized = ImageResizer.ResizeImageToCover(image, width: 2, height: 4))
                {
                    Assert.AreEqual(2, resized.Width);
                    Assert.AreEqual(4, resized.Height);
                }

                using (var resized = ImageResizer.ResizeImageToCover(image, width: 4, height: 2))
                {
                    Assert.AreEqual(4, resized.Width);
                    Assert.AreEqual(2, resized.Height);
                }
            }
        }
    }
}
