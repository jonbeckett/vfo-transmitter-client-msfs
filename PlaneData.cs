namespace VirtualFlightOnlineTransmitter
{
    /// <summary>
    /// Carries a single snapshot of aircraft state as received from SimConnect.
    /// All values have already been converted to the units shown in each property's
    /// documentation — no further conversion is required by the caller.
    /// </summary>
    public class PlaneData
    {
        /// <summary>Gets or sets the full aircraft title as reported by the simulator (e.g. "Cessna 172 Skyhawk Asobo").</summary>
        public string AircraftType { get; set; }

        /// <summary>Gets or sets the aircraft latitude in decimal degrees. Positive = North, negative = South.</summary>
        public double Latitude { get; set; }

        /// <summary>Gets or sets the aircraft longitude in decimal degrees. Positive = East, negative = West.</summary>
        public double Longitude { get; set; }

        /// <summary>Gets or sets the indicated altitude in feet.</summary>
        public double Altitude { get; set; }

        /// <summary>Gets or sets the true heading in degrees (0–360).</summary>
        public double Heading { get; set; }

        /// <summary>Gets or sets the indicated airspeed in knots.</summary>
        public double Airspeed { get; set; }

        /// <summary>Gets or sets the GPS ground speed in knots (converted from m/s by <see cref="SimConnectClient"/>).</summary>
        public double Groundspeed { get; set; }

        /// <summary>Gets or sets the normal touchdown velocity in feet per second at the moment of the last landing.</summary>
        public double TouchdownVelocity { get; set; }

        /// <summary>Gets or sets the four-digit squawk code as a string (e.g. "1200").</summary>
        public string TransponderCode { get; set; }
    }
}
