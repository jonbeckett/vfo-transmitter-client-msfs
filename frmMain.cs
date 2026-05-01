using System;
using System.Windows.Forms;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace VirtualFlightOnlineTransmitter
{
    /// <summary>
    /// Main application window for the VirtualFlight.Online Transmitter.
    /// Connects to Microsoft Flight Simulator via SimConnect, reads live aircraft data
    /// once per second, displays it on screen, and transmits it to the VFO server over HTTP.
    /// </summary>
    public partial class frmMain : Form
    {
        // Default server profile used when no saved configuration exists.
        private const string DefaultServersJson = "[{ \"serverName\":\"VirtualFlight.Online\",\"callsign\":\"Callsign\",\"pilotName\":\"Pilot Name\",\"groupName\":\"VirtualFlight.Online\",\"notes\":\"\",\"msfsServer\":\"WEST EUROPE\",\"serverURL\":\"https://transmitter.virtualflight.online/transmit\",\"pin\":\"\"}]";

        // Handles all SimConnect communication — connecting, requesting data, and disconnecting.
        private readonly SimConnectClient _simConnectClient = new SimConnectClient();

        // Sends position snapshots to the VFO server asynchronously.
        private readonly HttpTransmitter _httpTransmitter = new HttpTransmitter();

        // Guards against overlapping HTTP transmissions when the server is slow to respond.
        // The transmit timer fires every second; without this flag, requests could queue up.
        private bool _isTransmitting;

        // Records when the current simulator session started, used to display elapsed time.
        private DateTime ConnectionStartTime { get; set; }

        

        /// <summary>
        /// Converts a decimal longitude value to a formatted degrees/minutes/seconds string.
        /// </summary>
        /// <param name="val">Longitude in decimal degrees. Positive = East, negative = West.</param>
        /// <returns>A string in the form "51° 30' 00.00\" E".</returns>
        string LongitudeToString(double val)
        {
            double abs = Math.Abs(val);
            int d = (int)abs;
            int m = (int)((abs - d) * 60);
            double s = (((abs - d) * 60) - m) * 60;
            return d + "° " + m + "' " + string.Format("{0:0.00}", s) + "\" " + (val >= 0 ? "E" : "W");
        }

        /// <summary>
        /// Converts a decimal latitude value to a formatted degrees/minutes/seconds string.
        /// </summary>
        /// <param name="val">Latitude in decimal degrees. Positive = North, negative = South.</param>
        /// <returns>A string in the form "51° 30' 00.00\" N".</returns>
        string LatitudeToString(double val)
        {
            double abs = Math.Abs(val);
            int d = (int)abs;
            int m = (int)((abs - d) * 60);
            double s = (((abs - d) * 60) - m) * 60;
            return d + "° " + m + "' " + string.Format("{0:0.00}", s) + "\" " + (val >= 0 ? "N" : "S");
        }


        /// <summary>
        /// Constructor for the Form
        /// </summary>
        public frmMain()
        {
            InitializeComponent();

            _simConnectClient.DataReceived += HandleDataReceived;
        }

        /// <summary>
        /// Forwards Win32 messages to SimConnectClient so it can dispatch incoming data callbacks.
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == SimConnectClient.WM_USER_SIMCONNECT)
                _simConnectClient.HandleWindowMessage();

            base.WndProc(ref m);
        }

        /// <summary>
        /// Loads the server list from application settings, falling back to the built-in
        /// default if the setting is missing or unparseable.
        /// </summary>
        private JsonArray GetServersFromSettings()
        {
            string serversText = Properties.Settings.Default["servers"]?.ToString();
            if (string.IsNullOrWhiteSpace(serversText))
            {
                serversText = DefaultServersJson;
            }

            JsonArray servers = JsonNode.Parse(serversText)?.AsArray();
            if (servers == null || servers.Count == 0)
            {
                servers = JsonNode.Parse(DefaultServersJson).AsArray();
            }

            return servers;
        }

        /// <summary>
        /// Returns the index of the currently selected server, clamped to valid bounds.
        /// Falls back to 0 if the saved index is out of range.
        /// </summary>
        private int GetSelectedServerIndex(JsonArray servers)
        {
            int selectedServerIndex = 0;
            int.TryParse(Properties.Settings.Default["selectedServer"]?.ToString(), out selectedServerIndex);

            if (selectedServerIndex < 0 || selectedServerIndex >= servers.Count)
            {
                selectedServerIndex = 0;
            }

            return selectedServerIndex;
        }

        /// <summary>
        /// Persists all fields of the selected server profile into application settings.
        /// Settings are stored both inside the JSON server array and as individual flat keys
        /// so that <see cref="HttpTransmitter"/> can read them without deserialising the full list.
        /// </summary>
        private void SaveSelectedServer(JsonArray servers, int selectedServerIndex)
        {
            Server selectedServer = servers[selectedServerIndex].Deserialize<Server>();

            Properties.Settings.Default["selectedServer"] = selectedServerIndex.ToString();
            Properties.Settings.Default["serverName"] = selectedServer.ServerName;
            Properties.Settings.Default["serverURL"] = selectedServer.ServerUrl;
            Properties.Settings.Default["pin"] = selectedServer.Pin;
            Properties.Settings.Default["callsign"] = selectedServer.Callsign;
            Properties.Settings.Default["pilotName"] = selectedServer.PilotName;
            Properties.Settings.Default["groupName"] = selectedServer.GroupName;
            Properties.Settings.Default["msfsServer"] = selectedServer.MsfsServer;
            Properties.Settings.Default["notes"] = selectedServer.Notes;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Convenience helper that loads the server list and resolves the selected index in one call.
        /// Returns <c>false</c> if no valid server configuration is available.
        /// </summary>
        private bool TryGetSelectedServer(out JsonArray servers, out int selectedServerIndex)
        {
            servers = GetServersFromSettings();
            selectedServerIndex = servers.Count > 0 ? GetSelectedServerIndex(servers) : -1;
            return servers.Count > 0 && selectedServerIndex >= 0;
        }

        /// <summary>
        /// Updates a single field in both the JSON server array and the flat application settings,
        /// then saves. Called by every TextChanged / SelectedIndexChanged handler that needs
        /// to persist a user edit immediately.
        /// </summary>
        /// <param name="jsonKey">The key name inside the server JSON object (camelCase).</param>
        /// <param name="settingsKey">The corresponding flat application settings key.</param>
        /// <param name="value">The new value to store.</param>
        private void SaveFieldToSettings(string jsonKey, string settingsKey, string value)
        {
            if (TryGetSelectedServer(out JsonArray servers, out int selectedServerIndex))
            {
                servers[selectedServerIndex][jsonKey] = value;
                Properties.Settings.Default["servers"] = servers.ToJsonString();
                Properties.Settings.Default[settingsKey] = value;
                Properties.Settings.Default.Save();
            }
        }


        /// <summary>
        /// Handles clicks on the Connect button. Delegates to <see cref="Connect"/>.
        /// </summary>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!_simConnectClient.Connected)
            {
                Connect();
            }
        }


        /// <summary>
        /// Fires every second (set in the designer) while connected.
        /// Asks SimConnect for a fresh data snapshot; the response arrives
        /// asynchronously and is handled by <see cref="HandleDataReceived"/>.
        /// </summary>
        private void tmrTransmit_Tick(object sender, EventArgs e)
        {
            if (_simConnectClient.Connected)
            {
                // Request a fresh data snapshot from the simulator
                try
                {
                    _simConnectClient.RequestData();
                }
                catch
                {
                    tsslSimulatorStatus.Text = "Cannot connect to simulator";
                    Disconnect("Problem transmitting data simulator." + ((Properties.Settings.Default["AutoConnect"].ToString().ToLower() == "true") ? " - retrying every 5 seconds" : ""));
                }
            }
        }


        /// <summary>
        /// Handles aircraft data received from SimConnect, updates the UI and transmits to the server.
        /// Groundspeed is already in knots (converted in SimConnectClient).
        /// </summary>
        private async void HandleDataReceived(object sender, PlaneData data)
        {
            // Update the screen immediately on every tick
            this.tbAircraftType.Text      = data.AircraftType;
            this.tbLatitude.Text          = LatitudeToString(data.Latitude);
            this.tbLongitude.Text         = LongitudeToString(data.Longitude);
            this.tbAltitude.Text          = string.Format("{0:0. ft}", data.Altitude);
            this.tbHeading.Text           = string.Format("{0:0. deg}", data.Heading);
            this.tbAirspeed.Text          = string.Format("{0:0. knots}", data.Airspeed);
            this.tbGroundspeed.Text       = string.Format("{0:0. knots}", data.Groundspeed);
            this.tbTouchdownVelocity.Text = string.Format("{0:0. ft/min}", data.TouchdownVelocity * 60);

            this.tsslSimulatorStatus.Text = "LON : " + this.tbLongitude.Text + " - LAT : " + this.tbLatitude.Text + " - ALT : " + this.tbAltitude.Text + " - HDG : " + this.tbHeading.Text + " - GS : " + this.tbGroundspeed.Text + " - XPDR: " + data.TransponderCode;

            // Skip transmission if one is already in flight to avoid overlapping requests
            if (_isTransmitting) return;

            _isTransmitting = true;
            tsslCommunicationsStatus.Text = "Sending...";
            try
            {
                string status = await _httpTransmitter.TransmitAsync(data, tbNotes.Text, Application.ProductVersion);
                tsslCommunicationsStatus.Text = status;
                tsslMain.Text = DateTime.Now.Subtract(ConnectionStartTime).ToString(@"hh\:mm\:ss");
            }
            finally
            {
                _isTransmitting = false;
            }
        }


        /// <summary>
        /// Handle closing of the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If the simulator is connected, ask the user if they really want to close Transmitter
            if (_simConnectClient.Connected)
            {
                DialogResult result = MessageBox.Show("Transmitter is still connected to the simulator - are you sure you want to close it?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // disconnect from the simulator
                    this.tmrTransmit.Stop();
                    this.tmrConnect.Stop();
                    this.Disconnect(string.Empty);

                }
                else
                {
                    // cancel the form closure
                    e.Cancel = true;
                }
            }
        }


        /// <summary>
        /// Handle loading the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_Load(object sender, EventArgs e)
        {
            // pre-fill the settings boxes with data from properties

            JsonArray servers = GetServersFromSettings();
            int selectedServerIndex = GetSelectedServerIndex(servers);
            Server selectedServer = servers[selectedServerIndex].Deserialize<Server>();

            // populate the textboxes
            tbCallsign.Text = selectedServer.Callsign;
            tbPilotName.Text = selectedServer.PilotName;
            tbGroupName.Text = selectedServer.GroupName;
            cbMSFSServer.Text = selectedServer.MsfsServer;
            tbNotes.Text = selectedServer.Notes;

            // ensure runtime settings map to the selected server's config values
            SaveSelectedServer(servers, selectedServerIndex);

            autoConnectToolStripMenuItem.Checked = Properties.Settings.Default["AutoConnect"].ToString().ToLower() == "true";

            // Start the Transmitter
            if (autoConnectToolStripMenuItem.Checked)
            {
                // start the connection timer (which will attempt re-connections)
                tmrConnect.Start();
                tsslMain.Text = "Connecting to Simulator...";
                this.Refresh();

                // cause an immediate connect
                Connect();

            }
            else
            {
                tsslMain.Text = "Version " + System.Windows.Forms.Application.ProductVersion;
            }

            this.Refresh();


        }

        /// <summary>
        /// Updates settings when Server Name textbox is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbServerName_TextChanged(object sender, EventArgs e)
        { }

        /// <summary>
        /// Updates settings when Server URL textbox is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbServerURL_TextChanged(object sender, EventArgs e)
        { }

        private void tbPin_TextChanged(object sender, EventArgs e)
        { }

        /// <summary>
        /// Updates settings when callsign textbox is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbCallsign_TextChanged(object sender, EventArgs e)
        {
            SaveFieldToSettings("callsign", "callsign", tbCallsign.Text);
        }

        /// <summary>
        /// Updates settings when pilot name textbox is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbPilotName_TextChanged(object sender, EventArgs e)
        {
            SaveFieldToSettings("pilotName", "pilotName", tbPilotName.Text);
        }

        /// <summary>
        /// Updates settings when group name textbox content is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbGroupName_TextChanged(object sender, EventArgs e)
        {
            SaveFieldToSettings("groupName", "groupName", tbGroupName.Text);
        }

        /// <summary>
        /// Updates settings when server is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbMSFSServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveFieldToSettings("msfsServer", "msfsServer", cbMSFSServer.Text);
        }

        /// <summary>
        /// Updates settings when notes textbox is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbNotes_TextChanged(object sender, EventArgs e)
        {
            SaveFieldToSettings("notes", "notes", tbNotes.Text);
        }

        /// <summary>
        /// Remove spaces when focus leaves callsign field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbCallsign_Leave(object sender, EventArgs e)
        {
            tbCallsign.Text = tbCallsign.Text.Trim();
        }


        /// <summary>
        /// Remove spaces when focus leaves pilot name field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbPilotName_Leave(object sender, EventArgs e)
        {
            tbPilotName.Text = tbPilotName.Text.Trim();
        }


        /// <summary>
        /// Remove spaces when focus leaves group name field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbGroupName_Leave(object sender, EventArgs e)
        {
            tbGroupName.Text = tbGroupName.Text.Trim();
        }


        /// <summary>
        /// Remove spaces when focus leaves notes field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbNotes_Leave(object sender, EventArgs e)
        {
            tbNotes.Text = tbNotes.Text.Trim();
        }


        /// <summary>
        /// Handle users clicking on the About menu option (show a message)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string version = System.Windows.Forms.Application.ProductVersion;
            MessageBox.Show("Transmitter\nby Jonathan Beckett\nVirtual Flight Online\nhttps://virtualflight.online\nVersion " + version, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        /// <summary>
        /// Helper method to connect to SimConnect and update the interface appropriately
        /// </summary>
        private void Connect()
        {
            // first check if the default parameters have been changed
            if (this.tbCallsign.Text == "Callsign" || this.tbPilotName.Text == "Pilot Name")
            {
                tmrConnect.Stop();
                btnDisconnect.Enabled = false;
                tsslMain.Text = "";
                MessageBox.Show("It looks like you haven't changed your callsign, or name yet. Please change them before connecting.", "Let's do this first", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                // check if the parameters are empty
                if (tbCallsign.Text != string.Empty && tbPilotName.Text != string.Empty && tbGroupName.Text != string.Empty)
                {
                    // if the user is not connected
                    if (!_simConnectClient.Connected)
                    {
                        // try to connect
                        try
                        {
                            tmrConnect.Start();  // checks the connection to the sim is up every few seconds

                            btnConnect.Enabled = false;
                            btnDisconnect.Enabled = true;

                            tsslMain.Text = "Connecting...";
                            this.Refresh(); // redraw the form to show the connecting message

                            // Connect to simulator using the form's window handle for message dispatch
                            _simConnectClient.Connect(this.Handle);

                            // Disable the textboxes
                            tbCallsign.Enabled = false;
                            tbPilotName.Enabled = false;
                            tbGroupName.Enabled = false;
                            cbMSFSServer.Enabled = false;

                            // Initialise the connection start time
                            this.ConnectionStartTime = DateTime.Now;

                            tmrTransmit.Start(); // transmits data every few seconds

                            autoConnectToolStripMenuItem.Enabled = false;
                            resetSettingsToDefaultsToolStripMenuItem.Enabled = false;

                            this.Refresh();
                        }
                        catch
                        {
                            // problem connecting
                            tsslMain.Text = "Problem connecting to Simulator" + ((Properties.Settings.Default["AutoConnect"].ToString().ToLower() == "true") ? " - retrying every 5 seconds" : "");
                            this.Refresh();
                        }

                    }

                }
                else
                {
                    // user has not filled in all required parameters
                    Disconnect("Please fill required parameters");
                    MessageBox.Show("You must fill out the Callsign, Pilot Name, Aircraft Type, Group Name, and Server URL", "Please Fill Data Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    tmrConnect.Stop();
                    tmrTransmit.Stop();

                    btnConnect.Enabled = true;
                    btnDisconnect.Enabled = false;
                }

            }
        }


        /// <summary>
        /// Helper method to disconnect from SimConnect and update the interface appropriately
        /// </summary>
        private void Disconnect(string msg)
        {
            // stop the timers
            tmrTransmit.Stop();
            tmrConnect.Stop();

            if (msg.Length > 0)
            {
                tsslMain.Text = msg;
            }
            else
            {
                tsslMain.Text = "Not Connected";
                tsslCommunicationsStatus.Text = "...";
                tsslSimulatorStatus.Text = "...";
            }

            this.Refresh();

            // if we are connected, disconnect from the simulator
            if (_simConnectClient.Connected)
            {
                _simConnectClient.Disconnect();
            }

            // configure the connect / disconnect buttons
            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;

            // switch the UI components back on
            tbCallsign.Enabled = true;
            tbPilotName.Enabled = true;
            tbGroupName.Enabled = true;
            cbMSFSServer.Enabled = true;

            autoConnectToolStripMenuItem.Enabled = true;
            resetSettingsToDefaultsToolStripMenuItem.Enabled = true;

        }

        /// <summary>
        /// Exits the application (shuts things down first)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // disconnect if connected
            if (_simConnectClient.Connected) _simConnectClient.Disconnect();

            this.tmrConnect.Stop();
            this.tmrTransmit.Stop();

            // close the application
            this.Close();
        }


        /// <summary>
        /// Resets all server settings to the built-in defaults.
        /// Only permitted while disconnected from the simulator.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resetSettingsToDefaultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_simConnectClient.Connected)
            {
                Properties.Settings.Default["servers"] = DefaultServersJson;
                Properties.Settings.Default["selectedServer"] = "0";
                Properties.Settings.Default.Save();

                JsonArray servers = GetServersFromSettings();
                SaveSelectedServer(servers, 0);

                Server server = servers[0].Deserialize<Server>();
                tbCallsign.Text = server.Callsign;
                tbPilotName.Text = server.PilotName;
                tbGroupName.Text = server.GroupName;
                cbMSFSServer.Text = server.MsfsServer;
                tbNotes.Text = server.Notes;

                // save the settings
                Properties.Settings.Default.Save();

            }
            else
            {
                MessageBox.Show("Please disconnect from the simulator first", "Disconnect First", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }




        /// <summary>
        /// Toggles the aircraft data panel open or closed by resizing the form
        /// between its maximum and minimum heights.
        /// </summary>
        private void aircraftDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (aircraftDataToolStripMenuItem.Checked)
            {
                this.Height = this.MaximumSize.Height;
            }
            else
            {
                this.Height = this.MinimumSize.Height;
            }

        }

        /// <summary>
        /// Fires every 5 seconds (set in the designer) when auto-connect is active.
        /// Attempts to reconnect if the simulator connection has been lost.
        /// </summary>
        private void tmrConnect_Tick(object sender, EventArgs e)
        {
            if (!_simConnectClient.Connected)
            {
                Connect();
            }
        }

        /// <summary>
        /// Saves the auto-connect preference and starts or stops the reconnect timer accordingly.
        /// </summary>
        private void autoConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.autoConnectToolStripMenuItem.Checked)
            {
                Properties.Settings.Default["AutoConnect"] = "true";
                tmrConnect.Start();
            }
            else
            {
                Properties.Settings.Default["AutoConnect"] = "false";
                tmrConnect.Stop();
            }
            Properties.Settings.Default.Save();

        }

        /// <summary>
        /// Handles clicks on the Disconnect button. Stops all timers and closes the SimConnect session.
        /// </summary>
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            tmrConnect.Stop();
            Disconnect("");
        }

        // The server list management UI (add/remove/select) is reserved for a future release.
        private void btnAddServer_Click(object sender, EventArgs e) { }

        private void lbServers_SelectedIndexChanged(object sender, EventArgs e) { }

        private void btnRemoveServer_Click(object sender, EventArgs e) { }

        // --- Links menu: each handler opens the relevant URL in the default browser. ---

        private void websiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenUrl("https://virtualflight.online");
        }

        private void newsletterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenUrl("https://virtualflightonline.substack.com/");
        }

        private void airlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenUrl("https://airline.virtualflight.online");
        }

        private void forumsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenUrl("https://forums.virtualflight.online");
        }

        private void discordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenUrl("https://bit.ly/virtualflightonlinediscordserver");
        }

        private void facebookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenUrl("https://www.facebook.com/groups/virtualflight.online");
        }

        private void patreonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenUrl("https://patreon.com/virtualflightonline");
        }

        private void tsslMain_Click(object sender, EventArgs e) { }

        private void whosOnlineToolStripMenuItem_Click_2(object sender, EventArgs e)
        {
            OpenUrl("https://transmitter.virtualflight.online/status");
        }

        private void radarToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenUrl("https://transmitter.virtualflight.online/radar");
        }

        /// <summary>
        /// Opens a URL in the system default browser.
        /// UseShellExecute must be true for Process.Start to open URLs on .NET Framework.
        /// </summary>
        private static void OpenUrl(string url)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
