using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHue.Model
{
    public class Config
    {
        public int Id { get; set; }
        public string IP { get; set; }
        public string Key { get; set; }
    }
    public interface IDB : IModel
    {
        Task<Config> Getconfig();
        Task Setconfig(Config config);
        Task Deleteconfig();
    }
}
