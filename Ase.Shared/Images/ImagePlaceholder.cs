using System.Drawing;
using System.Reflection;

namespace Ase.Shared.Images
{
    public enum ImagePlaceholder
    {
        Bird, Cat, Dog, Pig, Rabbit, Hamster
    }

    public static class ImagePlaceholderSpecies
    {
        public static Image GenerateImage(this ImagePlaceholder value, Color backgroundColor)
        {
            using (var iconData = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"Ase.Shared.Icons.{value}.png"))
            using (var iconImage = new Bitmap(iconData))
            {
                Image placeholderImage = null;

                try
                {
                    placeholderImage = new Bitmap(iconImage.Width * 4, iconImage.Height * 4);

                    using (Graphics graphics = Graphics.FromImage(placeholderImage))
                    {
                        graphics.Clear(backgroundColor);

                        graphics.DrawImage(iconImage,
                            x: placeholderImage.Width / 2f - iconImage.Width / 2f,
                            y: placeholderImage.Height / 2f - iconImage.Height / 2f);

                        return placeholderImage;
                    }
                }
                catch
                {
                    placeholderImage?.Dispose();
                    throw;
                }
            }
        }
    }
}
