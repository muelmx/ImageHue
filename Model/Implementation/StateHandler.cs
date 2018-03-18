using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ImageHue.Model
{
    public class ColorEventArgs : EventArgs
    {
        public Color Color { get; set; }
    }

    internal class StateHandler : IStateHandler
    {
        private System.Timers.Timer _timer;
        private readonly Random _r = new Random();
        private bool _run = false;
        private Color _currentColor;
        private readonly IHue _hue;
        private readonly IImage _img;
        private double _speed = 1;
        private bool _random;
        private bool _sync;

        

        public event EventHandler<ColorEventArgs> ColorUpdate;

        public StateHandler(IHue hue, IImage img)
        {
            this._img = img;
            this._hue = hue;
        }

        public string SelectedGroup { private get; set; }
        public double Speed
        {
            private get => _speed; set
            {
                if (value <= 0) return;
                _speed = value;
                RestartTimer();
            }
        }
        public bool Run
        {
            private get => _run;
            set
            {
                _run = value;
                Update();
            }
        }

        public bool Random { set { _random = value; RestartTimer(); } private get => _random; }
        public bool Sync { set { _sync = value; RestartTimer(); } private get => _sync; }
        public int BriColor { private get; set; } = 255;
        public int BriWhite { private get; set; } = 255;
        public bool PickRandom { private get; set; }

        private void Update()
        {
            if (Run)
            {
                UpdateColor(RestartTimer());
            }
            else
            {
                _timer?.Stop();
                _timer = null;
            }
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!Run) return;
            UpdateColor(RestartTimer());

        }

        private double RestartTimer()
        {
            if (!Run) return 0;

            _timer?.Stop();

            var interval = 1000 * (60 + ( Random ? (int)Math.Floor(_r.NextDouble() * 90) : 60)) / Speed;
            _timer = new System.Timers.Timer
            {
                Interval = interval,
                AutoReset = false
            };
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();

            return interval;
        }

        private void UpdateColor(double timerInterval = 0)
        {
            if (_img == null) return;
            _currentColor = _img.GetNextValue();
            ColorUpdate?.Invoke(this, new ColorEventArgs { Color = _currentColor });

            var t = Sync && timerInterval > 0 ? TimeSpan.FromMilliseconds(timerInterval) : TimeSpan.FromSeconds((20 + (Random ? (int)Math.Floor(_r.NextDouble() * 10) : 10)) / (Speed/2));

            _hue.SetColor(_currentColor, SelectedGroup, t, BriWhite, BriColor, PickRandom);
        }
    }

}
