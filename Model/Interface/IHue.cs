using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHue.Model
{
    public interface IHue : IModel
    {
        event EventHandler<HueEventArgs> StatusUpdate;
        Task Init();
        Task<List<string>> GetGroups();
        Task SetColor(Color c, string @group, TimeSpan t, int briWhite, int briColor, bool randomLights);
        void TurnOff(string @group);
    }
}
