using System.Text.Json.Serialization;

namespace VirtualFlightOnlineTransmitter
{
    /// <summary>
    /// Represents a single server configuration entry as stored in application settings.
    /// Instances are serialised to / deserialised from a JSON array so that multiple
    /// server profiles can be saved and switched between at runtime.
    /// </summary>
    /// <remarks>
    /// JSON key names use camelCase to match the existing settings format.
    /// C# property names use PascalCase per .NET conventions.
    /// The <see cref="JsonPropertyName"/> attributes bridge the two.
    /// </remarks>
    internal class Server
    {
        /// <summary>Gets or sets the human-readable name for this server profile (e.g. "VirtualFlight.Online").</summary>
        [JsonPropertyName("serverName")]
        public string ServerName { get; set; }

        /// <summary>Gets or sets the pilot's callsign (e.g. "G-ABCD").</summary>
        [JsonPropertyName("callsign")]
        public string Callsign { get; set; }

        /// <summary>Gets or sets the pilot's display name.</summary>
        [JsonPropertyName("pilotName")]
        public string PilotName { get; set; }

        /// <summary>Gets or sets the group or airline the pilot belongs to.</summary>
        [JsonPropertyName("groupName")]
        public string GroupName { get; set; }

        /// <summary>Gets or sets any free-text notes transmitted alongside position data.</summary>
        [JsonPropertyName("notes")]
        public string Notes { get; set; }

        /// <summary>Gets or sets the MSFS multiplayer server region the pilot is connected to (e.g. "WEST EUROPE").</summary>
        [JsonPropertyName("msfsServer")]
        public string MsfsServer { get; set; }

        /// <summary>Gets or sets whether the transmitter should connect automatically on startup ("true" / "false").</summary>
        [JsonPropertyName("autoConnect")]
        public string AutoConnect { get; set; }

        /// <summary>Gets or sets the full URL of the transmit endpoint (e.g. "https://transmitter.virtualflight.online/transmit").</summary>
        [JsonPropertyName("serverURL")]
        public string ServerUrl { get; set; }

        /// <summary>Gets or sets the optional PIN used to authenticate with the server.</summary>
        [JsonPropertyName("pin")]
        public string Pin { get; set; }
    }
}
