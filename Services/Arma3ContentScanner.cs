using System.IO;
using Arma_3_LTRM.Models;

namespace Arma_3_LTRM.Services
{
    public class Arma3ContentScanner
    {
        public class DlcItem
        {
            public string Name { get; set; } = string.Empty;
            public string FolderName { get; set; } = string.Empty;
            public string KeyFileName { get; set; } = string.Empty;
            public bool IsError { get; set; } = false;
        }

        public class WorkshopItem
        {
            public string FolderName { get; set; } = string.Empty;
            public string FullPath { get; set; } = string.Empty;
            public bool IsError { get; set; } = false;
        }

        public List<DlcItem> ScanDlcs(string arma3ExePath)
        {
            var dlcList = new List<DlcItem>();

            try
            {
                if (string.IsNullOrEmpty(arma3ExePath))
                {
                    dlcList.Add(new DlcItem
                    {
                        Name = "Error: Arma 3 path not configured",
                        FolderName = string.Empty,
                        KeyFileName = "Please set the Arma 3 executable path in Settings",
                        IsError = true
                    });
                    return dlcList;
                }

                if (!File.Exists(arma3ExePath))
                {
                    dlcList.Add(new DlcItem
                    {
                        Name = "Error: Arma 3 executable not found",
                        FolderName = string.Empty,
                        KeyFileName = $"Path: {arma3ExePath}",
                        IsError = true
                    });
                    return dlcList;
                }

                var arma3Directory = Path.GetDirectoryName(arma3ExePath);
                if (string.IsNullOrEmpty(arma3Directory))
                {
                    dlcList.Add(new DlcItem
                    {
                        Name = "Error: Invalid Arma 3 path",
                        FolderName = string.Empty,
                        KeyFileName = "Cannot determine directory from executable path",
                        IsError = true
                    });
                    return dlcList;
                }

                var keysFolder = Path.Combine(arma3Directory, "keys");
                if (!Directory.Exists(keysFolder))
                {
                    dlcList.Add(new DlcItem
                    {
                        Name = "Error: Keys folder not found",
                        FolderName = string.Empty,
                        KeyFileName = $"Expected at: {keysFolder}",
                        IsError = true
                    });
                    return dlcList;
                }

                var keyFiles = Directory.GetFiles(keysFolder, "*.bikey");

                foreach (var keyFile in keyFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(keyFile);
                    
                    // Ignore a3.bikey
                    if (fileName.Equals("a3", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Get the uppercase folder name
                    var folderName = fileName.ToUpper();

                    // Special case: Replace a3c with contact
                    if (fileName.Equals("a3c", StringComparison.OrdinalIgnoreCase))
                    {
                        folderName = "CONTACT";
                    }

                    // Check if the DLC folder exists
                    var dlcFolderPath = Path.Combine(arma3Directory, folderName);
                    if (Directory.Exists(dlcFolderPath))
                    {
                        dlcList.Add(new DlcItem
                        {
                            Name = folderName,
                            FolderName = folderName,
                            KeyFileName = Path.GetFileName(keyFile)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                dlcList.Clear();
                dlcList.Add(new DlcItem
                {
                    Name = "Error: Failed to scan DLCs",
                    FolderName = string.Empty,
                    KeyFileName = ex.Message,
                    IsError = true
                });
                return dlcList;
            }

            if (dlcList.Count == 0)
            {
                dlcList.Add(new DlcItem
                {
                    Name = "No DLCs found",
                    FolderName = string.Empty,
                    KeyFileName = "No DLC key files found in keys folder",
                    IsError = true
                });
            }

            return dlcList.OrderBy(d => d.Name).ToList();
        }

        public List<WorkshopItem> ScanWorkshopItems(string arma3ExePath)
        {
            var workshopList = new List<WorkshopItem>();

            try
            {
                if (string.IsNullOrEmpty(arma3ExePath))
                {
                    workshopList.Add(new WorkshopItem
                    {
                        FolderName = "Error: Arma 3 path not configured",
                        FullPath = "Please set the Arma 3 executable path in Settings",
                        IsError = true
                    });
                    return workshopList;
                }

                if (!File.Exists(arma3ExePath))
                {
                    workshopList.Add(new WorkshopItem
                    {
                        FolderName = "Error: Arma 3 executable not found",
                        FullPath = $"Path: {arma3ExePath}",
                        IsError = true
                    });
                    return workshopList;
                }

                var arma3Directory = Path.GetDirectoryName(arma3ExePath);
                if (string.IsNullOrEmpty(arma3Directory))
                {
                    workshopList.Add(new WorkshopItem
                    {
                        FolderName = "Error: Invalid Arma 3 path",
                        FullPath = "Cannot determine directory from executable path",
                        IsError = true
                    });
                    return workshopList;
                }

                var workshopFolder = Path.Combine(arma3Directory, "!Workshop");
                if (!Directory.Exists(workshopFolder))
                {
                    workshopList.Add(new WorkshopItem
                    {
                        FolderName = "No Workshop folder found",
                        FullPath = $"Expected at: {workshopFolder}",
                        IsError = true
                    });
                    return workshopList;
                }

                var subFolders = Directory.GetDirectories(workshopFolder);

                foreach (var folder in subFolders)
                {
                    var folderName = Path.GetFileName(folder);

                    // Ignore folders starting with !
                    if (folderName.StartsWith("!"))
                        continue;

                    workshopList.Add(new WorkshopItem
                    {
                        FolderName = folderName,
                        FullPath = folder
                    });
                }
            }
            catch (Exception ex)
            {
                workshopList.Clear();
                workshopList.Add(new WorkshopItem
                {
                    FolderName = "Error: Failed to scan Workshop items",
                    FullPath = ex.Message,
                    IsError = true
                });
                return workshopList;
            }

            if (workshopList.Count == 0)
            {
                workshopList.Add(new WorkshopItem
                {
                    FolderName = "No Workshop items found",
                    FullPath = "!Workshop folder is empty or contains no valid items",
                    IsError = true
                });
            }

            return workshopList.OrderBy(w => w.FolderName).ToList();
        }
    }
}
