using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Collections.ObjectModel;
using Arma_3_LTRM.Models;
using Arma_3_LTRM.Services;
using EventManager = Arma_3_LTRM.Services.EventManager;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Button = System.Windows.Controls.Button;
using OpenFolderDialog = Microsoft.Win32.OpenFolderDialog;

namespace Arma_3_LTRM.Views
{
    public partial class MainWindow : Window
    {
        private readonly RepositoryManager _repositoryManager;
        private readonly EventManager _eventManager;
        private readonly SettingsManager _settingsManager;
        private readonly FtpManager _ftpManager;
        private ObservableCollection<string> _downloadLocations;

        public MainWindow()
        {
            InitializeComponent();

            _repositoryManager = new RepositoryManager();
            _eventManager = new EventManager();
            _eventManager.Initialize(_repositoryManager);
            _settingsManager = new SettingsManager();
            _ftpManager = new FtpManager();
            _downloadLocations = new ObservableCollection<string>();

            LoadData();
        }

        private void LoadData()
        {
            RepositoriesListBox.ItemsSource = _repositoryManager.Repositories;
            EventsListBox.ItemsSource = _eventManager.Events;
            ManageRepositoriesListBox.ItemsSource = _repositoryManager.Repositories;
            ManageEventsListBox.ItemsSource = _eventManager.Events;
            LoadSettings();
        }

        private void LoadSettings()
        {
            SettingsArma3PathTextBox.Text = _settingsManager.Settings.Arma3ExePath;
            
            _downloadLocations.Clear();
            foreach (var location in _settingsManager.Settings.BaseDownloadLocations)
            {
                _downloadLocations.Add(location);
            }
            SettingsDownloadLocationsListBox.ItemsSource = _downloadLocations;
        }

        private void SettingsBrowseArma3_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Arma 3 Executable|arma3_x64.exe;arma3.exe|All Files|*.*",
                Title = "Select Arma 3 Executable"
            };

