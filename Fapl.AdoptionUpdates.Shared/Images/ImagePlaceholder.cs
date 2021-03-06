﻿using System.Drawing;
using System.Reflection;

namespace Fapl.AdoptionUpdates.Shared.Images
{
    public enum ImagePlaceholder
    {
        Cat,
        Cattle,
        Chicken,
        Chinchilla,
        Dog,
        Donkey,
        Duck,
        Ferret,
        Fowl,
        Gerbil,
        Goat,
        Goose,
        GuineaPig,
        Hamster,
        Horse,
        Iguana,
        Lizard,
        Mouse,
        Parakeet,
        Pig,
        Pigeon,
        Rabbit,
        Rat,
        Snake,
        Tortoise,
        Turtle
    }

    public static class ImagePlaceholderSpecies
    {
        public static Image GenerateImage(this ImagePlaceholder value, Color backgroundColor)
        {
            using (var iconData = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"Fapl.AdoptionUpdates.Shared.Icons.{value}.png"))
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
