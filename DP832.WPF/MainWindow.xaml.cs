using System.Windows;
using DP832.Core;
using DP832.Helpers;

namespace DP832.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml — entry point for the DP832 WPF front-end.
    /// Uses <see cref="DP832Device"/> from <c>DP832.Core</c> for instrument communication
    /// and <see cref="DeviceHelpers"/> from <c>DP832.Helpers</c> for address formatting
    /// and value validation.
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDP832Device _device;

        /// <summary>Initialises the main window.</summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string address = DeviceAddressBox.Text.Trim();
            if (string.IsNullOrEmpty(address))
            {
                StatusBarText.Text = "Please enter a device address.";
                return;
            }

            try
            {
                _device = new DP832Device(address);
                _device.Connect();
                string idn = _device.GetIdentification();
                StatusText.Text = "Connected: " + idn;
                StatusBarText.Text = "Connected to " + address;
                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
            }
            catch (System.Exception ex)
            {
                StatusText.Text = "Connection failed: " + ex.Message;
                StatusBarText.Text = "Connection failed.";
                _device = null;
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_device != null)
            {
                _device.Disconnect();
                _device = null;
            }
            StatusText.Text = "Not connected. Enter a device address and click Connect.";
            StatusBarText.Text = "Disconnected.";
            ConnectButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
        }
    }
}
