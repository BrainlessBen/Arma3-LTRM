using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using Arma_3_LTRM.Models;

namespace Arma_3_LTRM.Services
{
    public class ServerManager
    {
        private const string SETTINGS_FOLDER = "Settings";
        private const string SERVERS_FILE = "servers.json";
        public ObservableCollection<Server> Servers { get; private set; }

        public ServerManager()
        {
            Servers = new ObservableCollection<Server>();
            LoadServers();
        }

        public void AddServer(Server server)
        {
            Servers.Add(server);
            SaveServers();
        }

        public void RemoveServer(Server server)
        {
            Servers.Remove(server);
            SaveServers();
        }

        public void UpdateServer(Server server)
        {
            SaveServers();
        }

        private void SaveServers()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                if (!Directory.Exists(SETTINGS_FOLDER))
                {
                    Directory.CreateDirectory(SETTINGS_FOLDER);
                }

                var filePath = Path.Combine(SETTINGS_FOLDER, SERVERS_FILE);
                var json = JsonSerializer.Serialize(Servers, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving servers: {ex.Message}");
            }
        }

        private void LoadServers()
        {
            try
            {
                var filePath = Path.Combine(SETTINGS_FOLDER, SERVERS_FILE);
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var servers = JsonSerializer.Deserialize<ObservableCollection<Server>>(json);
                    if (servers != null)
                    {
                        Servers = servers;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading servers: {ex.Message}");
            }
        }
    }
}
