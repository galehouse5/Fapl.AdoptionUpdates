using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Fapl.AdoptionUpdates.Shared.Images
{
    // Refer to: http://stackoverflow.com/questions/1922040/resize-an-image-c-sharp
    public static class ImageResizer
    {
        public static Image ResizeImageToCover(this Image image,
            int? width = null, int? height = null)
        {
            float aspectRatio = (float)image.Width / (float)image.Height;
            width = width ?? (int?)(height * aspectRatio) ?? image.Width;
            height = height ?? (int?)(width / aspectRatio) ?? image.Height;

            Bitmap result = null;

            try
            {
                result = new Bitmap(width.Value, height.Value);
                result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(result))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var attributes = new ImageAttributes())
                    {
                        attributes.SetWrapMode(WrapMode.TileFlipXY);

                        float resizedWidth = Math.Max(width.Value, height.Value * aspectRatio);
                        float resizedHeight = Math.Max(height.Value, width.Value / aspectRatio);
                        float overflowingWidth = resizedWidth - result.Width;
                        float overflowingHeight = resizedHeight - result.Height;

                        graphics.DrawImage(image,
                            x: -overflowingWidth / 2,
                            y: -overflowingHeight / 2,
                            width: resizedWidth,
                            height: resizedHeight);
                    }
                }

                return result;
            }
            catch
            {
                result?.Dispose();
                throw;
            }
        }
    }
}
