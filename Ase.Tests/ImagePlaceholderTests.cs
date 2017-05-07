using Ase.Shared.Images;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;

namespace Ase.Tests
{
    [TestClass]
    public class ImagePlaceholderTests
    {
        [TestMethod]
        public void GeneratesImage()
        {
            foreach (ImagePlaceholder placeholder in Enum.GetValues(typeof(ImagePlaceholder)))
            {
                using (Image image = placeholder.GenerateImage(ColorTranslator.FromHtml("#222222")))
                {
                    Assert.IsNotNull(image);
                }
            }
        }
    }
}
