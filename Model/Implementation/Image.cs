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
        private Bitmap _img;
        private readonly object _locker = new object();
        public Color GetNextValue()
        {
            if(_img == null)
            {
                return Color.Black;
            }

            Bitmap imageclone;
            lock (_locker)
            {
                imageclone = (Bitmap)_img.Clone();
            }

            var r = new Random();
            var clr = imageclone.GetPixel((int)Math.Floor(r.NextDouble() * imageclone.Width), (int)Math.Floor(r.NextDouble() * imageclone.Height));

            return clr;
        }

        public Task<System.Drawing.Image> Load(string path)
        {
            return Task.Factory.StartNew<System.Drawing.Image>(() =>
            {
                System.Drawing.Image image;
                var photoBytes = File.ReadAllBytes(path);
                ISupportedImageFormat format = new PngFormat();

                using (var inStream = new MemoryStream(photoBytes))
                {
                    using (var imageFactory = new ImageFactory())
                    {
                        image = (System.Drawing.Image)imageFactory.Load(inStream).Pixelate(50).Image.Clone();
                    }
                }

                lock (_locker)
                {
                    _img = new Bitmap(image);
                }
                return image;
            });
        }
    }
}
