using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImageHue.ViewModel
{
    public interface IMainViewModel : IViewModel
    {
        string Status { get; set; }

        System.Drawing.Image Image { get; set; }

        Color CurrentColor { get; set; }

        ICommand LoadImageCommand { get; }

        ICommand RunCommand { get; }

        ICommand TurnOffCommand { get; }

        bool Loading { get; set; }

        int Speed { get; set; }

        bool Run { get; set; }

        bool Random { get; set; }

        bool Sync { get; set; }

        int BriColor { get; set; }

        int BriWhite { get; set; }
 
        bool PickRandom { get; set; }

        List<string> Groups { get; set; }

        String SelectedGroup { get; set; }
    }
}
