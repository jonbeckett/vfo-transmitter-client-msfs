using System;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;

namespace VirtualFlightOnlineTransmitter
{
    /// <summary>
    /// Wraps the native Microsoft SimConnect API for MSFS 2020/2024.
    /// <para>
    /// SimConnect communicates asynchronously via Win32 window messages.
    /// The owning WinForms window must override <c>WndProc</c> and call
    /// <see cref="HandleWindowMessage"/> whenever a message with id
    /// <see cref="WM_USER_SIMCONNECT"/> arrives. This causes SimConnect to fire its
    /// callbacks on the UI thread, keeping all data handling thread-safe.
    /// </para>
    /// <para>
    /// Typical usage: call <see cref="Connect"/> once, then call <see cref="RequestData"/>
    /// on a timer. Each call to <see cref="RequestData"/> results in one <see cref="DataReceived"/> event.
    /// </para>
    /// </summary>
    public class SimConnectClient
    {
        // SimConnect sends data back via a Windows message posted to the application window.
#pragma warning disable CA1707, SA1310 // Win32 constant naming convention requires underscores
        public const int WM_USER_SIMCONNECT = 0x0402;
#pragma warning restore CA1707, SA1310

        private enum Definitions
        {
            PlaneInfo,
        }

        private enum Requests
        {
            PlaneInfo,
        }

        /// <summary>
        /// The struct that SimConnect marshals incoming data into.
        /// <para>
        /// IMPORTANT: Field order must exactly match the order of <c>AddToDataDefinition</c>
        /// calls in <see cref="Connect"/>. SimConnect fills the struct by position, not by name.
        /// </para>
        /// <para>
        /// All numeric variables are registered as <c>SIMCONNECT_DATATYPE.FLOAT64</c> and
        /// stored as <c>double</c> here. Using the same type for every numeric field avoids
        /// the size-mismatch bug that arises when mixing <c>double</c> (8 bytes) and
        /// <c>uint</c> (4 bytes) with FLOAT64 wire data.
        /// </para>
        /// <c>Pack=1</c> prevents the CLR from inserting padding between fields.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct PlaneInfoResponse
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string Title;
            public double PlaneLatitude;
            public double PlaneLongitude;
            public double IndicatedAltitude;
            public double GpsPositionAlt;
            public double PlaneAltitude;
            public double PlaneHeadingDegreesTrue;
            public double PlaneHeadingDegreesMagnetic;
            public double AirspeedIndicated;
            public double GpsGroundSpeed;
            public double PlaneTouchdownNormalVelocity;
            public double TransponderCode;
        }

        private SimConnect _simConnect;

        /// <summary>
        /// True while a SimConnect session is open.
        /// </summary>
        public bool Connected => _simConnect != null;

        /// <summary>
        /// Raised on the calling thread each time a new data snapshot arrives from the simulator.
        /// </summary>
        public event EventHandler<PlaneData> DataReceived;

