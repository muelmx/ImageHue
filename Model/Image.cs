using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHue.Model
{
    public class Image : IImage
    {
        Bitmap img;
        Object locker = new Object();
        public Color GetNextValue()
        {
            if(img == null)
            {
                return Color.Black;
            }

            Bitmap imageclone;
            lock (locker)
            {
                imageclone = (Bitmap)img.Clone();
            }

            Random r = new Random();
            Color clr = imageclone.GetPixel((int)Math.Floor(r.NextDouble() * imageclone.Width), (int)Math.Floor(r.NextDouble() * imageclone.Height));

            return clr;
        }

        public Task<System.Drawing.Image> Load(string path)
        {
            return Task.Factory.StartNew<System.Drawing.Image>(() =>
            {
                System.Drawing.Image image;
                byte[] photoBytes = File.ReadAllBytes(path);
                ISupportedImageFormat format = new PngFormat();

                using (MemoryStream inStream = new MemoryStream(photoBytes))
                {
                    using (ImageFactory imageFactory = new ImageFactory())
                    {
                        image = (System.Drawing.Image)imageFactory.Load(inStream).Pixelate(50).Image.Clone();
                    }
                }

                img = new Bitmap(image);
                return image;
            });
        }
    }
}
