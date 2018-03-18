using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageHue.Helper;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Original;
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
        readonly SSDPBridgeLocator _locator;
        private IEnumerable<Q42.HueApi.Models.Bridge.LocatedBridge> _bridges;
        readonly IDB _db;
        private ILocalHueClient _client;
        private int _initcounter = 0;
        private List<Q42.HueApi.Models.Groups.Group> _groups;
        private List<Q42.HueApi.Light> _lights;
        private readonly Random _r = new Random();

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
            if (configs == null)
            {
                StatusUpdate?.Invoke(this, new HueEventArgs { Status = "Searching" });
                var bridge = await GetBridge();

                if (bridge == null)
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
            catch (Exception e)
            {
                StatusUpdate?.Invoke(this, new HueEventArgs { Status = "Error, retry" });
                await _db.Deleteconfig();
                _initcounter++;

                if (_initcounter < 5)
                    await Init();
            }
        }

        public Task<List<string>> GetGroups()
        {
            return Task.Factory.StartNew<List<string>>(() =>
            {
                if (_client == null) return new List<string>();

                var groupsAwaiter = _client.GetGroupsAsync();
                var lightsAwaiter = _client.GetLightsAsync();
                Task.WaitAll(new List<Task>() { groupsAwaiter, lightsAwaiter }.ToArray());

                _groups = groupsAwaiter.Result.ToList();
                _groups.Add(new Q42.HueApi.Models.Groups.Group() { Id = "0", Name = "All" });
                _groups = _groups.OrderBy(gp => gp.Name).ToList();

                _lights = lightsAwaiter.Result.ToList();

                return _groups.Select(g => g.Name).ToList();
            });
        }

        private static int XyToTemperature(double[] xy)
        {
            //https://developers.meethue.com/documentation/lights-api
            var x = xy[0];
            var y = xy[1];

            var temp2 = (437 * Math.Pow((x - 0.332) / (0.1858 - y), 3) +
                           3601 * Math.Pow((x - 0.332) / (0.1858 - y), 2) +
                           6831 * ((x - 0.332) / (0.1858 - y))) +
                          5517;
            var ret1 = (float)(1 / temp2 * 1000000);

            // Method 1
            //http://stackoverflow.com/questions/13975917/calculate-colour-temperature-in-k
            //= (-449 * ((R1 - 0, 332) / (S1 - 0, 1858)) ^ 3) + (3525 * ((R1 - 0, 332) / (S1 - 0, 1858)) ^ 2) - (6823, 3 * ((R1 - 0, 332) / (S1 - 0, 1858))) + (5520, 33)
            var temp1 = -449 * Math.Pow((x - 0.332) / (y - 0.1858), 3)
                            + 3525 * Math.Pow((x - 0.332) / (y - 0.1858), 2)
                            - 6823.3 * ((x - 0.332) / (y - 0.1858))
                            + 5520.33;
            float ret2 = (float)(1 / temp1 * 1000000);
            float ret = (ret1 + ret2) / 2; //Take average

            if (ret < 153) ret = 153;
            else if (ret > 500) ret = 500;
            return (int)ret;
        }

        public Task SetColor(Color c, string @group, TimeSpan t, int briWhite, int briColor, bool randomLights)
        {
            return Task.Factory.StartNew(() =>
            {
                if (c == Color.Black) return;

                var gp = _groups?.Where(g => g.Name == group).FirstOrDefault();
                if (gp == null) return;

                var lights = _lights;
                if (gp.Lights != null)
                {
                    lights = _lights.Where(l => gp.Lights.Contains(l.Id)).ToList();
                }

                var whites = lights.Where(l => l.Type == "Color temperature light").ToList();
                var colors = lights.Where(l => l.Type == "Extended color light").ToList();

                if (randomLights)
                {
                    whites = whites.PickRandom(_r.Next(whites.Count + 1)).ToList();
                    colors = colors.PickRandom(_r.Next(whites.Count + 1)).ToList();
                }

                var color = new RGBColor(c.R, c.G, c.B);

                //Colors
                var cCommand = new LightCommand();
                if (briColor > 0)
                {
                    cCommand.TurnOn().SetColor(color, colors.FirstOrDefault()?.ModelId ?? "LCT001");
                    cCommand.Brightness = Convert.ToByte(briColor);
                    cCommand.TransitionTime = t;
                }
                else
                    cCommand.TurnOff();

                // Whites
                var tmpCommand = new LightCommand();
                tmpCommand.SetColor(color, whites.FirstOrDefault()?.ModelId ?? "LCT001");
                var wCommand = new LightCommand();
                if (briWhite > 0)
                {
                    wCommand.TurnOn();
                    wCommand.ColorTemperature = XyToTemperature(tmpCommand.ColorCoordinates);
                    wCommand.Brightness = Convert.ToByte(briWhite);
                    wCommand.TransitionTime = t;
                }
                else
                    wCommand.TurnOff();

                try
                {
                    if (colors.Count > 0)
                        _client?.SendCommandAsync(cCommand, colors.Select(l => l.Id)).Wait();
                    Thread.Sleep(colors.Count * 100);
                    if (whites.Count > 0)
                        _client?.SendCommandAsync(wCommand, whites.Select(l => l.Id)).Wait();
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e);
                }
            });
        }

        public void TurnOff(string @group)
        {
            var gp = _groups?.Where(g => g.Name == group).FirstOrDefault();
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
