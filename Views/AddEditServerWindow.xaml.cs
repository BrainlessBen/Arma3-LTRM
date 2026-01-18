using System.Windows;
using Arma_3_LTRM.Models;
using MessageBox = System.Windows.MessageBox;

namespace Arma_3_LTRM.Views
{
    public partial class AddEditServerWindow : Window
    {
        public Server? Server { get; private set; }
        private bool _isEditMode;

        public AddEditServerWindow(Server? server = null)
        {
            InitializeComponent();

            if (server != null)
            {
                _isEditMode = true;
                Server = server;
                Title = "Edit Server";
                LoadServerData();
            }
            else
            {
                _isEditMode = false;
                Server = new Server();
            }
        }

        private void LoadServerData()
        {
            if (Server != null)
            {
                NameTextBox.Text = Server.Name;
                AddressTextBox.Text = Server.Address;
                PortTextBox.Text = Server.Port.ToString();
                PasswordBox.Password = Server.Password;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Please enter a server name.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(AddressTextBox.Text))
            {
                MessageBox.Show("Please enter a server address.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(PortTextBox.Text, out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Please enter a valid port number (1-65535).", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Server != null)
            {
                Server.Name = NameTextBox.Text.Trim();
                Server.Address = AddressTextBox.Text.Trim();
                Server.Port = port;
                Server.Password = PasswordBox.Password;
            }

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
