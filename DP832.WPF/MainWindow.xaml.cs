using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
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

        /// <summary>Initialises the main window and pre-populates the TCPIP subnet field.</summary>
        public MainWindow()
        {
            InitializeComponent();
            TcpipSubnetBox.Text = GetHostIpPrefix() ?? string.Empty;
        }

        /// <summary>Disposes the device connection when the window is closed.</summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (_device is IDisposable disposable)
                disposable.Dispose();
            _device = null;
        }

        /// <summary>Returns the currently selected channel number (1–3).</summary>
        private int GetSelectedChannel()
        {
            return ChannelComboBox.SelectedIndex + 1;
        }

        /// <summary>Enables or disables all controls that require an active device connection.</summary>
        private void SetConnectedState(bool connected)
        {
            ConnectButton.IsEnabled = !connected;
            DisconnectButton.IsEnabled = connected;
            IdentifyButton.IsEnabled = connected;
            RefreshStatusButton.IsEnabled = connected;

            // System settings
            SetBrightnessButton.IsEnabled = connected;
            BeeperOnButton.IsEnabled = connected;
            BeeperOffButton.IsEnabled = connected;
            OtpOnButton.IsEnabled = connected;
            OtpOffButton.IsEnabled = connected;
            ScreenSaverOnButton.IsEnabled = connected;
            ScreenSaverOffButton.IsEnabled = connected;
            TrackModeSyncButton.IsEnabled = connected;
            TrackModeIndeButton.IsEnabled = connected;
            TrackCh1OnButton.IsEnabled = connected;
            TrackCh1OffButton.IsEnabled = connected;
            TrackCh2OnButton.IsEnabled = connected;
            TrackCh2OffButton.IsEnabled = connected;
            ResetButton.IsEnabled = connected;

            // Channel control
            SetVoltageButton.IsEnabled = connected;
            SetCurrentButton.IsEnabled = connected;
            SetOvpLevelButton.IsEnabled = connected;
            OvpOnButton.IsEnabled = connected;
            OvpOffButton.IsEnabled = connected;
            SetOcpLevelButton.IsEnabled = connected;
            OcpOnButton.IsEnabled = connected;
            OcpOffButton.IsEnabled = connected;
            OutputOnButton.IsEnabled = connected;
            OutputOffButton.IsEnabled = connected;
            ClearTripButton.IsEnabled = connected;
        }

        // ── Connection ───────────────────────────────────────────────────────────

        /// <summary>Shows/hides the GPIB or TCPIP input panel when the mode radio buttons change.</summary>
        private void ConnectionMode_Changed(object sender, RoutedEventArgs e)
        {
            if (GpibPanel == null || TcpipPanel == null)
                return;

            bool isGpib = GpibModeRadio.IsChecked == true;
            GpibPanel.Visibility  = isGpib ? Visibility.Visible : Visibility.Collapsed;
            TcpipPanel.Visibility = isGpib ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Builds a full VISA resource string from the currently selected connection mode
        /// and the values entered in the mode-specific input fields.
        /// Returns an empty string when input is insufficient or ambiguous.
        /// </summary>
        private string BuildAddress()
        {
            if (GpibModeRadio.IsChecked == true)
            {
                string raw = GpibDeviceNumberBox.Text.Trim();
                // ResolveAddress with no prefix converts a plain integer to GPIB0::{n}::INSTR.
                return DeviceHelpers.ResolveAddress(raw);
            }
            else
            {
                string prefix = TcpipSubnetBox.Text.Trim();
                string octet  = TcpipLastOctetBox.Text.Trim();

                if (string.IsNullOrEmpty(prefix))
                {
                    // No subnet prefix: require a full host address, not a bare integer (which would
                    // otherwise fall through to GPIB resolution inside ResolveAddress).
                    if (string.IsNullOrWhiteSpace(octet))
                        return string.Empty;

                    int dummy;
                    if (int.TryParse(octet, NumberStyles.Integer, CultureInfo.InvariantCulture, out dummy))
                        return string.Empty;

                    // Non-integer input (e.g. a full IPv4 address) is passed through as-is.
                    return DeviceHelpers.ResolveAddress(octet);
                }

                // When a prefix is supplied, a last-octet value is required.
                if (string.IsNullOrWhiteSpace(octet))
                    return string.Empty;

                // ResolveAddress with a host prefix converts a plain integer to TCPIP::{prefix}.{n}::INSTR.
                return DeviceHelpers.ResolveAddress(octet, prefix);
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string address = BuildAddress();
            if (string.IsNullOrEmpty(address))
            {
                if (TcpipModeRadio.IsChecked == true && string.IsNullOrEmpty(TcpipSubnetBox.Text.Trim()))
                    StatusBarText.Text = "In TCPIP mode, enter a full IPv4 address in the Last Octet field, or fill in the Subnet Prefix.";
                else if (TcpipModeRadio.IsChecked == true)
                    StatusBarText.Text = "Enter the last IP octet (or a full IPv4 address when no subnet prefix is set).";
                else
                    StatusBarText.Text = "Please enter a device address.";
                return;
            }

            IDP832Device candidate = new DP832Device(address);
            try
            {
                candidate.Connect();
                string idn = candidate.GetIdentification();
                _device = candidate;
                StatusText.Text = "Connected: " + idn;
                StatusBarText.Text = "Connected to " + address;
                SetConnectedState(true);
            }
            catch (Exception ex)
            {
                if (candidate is IDisposable disposable)
                    disposable.Dispose();
                StatusText.Text = "Connection failed: " + ex.Message;
                StatusBarText.Text = "Connection failed.";
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
            SetConnectedState(false);
        }

        private void IdentifyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string idn = _device.GetIdentification();
                StatusText.Text = "Device: " + idn;
                StatusBarText.Text = "Identified.";
            }
            catch (Exception ex)
            {
                StatusBarText.Text = "Error: " + ex.Message;
            }
        }

        // ── Status ───────────────────────────────────────────────────────────────

        private void RefreshStatusButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sb = new StringBuilder();

                for (int i = 0; i < 3; i++)
                {
                    int ch = i + 1;
                    string chName = "CH" + ch;
                    sb.AppendLine("=== " + chName + " ===");
                    try
                    {
                        double voltSet  = ParseDouble(_device.SendQuery(":SOURce" + ch + ":VOLTage?"));
                        double voltMeas = ParseDouble(_device.SendQuery(":MEASure:VOLTage? " + chName));
                        double currSet  = ParseDouble(_device.SendQuery(":SOURce" + ch + ":CURRent?"));
                        double currMeas = ParseDouble(_device.SendQuery(":MEASure:CURRent? " + chName));
                        double power    = ParseDouble(_device.SendQuery(":MEASure:POWEr? " + chName));
                        double ovpLevel = ParseDouble(_device.SendQuery(":SOURce" + ch + ":VOLTage:PROTection?"));
                        bool ovpEnabled = DeviceHelpers.ParseProtectionState(_device.SendQuery(":SOURce" + ch + ":VOLTage:PROTection:STATe?"));
                        double ocpLevel = ParseDouble(_device.SendQuery(":SOURce" + ch + ":CURRent:PROTection?"));
                        bool ocpEnabled = DeviceHelpers.ParseProtectionState(_device.SendQuery(":SOURce" + ch + ":CURRent:PROTection:STATe?"));
                        bool outEnabled = DeviceHelpers.ParseProtectionState(_device.SendQuery(":OUTPut? " + chName));

                        bool ovpTripped = false;
                        bool ocpTripped = false;
                        // Trip queries are optional; a failure leaves the value as false (default).
                        try { ovpTripped = DeviceHelpers.ParseProtectionState(_device.SendQuery(":SOURce" + ch + ":VOLTage:PROTection:TRIP?")); } catch { }
                        try { ocpTripped = DeviceHelpers.ParseProtectionState(_device.SendQuery(":SOURce" + ch + ":CURRent:PROTection:TRIP?")); } catch { }

                        sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                            "  Voltage:  {0:F3} V set,  {1:F3} V measured", voltSet, voltMeas));
                        sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                            "  Current:  {0:F3} A set,  {1:F3} A measured", currSet, currMeas));
                        sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                            "  Power:    {0:F3} W measured", power));
                        sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                            "  OVP:      {0:F3} V, {1}{2}",
                            ovpLevel,
                            ovpEnabled ? "Enabled" : "Disabled",
                            ovpTripped ? "  [TRIPPED]" : ""));
                        sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                            "  OCP:      {0:F3} A, {1}{2}",
                            ocpLevel,
                            ocpEnabled ? "Enabled" : "Disabled",
                            ocpTripped ? "  [TRIPPED]" : ""));
                        sb.AppendLine("  Output:   " + (outEnabled ? "ON" : "OFF"));
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine("  Error reading channel: " + ex.Message);
                    }
                    sb.AppendLine();
                }

                sb.AppendLine("=== System ===");
                try { sb.AppendLine("  Track Mode:   " + _device.SendQuery(":SYSTem:TRACKMode?")); }
                catch { sb.AppendLine("  Track Mode:   Error"); }
                try
                {
                    bool t1 = DeviceHelpers.ParseProtectionState(_device.SendQuery(":OUTPut:TRACk? CH1"));
                    bool t2 = DeviceHelpers.ParseProtectionState(_device.SendQuery(":OUTPut:TRACk? CH2"));
                    sb.AppendLine("  Track CH1:    " + (t1 ? "ON" : "OFF"));
                    sb.AppendLine("  Track CH2:    " + (t2 ? "ON" : "OFF"));
                }
                catch { sb.AppendLine("  Track:        Error"); }
                try { sb.AppendLine("  OTP:          " + (DeviceHelpers.ParseProtectionState(_device.SendQuery(":SYSTem:OTP?")) ? "Enabled" : "Disabled")); }
                catch { sb.AppendLine("  OTP:          Error"); }
                try { sb.AppendLine("  Beeper:       " + (DeviceHelpers.ParseProtectionState(_device.SendQuery(":SYSTem:BEEPer?")) ? "Enabled" : "Disabled")); }
                catch { sb.AppendLine("  Beeper:       Error"); }
                try { sb.AppendLine("  Brightness:   " + _device.SendQuery(":SYSTem:BRIGhtness?").Trim() + "%"); }
                catch { sb.AppendLine("  Brightness:   Error"); }
                try { sb.AppendLine("  Screen Saver: " + (DeviceHelpers.ParseProtectionState(_device.SendQuery(":SYSTem:SAVer?")) ? "Enabled" : "Disabled")); }
                catch { sb.AppendLine("  Screen Saver: Error"); }

                StatusText.Text = sb.ToString();
                StatusBarText.Text = "Status refreshed.";
            }
            catch (Exception ex)
            {
                StatusBarText.Text = "Error refreshing status: " + ex.Message;
            }
        }

        // ── Channel Control ─────────────────────────────────────────────────────

        private void SetVoltageButton_Click(object sender, RoutedEventArgs e)
        {
            int ch = GetSelectedChannel();
            double voltage;
            if (!double.TryParse(VoltageBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out voltage))
            {
                StatusBarText.Text = "Invalid voltage value.";
                return;
            }
            if (!DeviceHelpers.IsValidVoltage(voltage, ch))
            {
                double maxV = DeviceHelpers.GetChannelMaxVoltage(ch);
                StatusBarText.Text = string.Format(CultureInfo.InvariantCulture,
                    "Voltage must be 0\u2013{0} V for CH{1}.", maxV, ch);
                return;
            }
            try
            {
                _device.SendCommand(string.Format(CultureInfo.InvariantCulture,
                    ":SOURce{0}:VOLTage {1:F3}", ch, voltage));
                StatusBarText.Text = string.Format(CultureInfo.InvariantCulture,
                    "CH{0} voltage set to {1:F3} V.", ch, voltage);
            }
            catch (Exception ex) { StatusBarText.Text = "Error: " + ex.Message; }
        }

        private void SetCurrentButton_Click(object sender, RoutedEventArgs e)
        {
            int ch = GetSelectedChannel();
            double current;
            if (!double.TryParse(CurrentBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out current))
            {
                StatusBarText.Text = "Invalid current value.";
                return;
            }
            if (!DeviceHelpers.IsValidCurrent(current))
            {
                StatusBarText.Text = string.Format(CultureInfo.InvariantCulture,
                    "Current must be 0\u2013{0} A.", DeviceHelpers.GetChannelMaxCurrent());
                return;
            }
            try
            {
                _device.SendCommand(string.Format(CultureInfo.InvariantCulture,
                    ":SOURce{0}:CURRent {1:F3}", ch, current));
                StatusBarText.Text = string.Format(CultureInfo.InvariantCulture,
                    "CH{0} current limit set to {1:F3} A.", ch, current);
            }
            catch (Exception ex) { StatusBarText.Text = "Error: " + ex.Message; }
        }

        private void SetOvpLevelButton_Click(object sender, RoutedEventArgs e)
        {
            int ch = GetSelectedChannel();
            double level;
            if (!double.TryParse(OvpLevelBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out level))
            {
                StatusBarText.Text = "Invalid OVP level.";
                return;
            }
            if (!DeviceHelpers.IsValidOvpLevel(level, ch))
            {
                double maxV = DeviceHelpers.GetChannelMaxVoltage(ch) + 1;
                StatusBarText.Text = string.Format(CultureInfo.InvariantCulture,
                    "OVP level must be 0.01\u2013{0} V for CH{1}.", maxV, ch);
                return;
            }
            try
            {
                _device.SendCommand(string.Format(CultureInfo.InvariantCulture,
                    ":SOURce{0}:VOLTage:PROTection {1:F3}", ch, level));
                StatusBarText.Text = string.Format(CultureInfo.InvariantCulture,
                    "CH{0} OVP level set to {1:F3} V.", ch, level);
            }
            catch (Exception ex) { StatusBarText.Text = "Error: " + ex.Message; }
        }

        private void OvpOnButton_Click(object sender, RoutedEventArgs e)
        {
            SetOvpState(true);
        }

        private void OvpOffButton_Click(object sender, RoutedEventArgs e)
        {
            SetOvpState(false);
        }

        private void SetOvpState(bool enabled)
        {
            int ch = GetSelectedChannel();
            try
            {
                _device.SendCommand(":SOURce" + ch + ":VOLTage:PROTection:STATe " + (enabled ? "ON" : "OFF"));
                StatusBarText.Text = "CH" + ch + " OVP " + (enabled ? "enabled" : "disabled") + ".";
            }
            catch (Exception ex) { StatusBarText.Text = "Error: " + ex.Message; }
        }

        private void SetOcpLevelButton_Click(object sender, RoutedEventArgs e)
        {
            int ch = GetSelectedChannel();
            double level;
            if (!double.TryParse(OcpLevelBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out level))
            {
                StatusBarText.Text = "Invalid OCP level.";
                return;
            }
            if (!DeviceHelpers.IsValidOcpLevel(level))
            {
                double maxA = DeviceHelpers.GetChannelMaxCurrent() + 1;
                StatusBarText.Text = string.Format(CultureInfo.InvariantCulture,
                    "OCP level must be 0.001\u2013{0} A.", maxA);
                return;
            }
            try
            {
                _device.SendCommand(string.Format(CultureInfo.InvariantCulture,
                    ":SOURce{0}:CURRent:PROTection {1:F3}", ch, level));
                StatusBarText.Text = string.Format(CultureInfo.InvariantCulture,
                    "CH{0} OCP level set to {1:F3} A.", ch, level);
            }
            catch (Exception ex) { StatusBarText.Text = "Error: " + ex.Message; }
        }

        private void OcpOnButton_Click(object sender, RoutedEventArgs e)
        {
            SetOcpState(true);
        }

        private void OcpOffButton_Click(object sender, RoutedEventArgs e)
        {
            SetOcpState(false);
        }

        private void SetOcpState(bool enabled)
        {
            int ch = GetSelectedChannel();
            try
            {
                _device.SendCommand(":SOURce" + ch + ":CURRent:PROTection:STATe " + (enabled ? "ON" : "OFF"));
                StatusBarText.Text = "CH" + ch + " OCP " + (enabled ? "enabled" : "disabled") + ".";
            }
            catch (Exception ex) { StatusBarText.Text = "Error: " + ex.Message; }
        }

        private void OutputOnButton_Click(object sender, RoutedEventArgs e)
        {
            SetOutput(true);
        }

        private void OutputOffButton_Click(object sender, RoutedEventArgs e)
        {
            SetOutput(false);
        }

        private void SetOutput(bool enabled)
        {
            int ch = GetSelectedChannel();
            try
            {
                _device.SendCommand(":OUTPut CH" + ch + "," + (enabled ? "ON" : "OFF"));
                StatusBarText.Text = "CH" + ch + " output " + (enabled ? "enabled" : "disabled") + ".";
            }
            catch (Exception ex) { StatusBarText.Text = "Error: " + ex.Message; }
        }

        private void ClearTripButton_Click(object sender, RoutedEventArgs e)
        {
            int ch = GetSelectedChannel();
            string chName = "CH" + ch;
            try
            {
                bool ovpTripped = false;
                bool ocpTripped = false;
                // Trip queries are optional; a failure leaves the value as false (default).
                try { ovpTripped = DeviceHelpers.ParseProtectionState(_device.SendQuery(":SOURce" + ch + ":VOLTage:PROTection:TRIP?")); } catch { }
                try { ocpTripped = DeviceHelpers.ParseProtectionState(_device.SendQuery(":SOURce" + ch + ":CURRent:PROTection:TRIP?")); } catch { }

                if (!ovpTripped && !ocpTripped)
                {
                    StatusBarText.Text = "No protection trips detected on " + chName + ".";
                    return;
                }

                // Clear commands are attempted independently; a failure on one does not block the other.
                if (ovpTripped)
                    try { _device.SendCommand(":OUTPut:OVP:CLEar " + chName); } catch { }
                if (ocpTripped)
                    try { _device.SendCommand(":OUTPut:OCP:CLEar " + chName); } catch { }

                StatusBarText.Text = chName + " protection trips cleared.";
            }
            catch (Exception ex) { StatusBarText.Text = "Error: " + ex.Message; }
        }

        // ── System Settings ─────────────────────────────────────────────────────

        private void SetBrightnessButton_Click(object sender, RoutedEventArgs e)
        {
            int brightness;
            if (!int.TryParse(BrightnessBox.Text, out brightness))
            {
                StatusBarText.Text = "Invalid brightness value.";
                return;
            }
            if (!DeviceHelpers.IsValidBrightness(brightness))
            {
                StatusBarText.Text = "Brightness must be 1\u2013100.";
                return;
            }
            try
            {
                _device.SendCommand(":SYSTem:BRIGhtness " + brightness.ToString(CultureInfo.InvariantCulture));
                StatusBarText.Text = "Brightness set to " + brightness + "%.";
            }
            catch (Exception ex) { StatusBarText.Text = "Error: " + ex.Message; }
        }

        private void BeeperOnButton_Click(object sender, RoutedEventArgs e)
        {
            SetBeeper(true);
        }

        private void BeeperOffButton_Click(object sender, RoutedEventArgs e)
        {
            SetBeeper(false);
        }

        private void SetBeeper(bool enabled)
        {
            try
            {
                _device.SendCommand(":SYSTem:BEEPer " + (enabled ? "ON" : "OFF"));
                StatusBarText.Text = "Beeper " + (enabled ? "enabled" : "disabled") + ".";
            }
            catch (Exception ex) { StatusBarText.Text = "Error: " + ex.Message; }
        }

        private void OtpOnButton_Click(object sender, RoutedEventArgs e)
        {
            SetOtp(true);
        }

        private void OtpOffButton_Click(object sender, RoutedEventArgs e)
        {
            SetOtp(false);
        }

        private void SetOtp(bool enabled)
        {
            try
            {
                _device.SendCommand(":SYSTem:OTP " + (enabled ? "ON" : "OFF"));
                StatusBarText.Text = "OTP " + (enabled ? "enabled" : "disabled") + ".";
            }
            catch (Exception ex) { StatusBarText.Text = "Error: " + ex.Message; }
        }

        private void ScreenSaverOnButton_Click(object sender, RoutedEventArgs e)
        {
            SetScreenSaver(true);
        }

        private void ScreenSaverOffButton_Click(object sender, RoutedEventArgs e)
        {
            SetScreenSaver(false);
        }

        private void SetScreenSaver(bool enabled)
        {
            try
            {
                _device.SendCommand(":SYSTem:SAVer " + (enabled ? "ON" : "OFF"));
                StatusBarText.Text = "Screen saver " + (enabled ? "enabled" : "disabled") + ".";
            }
            catch (Exception ex) { StatusBarText.Text = "Error: " + ex.Message; }
        }

        private void TrackModeSyncButton_Click(object sender, RoutedEventArgs e)
        {
            SetTrackingMode("SYNC");
        }

        private void TrackModeIndeButton_Click(object sender, RoutedEventArgs e)
        {
            SetTrackingMode("INDE");
        }

        private void SetTrackingMode(string mode)
        {
            try
            {
                _device.SendCommand(":SYSTem:TRACKMode " + mode);
                StatusBarText.Text = "Track mode set to " + mode + ".";
            }
            catch (Exception ex) { StatusBarText.Text = "Error: " + ex.Message; }
        }

        private void TrackCh1OnButton_Click(object sender, RoutedEventArgs e)
        {
            SetTrack(1, true);
        }

        private void TrackCh1OffButton_Click(object sender, RoutedEventArgs e)
        {
            SetTrack(1, false);
        }

        private void TrackCh2OnButton_Click(object sender, RoutedEventArgs e)
        {
            SetTrack(2, true);
        }

        private void TrackCh2OffButton_Click(object sender, RoutedEventArgs e)
        {
            SetTrack(2, false);
        }

        private void SetTrack(int channel, bool enabled)
        {
            try
            {
                _device.SendCommand(":OUTPut:TRACk CH" + channel + "," + (enabled ? "ON" : "OFF"));
                StatusBarText.Text = "CH" + channel + " tracking " + (enabled ? "enabled" : "disabled") + ".";
            }
            catch (Exception ex) { StatusBarText.Text = "Error: " + ex.Message; }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                    "Reset the device to factory defaults?",
                    "Confirm Reset",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
            try
            {
                _device.SendCommand("*RST");
                StatusBarText.Text = "Device reset to factory defaults.";
            }
            catch (Exception ex) { StatusBarText.Text = "Error: " + ex.Message; }
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Parses a SCPI query response string to a double using invariant culture.
        /// Returns 0.0 if the string cannot be parsed.
        /// </summary>
        private static double ParseDouble(string raw)
        {
            double value;
            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return value;
            return 0.0;
        }

        /// <summary>
        /// Returns the first three octets (e.g. <c>192.168.1</c>) of the host machine's
        /// first active non-loopback IPv4 network interface, or <see langword="null"/> if
        /// no such interface is found.
        /// </summary>
        private static string GetHostIpPrefix()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up ||
                    ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                IPInterfaceProperties props;
                try { props = ni.GetIPProperties(); }
                catch (NetworkInformationException) { continue; }

                foreach (UnicastIPAddressInformation addr in props.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    string[] octets = addr.Address.ToString().Split('.');
                    if (octets.Length == 4)
                        return $"{octets[0]}.{octets[1]}.{octets[2]}";
                }
            }
            return null;
        }
    }
}
