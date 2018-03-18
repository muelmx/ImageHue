using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHue.ViewModel
{
    using System.Drawing;
    using System.IO;
    using System.Windows.Input;
    using Microsoft.Win32;
    using Model;

    public class MainViewModel : BaseViewModel, IMainViewModel
    {
        private readonly IHue _hue;
        private readonly IImage _img;
        private readonly IStateHandler _handler;

        private string _status = "Starting up...";
        private System.Drawing.Image _image;
        private Color _currentColor;
        private bool _loading;

        private RelayCommand _runCommand;
        private RelayCommand _turnOffCommand;
        private RelayCommand _loadImageCommand;

        private List<string> _groups;
        private string _selectedGroup;
        private int _speed = 10;
        private bool _sync = true;
        private bool _random = true;
        private int _briColor = 255;
        private int _briWhite = 255;
        private bool _pickRandom;
        private bool _run;

        public MainViewModel(IHue hue, IImage img, IStateHandler handler)
        {
            _img = img;
            _hue = hue;
            _handler = handler;
            _handler.ColorUpdate += _handler_ColorUpdate;
            _hue.StatusUpdate += _hue_StatusUpdate;
            Task.Factory.StartNew(async () =>
            {
                await _hue.Init();
                Groups = await _hue.GetGroups();
            });

        }

        private void _handler_ColorUpdate(object sender, ColorEventArgs e)
        {
            this.CurrentColor = e.Color;
        }

        private void _hue_StatusUpdate(object sender, HueEventArgs e)
        {

            Status = e.Status;
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public System.Drawing.Image Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }

        public Color CurrentColor
        {
            get => _currentColor; set
            {
                _currentColor = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadImageCommand
        {
            get
            {
                return _loadImageCommand ?? (_loadImageCommand = new RelayCommand(LoadImageAsyncHandler, param => !Loading));
            }
        }

        public bool Loading
        {
            get => _loading; set
            {
                _loading = value;
                OnPropertyChanged();
            }
        }


        private async void LoadImageAsyncHandler(object o)
        {
            await LoadImageAsync();
        }
        public async Task LoadImageAsync()
        {
            //Open a file dialog, so that the user can chose a picture
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = $"Images (*.png;*.jpeg;*jpg;*.gif;*.tif)|*.png;*jpg;*.jpeg;*.gif;*.tif"
            };

            if (openFileDialog.ShowDialog() != true) return;

            var filename = Path.GetFullPath(openFileDialog.FileName);
            if (!File.Exists(filename)) return;
            Loading = true;
            Image = await _img.Load(filename);
            Loading = false;
        }

        public ICommand RunCommand => _runCommand ?? (_runCommand = new RelayCommand(RunCommandHandler));

        private void RunCommandHandler(object o)
        {
        }

        public bool Run
        {
            get => _run;
            set
            {
                _run = value; OnPropertyChanged();
                _handler.Run = _run;
            }
        }


        public ICommand TurnOffCommand => _turnOffCommand ?? (_turnOffCommand = new RelayCommand(TurnOffCommandHandler));

        private void TurnOffCommandHandler(object o)
        {
            Run = false;
            _hue.TurnOff(SelectedGroup);
        }



        public List<string> Groups
        {
            get => _groups;
            set
            {
                _groups = value;
                SelectedGroup = _groups[0];
                OnPropertyChanged();
            }
        }
        public string SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                _selectedGroup = value; OnPropertyChanged();
                _handler.SelectedGroup = _selectedGroup;
            }
        }

        public int Speed
        {
            get => _speed; set
            {
                _handler.Speed = value;
                _speed = value;
            }
        }

        public bool Sync
        {
            get => _sync;
            set
            {
                _sync = value;
                _handler.Sync = _sync;
            }
        }

        public bool Random
        {
            get => _random;
            set
            {
                _random = value;
                _handler.Random = _random;
            }
        }

        public int BriColor
        {
            get => _briColor;
            set
            {
                if (value < 0 || value > 255) throw new ArgumentOutOfRangeException(nameof(value));
                _briColor = value;
                _handler.BriColor = _briColor;
            }
        }

        public int BriWhite
        {
            get => _briWhite;
            set
            {
                if (value < 0 || value > 255) throw new ArgumentOutOfRangeException(nameof(value));
                _briWhite = value;
                _handler.BriWhite = _briWhite;
            }
        }

        public bool PickRandom
        {
            get => _pickRandom;
            set
            {
                _pickRandom = value;
                _handler.PickRandom = _pickRandom;
            }
        }
    }
}
