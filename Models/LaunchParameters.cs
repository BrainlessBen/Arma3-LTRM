using System.ComponentModel;

namespace Arma_3_LTRM.Models
{
    public class LaunchParameters : INotifyPropertyChanged
    {
        // Profile options
        private bool _useName;
        private string _name;
        private bool _useProfile;
        private string _profilePath;
        private string _unit;

        // Mission
        private bool _useMission;
        private string _missionPath;

        // Display options
        private bool _windowed;

        // Game Loading Speedup
        private bool _noSplash;
        private bool _skipIntro;
        private bool _emptyWorld;
        private bool _enableHT;

        // Misc. options
        private bool _showScriptErrors;
        private bool _noPause;
        private bool _noPauseAudio;
        private bool _noLogs;
        private bool _noFreezeCheck;
        private bool _noFilePatching;
        private bool _debug;

        // Server options
        private string _config;
        private string _bePath;

        #region Profile Options

        public bool UseName
        {
            get => _useName;
            set
            {
                if (_useName != value)
                {
                    _useName = value;
                    OnPropertyChanged(nameof(UseName));
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public bool UseProfile
        {
            get => _useProfile;
            set
            {
                if (_useProfile != value)
                {
                    _useProfile = value;
                    OnPropertyChanged(nameof(UseProfile));
                }
            }
        }

        public string ProfilePath
        {
            get => _profilePath;
            set
            {
                if (_profilePath != value)
                {
                    _profilePath = value;
                    OnPropertyChanged(nameof(ProfilePath));
                }
            }
        }

        public string Unit
        {
            get => _unit;
            set
            {
                if (_unit != value)
                {
                    _unit = value;
                    OnPropertyChanged(nameof(Unit));
                }
            }
        }

        #endregion

        #region Mission

        public bool UseMission
        {
            get => _useMission;
            set
            {
                if (_useMission != value)
                {
                    _useMission = value;
                    OnPropertyChanged(nameof(UseMission));
                }
            }
        }

        public string MissionPath
        {
            get => _missionPath;
            set
            {
                if (_missionPath != value)
                {
                    _missionPath = value;
                    OnPropertyChanged(nameof(MissionPath));
                }
            }
        }

        #endregion

        #region Display Options

        public bool Windowed
        {
            get => _windowed;
            set
            {
                if (_windowed != value)
                {
                    _windowed = value;
                    OnPropertyChanged(nameof(Windowed));
                }
            }
        }

        #endregion

        #region Game Loading Speedup

        public bool NoSplash
        {
            get => _noSplash;
            set
            {
                if (_noSplash != value)
                {
                    _noSplash = value;
                    OnPropertyChanged(nameof(NoSplash));
                }
            }
        }

        public bool SkipIntro
        {
            get => _skipIntro;
            set
            {
                if (_skipIntro != value)
                {
                    _skipIntro = value;
                    OnPropertyChanged(nameof(SkipIntro));
                }
            }
        }

        public bool EmptyWorld
        {
            get => _emptyWorld;
            set
            {
                if (_emptyWorld != value)
                {
                    _emptyWorld = value;
                    OnPropertyChanged(nameof(EmptyWorld));
                }
            }
        }

        public bool EnableHT
        {
            get => _enableHT;
            set
            {
                if (_enableHT != value)
                {
                    _enableHT = value;
                    OnPropertyChanged(nameof(EnableHT));
                }
            }
        }

        #endregion

        #region Misc. Options

        public bool ShowScriptErrors
        {
            get => _showScriptErrors;
            set
            {
                if (_showScriptErrors != value)
                {
                    _showScriptErrors = value;
                    OnPropertyChanged(nameof(ShowScriptErrors));
                }
            }
        }

        public bool NoPause
        {
            get => _noPause;
            set
            {
                if (_noPause != value)
                {
                    _noPause = value;
                    OnPropertyChanged(nameof(NoPause));
                }
            }
        }

        public bool NoPauseAudio
        {
            get => _noPauseAudio;
            set
            {
                if (_noPauseAudio != value)
                {
                    _noPauseAudio = value;
                    OnPropertyChanged(nameof(NoPauseAudio));
                }
            }
        }

        public bool NoLogs
        {
            get => _noLogs;
            set
            {
                if (_noLogs != value)
                {
                    _noLogs = value;
                    OnPropertyChanged(nameof(NoLogs));
                }
            }
        }

        public bool NoFreezeCheck
        {
            get => _noFreezeCheck;
            set
            {
                if (_noFreezeCheck != value)
                {
                    _noFreezeCheck = value;
                    OnPropertyChanged(nameof(NoFreezeCheck));
                }
            }
        }

        public bool NoFilePatching
        {
            get => _noFilePatching;
            set
            {
                if (_noFilePatching != value)
                {
                    _noFilePatching = value;
                    OnPropertyChanged(nameof(NoFilePatching));
                }
            }
        }

        public bool Debug
        {
            get => _debug;
            set
            {
                if (_debug != value)
                {
                    _debug = value;
                    OnPropertyChanged(nameof(Debug));
                }
            }
        }

        #endregion

        #region Server Options

        public string Config
        {
            get => _config;
            set
            {
                if (_config != value)
                {
                    _config = value;
                    OnPropertyChanged(nameof(Config));
                }
            }
        }

        public string BePath
        {
            get => _bePath;
            set
            {
                if (_bePath != value)
                {
                    _bePath = value;
                    OnPropertyChanged(nameof(BePath));
                }
            }
        }

        #endregion

        public LaunchParameters()
        {
            _useName = false;
            _name = string.Empty;
            _useProfile = false;
            _profilePath = string.Empty;
            _unit = string.Empty;
            _useMission = false;
            _missionPath = string.Empty;
            _windowed = false;
            _noSplash = false;
            _skipIntro = false;
            _emptyWorld = false;
            _enableHT = false;
            _showScriptErrors = false;
            _noPause = false;
            _noPauseAudio = false;
            _noLogs = false;
            _noFreezeCheck = false;
            _noFilePatching = false;
            _debug = false;
            _config = string.Empty;
            _bePath = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
