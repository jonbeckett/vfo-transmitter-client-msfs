using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace VirtualFlightOnlineTransmitter
{
    /// <summary>
    /// Sends aircraft position snapshots to the VirtualFlight.Online server over HTTP GET.
    ///
    /// <para>
    /// A single static <see cref="HttpClient"/> is shared for the lifetime of the application.
    /// This is the Microsoft-recommended pattern — creating a new HttpClient per request causes
    /// socket exhaustion under load.
    /// </para>
    ///
    /// <para>
    /// All transmissions are non-blocking. <see cref="TransmitAsync"/> returns a status string
    /// (round-trip time or error message) that the UI can display directly.
    /// </para>
    /// </summary>
    public class HttpTransmitter
    {
        // Shared across all calls for the lifetime of the process.
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5),
        };

        // Numbers in the query string must use a decimal point, not a locale-specific separator.
        private static readonly CultureInfo _usFormat = new CultureInfo("en-US");

        /// <summary>
        /// Transmits a <see cref="PlaneData"/> snapshot to the server configured in application settings.
        /// </summary>
        /// <param name="data">The aircraft state snapshot to transmit.</param>
        /// <param name="notes">Free-text notes entered by the user.</param>
        /// <param name="version">The application version string, included so the server can log client versions.</param>
        /// <returns>
        /// A short status string ready to display in the UI:
        /// the round-trip time in milliseconds on success (e.g. "142.37 ms"),
        /// or an error description on failure.
        /// </returns>
        public async Task<string> TransmitAsync(PlaneData data, string notes, string version)
        {
            try
            {
                string url = BuildUrl(data, notes, version);

                DateTime requestTime = DateTime.Now;
                HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(true);
                TimeSpan timeTaken = DateTime.Now - requestTime;

                return response.IsSuccessStatusCode
                    ? timeTaken.TotalMilliseconds.ToString("0.00") + " ms"
                    : "Error: " + response.StatusCode;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Builds the full query-string URL for a single transmission.
        /// All string values are URL-encoded to prevent delimiter injection.
        /// Numeric values are formatted with a US decimal point regardless of system locale.
        /// </summary>
        private string BuildUrl(PlaneData data, string notes, string version)
        {
            // Read settings defensively to avoid NullReferenceExceptions when a key is missing
            string serverUrl = Properties.Settings.Default["ServerURL"]?.ToString() ?? string.Empty;
            string callsign = Properties.Settings.Default["Callsign"]?.ToString() ?? string.Empty;
            string pilotName = Properties.Settings.Default["PilotName"]?.ToString() ?? string.Empty;
            string groupName = Properties.Settings.Default["GroupName"]?.ToString() ?? string.Empty;
            string msfsServer = Properties.Settings.Default["MSFSServer"]?.ToString() ?? string.Empty;
            string pin = Properties.Settings.Default["Pin"]?.ToString() ?? string.Empty;

            return serverUrl
                + "?Callsign="          + WebUtility.UrlEncode(callsign)
                + "&PilotName="         + WebUtility.UrlEncode(pilotName)
                + "&GroupName="         + WebUtility.UrlEncode(groupName)
                + "&MSFSServer="        + WebUtility.UrlEncode(msfsServer)
                + "&Pin="               + WebUtility.UrlEncode(pin)
                + "&AircraftType="      + WebUtility.UrlEncode(data?.AircraftType ?? string.Empty)
                + "&Latitude="          + (data?.Latitude.ToString(_usFormat) ?? "0")
                + "&Longitude="         + (data?.Longitude.ToString(_usFormat) ?? "0")
                + "&Altitude="          + (data?.Altitude.ToString(_usFormat) ?? "0")
                + "&Airspeed="          + (data?.Airspeed.ToString(_usFormat) ?? "0")
                + "&Groundspeed="       + (data?.Groundspeed.ToString(_usFormat) ?? "0")
                + "&Heading="           + (data?.Heading.ToString(_usFormat) ?? "0")
                + "&TouchdownVelocity=" + (data?.TouchdownVelocity.ToString(_usFormat) ?? "0")
                + "&TransponderCode="   + WebUtility.UrlEncode(data?.TransponderCode ?? string.Empty)
                + "&Version="           + WebUtility.UrlEncode(version ?? string.Empty)
                + "&Notes="             + WebUtility.UrlEncode(notes ?? string.Empty);
        }
    }
}
