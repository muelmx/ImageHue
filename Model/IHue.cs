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
        void SetColor(Color c, string Group, TimeSpan t);
        void TurnOff(string Group);
    }
}
