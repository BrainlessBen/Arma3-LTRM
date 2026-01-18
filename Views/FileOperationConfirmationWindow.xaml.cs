using System.Windows;
using Arma_3_LTRM.Models;

namespace Arma_3_LTRM.Views
{
    public partial class FileOperationConfirmationWindow : Window
    {
        public bool UserConfirmed { get; private set; }

        public FileOperationConfirmationWindow(FileOperationAnalysis analysis)
        {
            InitializeComponent();
            
            // Set the counts
            CreateCountText.Text = $"{analysis.FilesToCreate} file{(analysis.FilesToCreate != 1 ? "s" : "")}";
            ModifyCountText.Text = $"{analysis.FilesToModify} file{(analysis.FilesToModify != 1 ? "s" : "")}";
            DeleteCountText.Text = $"{analysis.FilesToDelete} file{(analysis.FilesToDelete != 1 ? "s" : "")}";
            DownloadSizeText.Text = FormatFileSize(analysis.TotalDownloadSize);
            
            UserConfirmed = false;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            UserConfirmed = true;
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            UserConfirmed = false;
            DialogResult = false;
            Close();
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
