using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.HSB;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Bridge;
using Q42.HueApi.NET;

namespace ImageHue.Model
{
    public class HueEventArgs : EventArgs
    {
        public string Status { get; set; }
    }
    public class Hue : IHue
    {
        SSDPBridgeLocator _locator;
        IEnumerable<Q42.HueApi.Models.Bridge.LocatedBridge> _bridges;
        IDB _db;
        ILocalHueClient _client;
        int _initcounter = 0;
        List<Q42.HueApi.Models.Groups.Group> _groups;

        public event EventHandler<HueEventArgs> StatusUpdate;

        public Hue(IDB db)
        {
            _locator = new SSDPBridgeLocator();
            _db = db;
        }

        private async Task<LocatedBridge> GetBridge()
        {
            _bridges = await _locator.LocateBridgesAsync(TimeSpan.FromSeconds(10));
            return _bridges.First();
        }


        public async Task Init()
        {
            var configs = await _db.Getconfig();
            if(configs == null)
            {
                StatusUpdate?.Invoke(this, new HueEventArgs { Status = "Searching" });
                var bridge = await GetBridge();

                if(bridge == null)
                {
                    StatusUpdate?.Invoke(this, new HueEventArgs { Status = "No Bridge found" });
                    return;
                }

                _client = new LocalHueClient(bridge.IpAddress);

                while (true)
                {
                    StatusUpdate?.Invoke(this, new HueEventArgs { Status = "Press the Button" });
                    await Task.Factory.StartNew(() => Thread.Sleep(TimeSpan.FromSeconds(5)));
                    try
                    {
                        var appkey = await _client.RegisterAsync("ImageHue", System.Environment.MachineName.ToString());
                        await _db.Setconfig(new Config { IP = bridge.IpAddress, Key = appkey });
                        break;
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine(e.Message);
                    }
                }
                StatusUpdate?.Invoke(this, new HueEventArgs { Status = "Found IP: " + bridge.IpAddress });
            }
            else
            {
                _client = new LocalHueClient(configs.IP);
                _client.Initialize(configs.Key);
            }

            try
            {
                var test = await _client.GetCapabilitiesAsync();
                StatusUpdate?.Invoke(this, new HueEventArgs { Status = "Connection Established" });
            }
            catch(Exception e)
            {
                StatusUpdate?.Invoke(this, new HueEventArgs { Status = "Error, retry" });
                await _db.Deleteconfig();
                _initcounter++;

                if(_initcounter < 5)
                    await Init();
            }
        }

        public Task<List<string>> GetGroups()
        {
            return Task.Factory.StartNew<List<string>>(() =>
            {
                if (_client == null) return new List<string>();

                var awaiter =  _client.GetGroupsAsync();
                awaiter.Wait();
                _groups = awaiter.Result.ToList();
                _groups.Add(new Q42.HueApi.Models.Groups.Group() { Id = "0", Name = "All" });
                _groups = _groups.OrderBy(gp => gp.Name).ToList();

                return _groups.Select(g => g.Name).ToList();
            });
        }

        public void SetColor(Color c, string Group, TimeSpan t)
        {
            var gp = _groups?.Where(g => g.Name == Group).FirstOrDefault();
            if (gp == null) return;

            var command = new LightCommand();
            command.TurnOn().SetColor(new RGBColor(c.R, c.G, c.B));
            command.TransitionTime = t;

            try
            {
                _client?.SendGroupCommandAsync(command, gp.Id);
            }
            catch(Exception e)
            {
                System.Console.WriteLine(e);
            }
        }

        public void TurnOff(string Group)
        {
            var gp = _groups?.Where(g => g.Name == Group).FirstOrDefault();
            if (gp == null) return;

            var command = new LightCommand();
            command.TurnOff();
            try
            {
                _client?.SendGroupCommandAsync(command, gp.Id);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
        }
    }
}