        /// <summary>
        /// Opens a SimConnect session. The window handle is required so that
        /// SimConnect can post WM_USER_SIMCONNECT messages back to the application.
        /// </summary>
        /// <param name="windowHandle">Handle of the owning WinForms window (Form.Handle).</param>
        public void Connect(IntPtr windowHandle)
        {
            _simConnect = new SimConnect("VirtualFlightOnlineClient", windowHandle, WM_USER_SIMCONNECT, null, 0);

            // Register each SimVar in the same order as the struct fields above.
            // Using SIMCONNECT_UNUSED (0xFFFFFFFF) for the datum ID means SimConnect assigns it automatically.
            _simConnect.AddToDataDefinition(Definitions.PlaneInfo, "TITLE",                              null,             SIMCONNECT_DATATYPE.STRING256, 0, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(Definitions.PlaneInfo, "PLANE LATITUDE",                    "degrees",        SIMCONNECT_DATATYPE.FLOAT64,   0, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(Definitions.PlaneInfo, "PLANE LONGITUDE",                   "degrees",        SIMCONNECT_DATATYPE.FLOAT64,   0, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(Definitions.PlaneInfo, "INDICATED ALTITUDE",                "feet",           SIMCONNECT_DATATYPE.FLOAT64,   0, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(Definitions.PlaneInfo, "GPS POSITION ALT",                  "meters",         SIMCONNECT_DATATYPE.FLOAT64,   0, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(Definitions.PlaneInfo, "PLANE ALTITUDE",                    "feet",           SIMCONNECT_DATATYPE.FLOAT64,   0, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(Definitions.PlaneInfo, "PLANE HEADING DEGREES TRUE",        "degrees",        SIMCONNECT_DATATYPE.FLOAT64,   0, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(Definitions.PlaneInfo, "PLANE HEADING DEGREES MAGNETIC",    "degrees",        SIMCONNECT_DATATYPE.FLOAT64,   0, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(Definitions.PlaneInfo, "AIRSPEED INDICATED",                "knots",          SIMCONNECT_DATATYPE.FLOAT64,   0, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(Definitions.PlaneInfo, "GPS GROUND SPEED",                  "meters/second",  SIMCONNECT_DATATYPE.FLOAT64,   0, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(Definitions.PlaneInfo, "PLANE TOUCHDOWN NORMAL VELOCITY",   "feet/second",    SIMCONNECT_DATATYPE.FLOAT64,   0, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(Definitions.PlaneInfo, "TRANSPONDER CODE:1",                "bco16",          SIMCONNECT_DATATYPE.FLOAT64,   0, SimConnect.SIMCONNECT_UNUSED);

            // Tell SimConnect which managed struct maps to this definition.
            _simConnect.RegisterDataDefineStruct<PlaneInfoResponse>(Definitions.PlaneInfo);

            _simConnect.OnRecvSimobjectDataBytype += OnRecvSimobjectDataBytype;
        }

        /// <summary>
        /// Asks SimConnect for a single data snapshot of the user's aircraft.
        /// The response arrives asynchronously via WM_USER_SIMCONNECT / HandleWindowMessage.
        /// </summary>
        public void RequestData()
        {
            _simConnect?.RequestDataOnSimObjectType(Requests.PlaneInfo, Definitions.PlaneInfo, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
        }

        /// <summary>
        /// Closes the SimConnect session and releases all resources.
        /// </summary>
        public void Disconnect()
        {
            if (_simConnect != null)
            {
                _simConnect.OnRecvSimobjectDataBytype -= OnRecvSimobjectDataBytype;
                _simConnect.Dispose();
                _simConnect = null;
            }
        }

        /// <summary>
        /// Must be called from the owning form's WndProc whenever a message with id
        /// WM_USER_SIMCONNECT arrives. This causes SimConnect to dispatch any pending
        /// callbacks (including OnRecvSimobjectDataBytype).
        /// </summary>
        public void HandleWindowMessage()
        {
            _simConnect?.ReceiveMessage();
        }

        /// <summary>
        /// Converts a raw BCO16 (Binary Coded Octal, 16-bit) value as returned by SimConnect
        /// into a human-readable four-digit transponder squawk code.
        /// Each nibble (4 bits) holds one octal digit (0-7), so 0x1200 = squawk 1200.
        /// </summary>
        /// <param name="bco16Value">The raw BCO16 value from SimConnect, received as a double.</param>
        /// <returns>The decoded squawk code as an unsigned integer (e.g. 1200 for VFR).</returns>
        private static uint DecodeBco16(double bco16Value)
        {
            uint raw = (uint)bco16Value;
            uint d1 = (raw >> 12) & 0xF;
            uint d2 = (raw >> 8) & 0xF;
            uint d3 = (raw >> 4) & 0xF;
            uint d4 = raw & 0xF;
            return (d1 * 1000) + (d2 * 100) + (d3 * 10) + d4;
        }

        /// <summary>
        /// Called by SimConnect (via <see cref="HandleWindowMessage"/>) when the simulator
        /// has fulfilled a request made by <see cref="RequestData"/>.
        /// Converts the raw <see cref="PlaneInfoResponse"/> struct into a <see cref="PlaneData"/>
        /// object and raises the <see cref="DataReceived"/> event.
        /// </summary>
        private void OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (data.dwRequestID != (uint)Requests.PlaneInfo)
            {
                return;
            }

            PlaneInfoResponse r = (PlaneInfoResponse)data.dwData[0];

            DataReceived?.Invoke(this, new PlaneData
            {
                AircraftType = r.Title,
                Latitude = r.PlaneLatitude,
                Longitude = r.PlaneLongitude,
                Altitude = r.IndicatedAltitude,
                Heading = r.PlaneHeadingDegreesTrue,
                Airspeed = r.AirspeedIndicated,
                Groundspeed = r.GpsGroundSpeed * 1.94384449,  // m/s → knots
                TouchdownVelocity = r.PlaneTouchdownNormalVelocity,
                TransponderCode = DecodeBco16(r.TransponderCode).ToString(),
            });
        }
    }
}
