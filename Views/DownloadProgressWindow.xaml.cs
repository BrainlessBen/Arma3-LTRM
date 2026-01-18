using System.Windows;
using System.Windows.Threading;

namespace Arma_3_LTRM.Views
{
    public partial class DownloadProgressWindow : Window
    {
        public DownloadProgressWindow()
        {
            InitializeComponent();
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
