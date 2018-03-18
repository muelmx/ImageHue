using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHue.Model
{
    public interface IImage : IModel
    {
        Task<System.Drawing.Image> Load(string path);
        Color GetNextValue();
    }
}
