using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHue.ViewModel
{
    public class ChromeViewModel : BaseViewModel, IChromeViewModel
    {
        public IMainViewModel Main { get; }

        public ChromeViewModel(IMainViewModel main)
        {
            Main = main;
        }
    }
}
