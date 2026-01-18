using System.Windows;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace Arma_3_LTRM.Views
{
    public partial class DownloadProgressWindow : Window
    {
        private CancellationTokenSource? _cancellationTokenSource;
        
        public CancellationToken CancellationToken => _cancellationTokenSource?.Token ?? CancellationToken.None;

        public DownloadProgressWindow()
        {
            InitializeComponent();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void AppendLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressTextBlock.Text += message + Environment.NewLine;
                
                Dispatcher.InvokeAsync(() =>
                {
                    var scrollViewer = FindScrollViewer(ProgressTextBlock);
                    scrollViewer?.ScrollToEnd();
                }, DispatcherPriority.Background);
            });
        }

        public void MarkCompleted()
        {
            Dispatcher.Invoke(() =>
            {
                CancelButton.Content = "Close";
                CancelButton.IsEnabled = true;
            });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (CancelButton.Content.ToString() == "Close")
            {
                Close();
            }
            else
            {
                _cancellationTokenSource?.Cancel();
                CancelButton.IsEnabled = false;
                CancelButton.Content = "Cancelling...";
                AppendLog("Cancellation requested - stopping download...");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var result = MessageBox.Show("Download is in progress. Are you sure you want to cancel?", 
                    "Cancel Download", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                
                _cancellationTokenSource.Cancel();
            }
        }

        private System.Windows.Controls.ScrollViewer? FindScrollViewer(DependencyObject element)
        {
            if (element == null)
                return null;

            var parent = System.Windows.Media.VisualTreeHelper.GetParent(element);
            
            if (parent is System.Windows.Controls.ScrollViewer scrollViewer)
                return scrollViewer;

            return FindScrollViewer(parent);
        }
    }
}

