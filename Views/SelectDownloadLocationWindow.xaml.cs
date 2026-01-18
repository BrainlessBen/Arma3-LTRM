using System.Windows;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;

namespace Arma_3_LTRM.Views
{
    public partial class SelectDownloadLocationWindow : Window
    {
        public string? SelectedLocation { get; private set; }

        public SelectDownloadLocationWindow(List<string> locations)
        {
            InitializeComponent();
            LocationsListBox.ItemsSource = locations;
            
            if (locations.Count > 0)
            {
                LocationsListBox.SelectedIndex = 0;
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (LocationsListBox.SelectedItem is string selectedLocation)
            {
                SelectedLocation = selectedLocation;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a location.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void LocationsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LocationsListBox.SelectedItem is string selectedLocation)
            {
                SelectedLocation = selectedLocation;
                DialogResult = true;
                Close();
            }
        }
    }
}
