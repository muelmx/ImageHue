using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHue.ViewModel
{
    public interface IChromeViewModel : IViewModel
    {
        IMainViewModel Main { get; }
    }
}
