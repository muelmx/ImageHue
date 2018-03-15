using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHue.Model
{
    public interface IStateHandler : IModel
    {
        string SelectedGroup { set; }
        double Speed { set; }
        bool Run { set; }
        bool Random { set; }
        bool Sync { set; }

        event EventHandler<ColorEventArgs> ColorUpdate;
    }
}
