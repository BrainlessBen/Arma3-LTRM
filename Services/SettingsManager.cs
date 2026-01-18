using System.IO;
using System.Text.Json;
using Arma_3_LTRM.Models;

namespace Arma_3_LTRM.Services
{
    public class SettingsManager
    {
        private const string SETTINGS_FILE = "settings.json";
        public AppSettings Settings { get; private set; }

        public SettingsManager()
        {
            Settings = new AppSettings();
            LoadSettings();
        }

        public void SaveSettings()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(Settings, options);
                File.WriteAllText(SETTINGS_FILE, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SETTINGS_FILE))
                {
                    var json = File.ReadAllText(SETTINGS_FILE);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        Settings = settings;
                        
                        // Ensure BaseDownloadLocations is never null
                        if (Settings.BaseDownloadLocations == null)
                        {
                            Settings.BaseDownloadLocations = new List<string>();
                        }
                        
                        // Ensure LaunchParameters is never null
                        if (Settings.LaunchParameters == null)
                        {
                            Settings.LaunchParameters = new LaunchParameters();
                        }
                        
                        // Migration: Convert old single BaseDownloadLocation to new list format
                        if (Settings.BaseDownloadLocations.Count == 0)
                        {
                            Settings.BaseDownloadLocations = new List<string>
                            {
                                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads")
                            };
                            SaveSettings(); // Save migrated settings
                        }
                    }
                }
                else
                {
                    // Ensure default settings have BaseDownloadLocations initialized
                    if (Settings.BaseDownloadLocations == null || Settings.BaseDownloadLocations.Count == 0)
                    {
                        Settings.BaseDownloadLocations = new List<string> 
                        { 
                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads") 
                        };
                    }
                    
                    // Ensure LaunchParameters is initialized
                    if (Settings.LaunchParameters == null)
                    {
                        Settings.LaunchParameters = new LaunchParameters();
                    }
                    
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
        }
    }
}
