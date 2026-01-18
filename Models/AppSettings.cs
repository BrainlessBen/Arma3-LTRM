using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Arma_3_LTRM.Models
{
    public class AppSettings : INotifyPropertyChanged
    {
        private string _arma3ExePath;
        private List<string> _baseDownloadLocations;
        private List<string> _hiddenRepositories;
        private List<string> _hiddenEvents;
        private LaunchParameters _launchParameters;
        private double _cacheLifetimeHours = 1.0;

        public string Arma3ExePath
        {
            get => _arma3ExePath;
            set
            {
                if (_arma3ExePath != value)
                {
                    _arma3ExePath = value;
                    OnPropertyChanged(nameof(Arma3ExePath));
                }
            }
        }

        public List<string> BaseDownloadLocations
        {
            get => _baseDownloadLocations;
            set
            {
                if (_baseDownloadLocations != value)
                {
                    _baseDownloadLocations = value;
                    OnPropertyChanged(nameof(BaseDownloadLocations));
                }
            }
        }

        public List<string> HiddenRepositories
        {
            get => _hiddenRepositories;
            set
            {
                if (_hiddenRepositories != value)
                {
                    _hiddenRepositories = value;
                    OnPropertyChanged(nameof(HiddenRepositories));
                }
            }
        }

        public List<string> HiddenEvents
        {
            get => _hiddenEvents;
            set
            {
                if (_hiddenEvents != value)
                {
                    _hiddenEvents = value;
                    OnPropertyChanged(nameof(HiddenEvents));
                }
            }
        }

        public LaunchParameters LaunchParameters
        {
            get => _launchParameters;
            set
            {
                if (_launchParameters != value)
                {
                    _launchParameters = value;
                    OnPropertyChanged(nameof(LaunchParameters));
                }
            }
        }

        public double CacheLifetimeHours
        {
            get => _cacheLifetimeHours;
            set
            {
                if (Math.Abs(_cacheLifetimeHours - value) > 0.01)
                {
                    _cacheLifetimeHours = value;
                    OnPropertyChanged(nameof(CacheLifetimeHours));
                }
            }
        }

        public AppSettings()
        {
            _arma3ExePath = string.Empty;
            _baseDownloadLocations = new List<string>();
            _hiddenRepositories = new List<string>();
            _hiddenEvents = new List<string>();
            _launchParameters = new LaunchParameters();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
