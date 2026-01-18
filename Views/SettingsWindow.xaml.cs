using System.Windows;
using System.Collections.ObjectModel;
using Arma_3_LTRM.Services;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using OpenFolderDialog = Microsoft.Win32.OpenFolderDialog;
using MessageBox = System.Windows.MessageBox;

namespace Arma_3_LTRM.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsManager _settingsManager;
        private ObservableCollection<string> _downloadLocations;

        public SettingsWindow(SettingsManager settingsManager)
        {
            InitializeComponent();
            _settingsManager = settingsManager;
            _downloadLocations = new ObservableCollection<string>();
            LoadSettings();
        }

        private void LoadSettings()
        {
            Arma3PathTextBox.Text = _settingsManager.Settings.Arma3ExePath;
            
            _downloadLocations.Clear();
            foreach (var location in _settingsManager.Settings.BaseDownloadLocations)
            {
                _downloadLocations.Add(location);
            }
            BaseDownloadLocationsListBox.ItemsSource = _downloadLocations;
        }

        private void BrowseArma3Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Arma 3 Executable|arma3.exe;arma3_x64.exe|All Files|*.*",
                Title = "Select Arma 3 Executable"
            };

            if (dialog.ShowDialog() == true)
            {
                Arma3PathTextBox.Text = dialog.FileName;
            }
        }

        private void AddLocationButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Download Location"
            };

            if (dialog.ShowDialog() == true)
            {
                if (!_downloadLocations.Contains(dialog.FolderName))
                {
                    _downloadLocations.Add(dialog.FolderName);
                }
                else
                {
                    MessageBox.Show("This location is already in the list.", "Duplicate Location", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void RemoveLocationButton_Click(object sender, RoutedEventArgs e)
        {
            if (BaseDownloadLocationsListBox.SelectedItem is string selectedLocation)
            {
                if (_downloadLocations.Count <= 1)
                {
                    MessageBox.Show("You must have at least one download location.", "Cannot Remove", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                _downloadLocations.Remove(selectedLocation);
            }
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = BaseDownloadLocationsListBox.SelectedIndex;
            if (selectedIndex > 0)
            {
                var item = _downloadLocations[selectedIndex];
                _downloadLocations.RemoveAt(selectedIndex);
                _downloadLocations.Insert(selectedIndex - 1, item);
                BaseDownloadLocationsListBox.SelectedIndex = selectedIndex - 1;
            }
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = BaseDownloadLocationsListBox.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < _downloadLocations.Count - 1)
            {
                var item = _downloadLocations[selectedIndex];
                _downloadLocations.RemoveAt(selectedIndex);
                _downloadLocations.Insert(selectedIndex + 1, item);
                BaseDownloadLocationsListBox.SelectedIndex = selectedIndex + 1;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _settingsManager.Settings.Arma3ExePath = Arma3PathTextBox.Text;
            _settingsManager.Settings.BaseDownloadLocations = _downloadLocations.ToList();
            _settingsManager.SaveSettings();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