            if (dialog.ShowDialog() == true)
            {
                SettingsArma3PathTextBox.Text = dialog.FileName;
            }
        }

        private void SettingsAddLocationButton_Click(object sender, RoutedEventArgs e)
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

        private void SettingsRemoveLocationButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsDownloadLocationsListBox.SelectedItem is string selectedLocation)
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

        private void SettingsMoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = SettingsDownloadLocationsListBox.SelectedIndex;
            if (selectedIndex > 0)
            {
                var item = _downloadLocations[selectedIndex];
                _downloadLocations.RemoveAt(selectedIndex);
                _downloadLocations.Insert(selectedIndex - 1, item);
                SettingsDownloadLocationsListBox.SelectedIndex = selectedIndex - 1;
            }
        }

        private void SettingsMoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = SettingsDownloadLocationsListBox.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < _downloadLocations.Count - 1)
            {
                var item = _downloadLocations[selectedIndex];
                _downloadLocations.RemoveAt(selectedIndex);
                _downloadLocations.Insert(selectedIndex + 1, item);
                SettingsDownloadLocationsListBox.SelectedIndex = selectedIndex + 1;
            }
        }

        private void SettingsSave_Click(object sender, RoutedEventArgs e)
        {
            _settingsManager.Settings.Arma3ExePath = SettingsArma3PathTextBox.Text;
            _settingsManager.Settings.BaseDownloadLocations = _downloadLocations.ToList();
            _settingsManager.SaveSettings();
            MessageBox.Show("Settings saved successfully!", "Settings Saved", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Arma 3 LTRM - Lowlands Tactical Repo Manager\n\n" +
                          "A modern FTP-based mod repository manager for Arma 3.\n\n" +
                          "Version 2.0\n" +
                          "© 2024 Lowlands Tactical",
                          "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void DownloadLaunchRepository_Click(object sender, RoutedEventArgs e)
        {
            var selectedRepos = RepositoriesListBox.SelectedItems.Cast<Repository>().ToList();
            if (selectedRepos.Count == 0)
            {
                MessageBox.Show("Please select at least one repository.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateArma3Path())
                return;

            var selectedLocation = SelectDownloadLocation();
            if (selectedLocation == null)
                return;

            var downloadPaths = new List<string>();
            foreach (var repo in selectedRepos)
            {
                var repoPath = Path.Combine(selectedLocation, "Repositories", repo.Name);
                downloadPaths.Add(repoPath);

                var progressWindow = new DownloadProgressWindow();
                progressWindow.Owner = this;

                var progress = new Progress<string>(message => progressWindow.AppendLog(message));

                progressWindow.Show();
                var success = await _ftpManager.DownloadRepositoryAsync(repo, repoPath, progress);
                progressWindow.Close();

                if (!success)
                {
                    MessageBox.Show($"Failed to download repository '{repo.Name}'.", "Download Failed", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            LaunchArma3(downloadPaths);
        }

        private async void DownloadRepository_Click(object sender, RoutedEventArgs e)
        {
            var selectedRepos = RepositoriesListBox.SelectedItems.Cast<Repository>().ToList();
            if (selectedRepos.Count == 0)
            {
                MessageBox.Show("Please select at least one repository.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedLocation = SelectDownloadLocation();
            if (selectedLocation == null)
                return;

            foreach (var repo in selectedRepos)
            {
                var repoPath = Path.Combine(selectedLocation, "Repositories", repo.Name);

                var progressWindow = new DownloadProgressWindow();
                progressWindow.Owner = this;

                var progress = new Progress<string>(message => progressWindow.AppendLog(message));

                progressWindow.Show();
                await _ftpManager.DownloadRepositoryAsync(repo, repoPath, progress, progressWindow.CancellationToken);
                progressWindow.MarkCompleted();
            }

            MessageBox.Show("Repository download completed!", "Download Complete", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LaunchRepository_Click(object sender, RoutedEventArgs e)
        {
            var selectedRepos = RepositoriesListBox.SelectedItems.Cast<Repository>().ToList();
            if (selectedRepos.Count == 0)
            {
                MessageBox.Show("Please select at least one repository.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateArma3Path())
                return;

            var downloadPaths = new List<string>();
            foreach (var repo in selectedRepos)
            {
                var foundPath = FindRepositoryInLocations(repo.Name);
                if (foundPath == null)
                {
                    MessageBox.Show($"Repository '{repo.Name}' has not been downloaded to any configured location.", 
                        "Repository Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                downloadPaths.Add(foundPath);
            }

            LaunchArma3(downloadPaths);
        }

        private async void DownloadLaunchEvent_Click(object sender, RoutedEventArgs e)
        {
            var selectedEvents = EventsListBox.SelectedItems.Cast<Event>().ToList();
            if (selectedEvents.Count == 0)
            {
                MessageBox.Show("Please select at least one event.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateArma3Path())
                return;

            var selectedLocation = SelectDownloadLocation();
            if (selectedLocation == null)
                return;

            var eventPaths = new List<string>();
            foreach (var evt in selectedEvents)
            {
                var progressWindow = new DownloadProgressWindow();
                progressWindow.Owner = this;
                var progress = new Progress<string>(message => progressWindow.AppendLog(message));
                progressWindow.Show();

                foreach (var modFolder in evt.ModFolders)
                {
                    var repository = evt.Repositories.FirstOrDefault(r => r.Id == modFolder.RepositoryId);
                    if (repository == null)
                    {
                        progressWindow.Close();
                        MessageBox.Show($"Repository not found for folder '{modFolder.FolderPath}'.\n\nPlease check your repository configuration.", 
                            "Repository Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Maintain full folder structure from FTP path
                    // e.g., /mods/xyz/@ACE becomes eventBasePath/mods/xyz/@ACE
                    var relativePath = modFolder.FolderPath.TrimStart('/');
                    var localPath = Path.Combine(selectedLocation, relativePath);
                    
                    await _ftpManager.DownloadFolderAsync(
                        repository.Url,
                        repository.Port,
                        repository.Username,
                        repository.Password,
                        modFolder.FolderPath,
                        localPath,
                        progress
                    );
                }

                progressWindow.Close();
                eventPaths.Add(selectedLocation);
            }

            LaunchArma3(eventPaths);
        }

        private async void DownloadEvent_Click(object sender, RoutedEventArgs e)
        {
            var selectedEvents = EventsListBox.SelectedItems.Cast<Event>().ToList();
            if (selectedEvents.Count == 0)
            {
                MessageBox.Show("Please select at least one event.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedLocation = SelectDownloadLocation();
            if (selectedLocation == null)
                return;

            foreach (var evt in selectedEvents)
            {
                var progressWindow = new DownloadProgressWindow();
                progressWindow.Owner = this;
                var progress = new Progress<string>(message => progressWindow.AppendLog(message));
                progressWindow.Show();

                try
                {
                    foreach (var modFolder in evt.ModFolders)
                    {
                        var repository = evt.Repositories.FirstOrDefault(r => r.Id == modFolder.RepositoryId);
                        if (repository == null)
                        {
                            progressWindow.Close();
                            MessageBox.Show($"Repository not found for folder '{modFolder.FolderPath}'.\n\nPlease check your repository configuration.", 
                                "Repository Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Maintain full folder structure from FTP path
                        // e.g., /mods/xyz/@ACE becomes eventBasePath/mods/xyz/@ACE
                        var relativePath = modFolder.FolderPath.TrimStart('/');
                        var localPath = Path.Combine(selectedLocation, relativePath);
                        
                        await _ftpManager.DownloadFolderAsync(
                            repository.Url,
                            repository.Port,
                            repository.Username,
                            repository.Password,
                            modFolder.FolderPath,
                            localPath,
                            progress,
                            progressWindow.CancellationToken
                        );
                    }

                    progressWindow.MarkCompleted();
                }
                catch (OperationCanceledException)
                {
                    progressWindow.Close();
                    return;
                }
            }

            MessageBox.Show("Event download completed!", "Download Complete", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LaunchEvent_Click(object sender, RoutedEventArgs e)
        {
            var selectedEvents = EventsListBox.SelectedItems.Cast<Event>().ToList();
            if (selectedEvents.Count == 0)
            {
                MessageBox.Show("Please select at least one event.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateArma3Path())
                return;

            var eventPaths = new List<string>();
            foreach (var evt in selectedEvents)
            {
                var foundPath = FindEventInLocations(evt);
                if (foundPath == null)
                {
                    MessageBox.Show($"Event '{evt.Name}' has not been downloaded to any configured location.", 
                        "Event Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                eventPaths.Add(foundPath);
            }

            LaunchArma3(eventPaths);
        }

        private void AddRepository_Click(object sender, RoutedEventArgs e)
        {
            var addRepoWindow = new AddRepositoryWindow();
            addRepoWindow.Owner = this;
            if (addRepoWindow.ShowDialog() == true && addRepoWindow.Repository != null)
            {
                _repositoryManager.AddRepository(addRepoWindow.Repository);
            }
        }

        private void EditRepository_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Repository repo)
            {
                var editRepoWindow = new AddRepositoryWindow(repo);
                editRepoWindow.Owner = this;
                if (editRepoWindow.ShowDialog() == true)
                {
                    _repositoryManager.UpdateRepository(repo);
                }
            }
        }

        private void DeleteRepository_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Repository repo)
            {
                var result = MessageBox.Show($"Are you sure you want to delete repository '{repo.Name}'?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    _repositoryManager.RemoveRepository(repo);
                }
            }
        }

        private async void TestRepositoryConnection_Click(object sender, RoutedEventArgs e)
        {
            var selectedRepo = ManageRepositoriesListBox.SelectedItem as Repository;
            if (selectedRepo == null)
            {
                MessageBox.Show("Please select a repository to test.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = false;
                button.Content = "Testing...";
            }

            bool success = await Task.Run(() => _ftpManager.TestConnection(selectedRepo));

            if (button != null)
            {
                button.IsEnabled = true;
                button.Content = "Test Connection";
            }

            if (success)
            {
                MessageBox.Show($"Successfully connected to '{selectedRepo.Name}'!", 
                    "Connection Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Failed to connect to '{selectedRepo.Name}'.\n\nPlease check the connection settings.", 
                    "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            if (_repositoryManager.Repositories.Count == 0)
            {
                MessageBox.Show("Please add at least one repository before creating an event.", 
                    "No Repositories", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var addEventWindow = new AddEditEventWindow(_repositoryManager);
            addEventWindow.Owner = this;
            if (addEventWindow.ShowDialog() == true && addEventWindow.Event != null)
            {
                _eventManager.AddEvent(addEventWindow.Event);
            }
        }

        private void EditEvent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Event evt)
            {
                var editEventWindow = new AddEditEventWindow(_repositoryManager, evt);
                editEventWindow.Owner = this;
                if (editEventWindow.ShowDialog() == true)
                {
                    _eventManager.UpdateEvent(evt);
                }
            }
        }

        private void DeleteEvent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Event evt)
            {
                var result = MessageBox.Show($"Are you sure you want to delete event '{evt.Name}'?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    _eventManager.RemoveEvent(evt);
                }
            }
        }

        private bool ValidateArma3Path()
        {
            if (string.IsNullOrWhiteSpace(_settingsManager.Settings.Arma3ExePath) || 
                !File.Exists(_settingsManager.Settings.Arma3ExePath))
            {
                MessageBox.Show("Arma 3 executable path is not set or invalid.\n\nPlease configure it in the Settings tab.", 
                    "Arma 3 Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private string? SelectDownloadLocation()
        {
            if (_settingsManager.Settings.BaseDownloadLocations == null || _settingsManager.Settings.BaseDownloadLocations.Count == 0)
            {
                MessageBox.Show("No download locations configured.\n\nPlease add at least one download location in Settings.", 
                    "No Download Locations", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            if (_settingsManager.Settings.BaseDownloadLocations.Count == 1)
            {
                return _settingsManager.Settings.BaseDownloadLocations[0];
            }

            var selectLocationWindow = new SelectDownloadLocationWindow(_settingsManager.Settings.BaseDownloadLocations);
            selectLocationWindow.Owner = this;
            if (selectLocationWindow.ShowDialog() == true)
            {
                return selectLocationWindow.SelectedLocation;
            }

            return null;
        }

        private string? FindRepositoryInLocations(string repositoryName)
        {
            foreach (var location in _settingsManager.Settings.BaseDownloadLocations)
            {
                var repoPath = Path.Combine(location, "Repositories", repositoryName);
                if (Directory.Exists(repoPath))
                {
                    return repoPath;
                }
            }
            return null;
        }

        private string? FindEventInLocations(Event evt)
        {
            foreach (var location in _settingsManager.Settings.BaseDownloadLocations)
            {
                bool allFoldersExist = true;
                foreach (var modFolder in evt.ModFolders)
                {
                    var relativePath = modFolder.FolderPath.TrimStart('/');
                    var localPath = Path.Combine(location, relativePath);
                    if (!Directory.Exists(localPath))
                    {
                        allFoldersExist = false;
                        break;
                    }
                }
                
                if (allFoldersExist)
                {
                    return location;
                }
            }
            return null;
        }

        private void LaunchArma3(List<string> modPaths)
        {
            try
            {
                var modFolders = new List<string>();
                
                foreach (var basePath in modPaths)
                {
                    if (Directory.Exists(basePath))
                    {
                        var directories = Directory.GetDirectories(basePath, "*", SearchOption.AllDirectories);
                        foreach (var dir in directories)
                        {
                            if (Path.GetFileName(dir).StartsWith("@"))
                            {
                                modFolders.Add(dir);
                            }
                        }
                    }
                }

                if (modFolders.Count == 0)
                {
                    MessageBox.Show("No mod folders found (folders starting with '@').", 
                        "No Mods Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var modParams = string.Join(";", modFolders);
                var arguments = $"-mod={modParams}";

                var processInfo = new ProcessStartInfo
                {
                    FileName = _settingsManager.Settings.Arma3ExePath,
                    Arguments = arguments,
                    UseShellExecute = false
                };

                Process.Start(processInfo);

                MessageBox.Show($"Arma 3 launched with {modFolders.Count} mod(s)!", 
                    "Launch Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch Arma 3: {ex.Message}", 
                    "Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
