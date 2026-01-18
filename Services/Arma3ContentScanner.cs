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
        }

        public class WorkshopItem
        {
            public string FolderName { get; set; } = string.Empty;
            public string FullPath { get; set; } = string.Empty;
        }

        public List<DlcItem> ScanDlcs(string arma3ExePath)
        {
            var dlcList = new List<DlcItem>();

            try
            {
                if (string.IsNullOrEmpty(arma3ExePath) || !File.Exists(arma3ExePath))
                    return dlcList;

                var arma3Directory = Path.GetDirectoryName(arma3ExePath);
                if (string.IsNullOrEmpty(arma3Directory))
                    return dlcList;

                var keysFolder = Path.Combine(arma3Directory, "keys");
                if (!Directory.Exists(keysFolder))
                    return dlcList;

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
            catch
            {
                // Silently fail
            }

            return dlcList.OrderBy(d => d.Name).ToList();
        }

        public List<WorkshopItem> ScanWorkshopItems(string arma3ExePath)
        {
            var workshopList = new List<WorkshopItem>();

            try
            {
                if (string.IsNullOrEmpty(arma3ExePath) || !File.Exists(arma3ExePath))
                    return workshopList;

                var arma3Directory = Path.GetDirectoryName(arma3ExePath);
                if (string.IsNullOrEmpty(arma3Directory))
                    return workshopList;

                var workshopFolder = Path.Combine(arma3Directory, "!Workshop");
                if (!Directory.Exists(workshopFolder))
                    return workshopList;

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
            catch
            {
                // Silently fail
            }

            return workshopList.OrderBy(w => w.FolderName).ToList();
        }
    }
}
