using System;
using System.Windows.Forms;

namespace VirtualFlightOnlineTransmitter
{
    /// <summary>
    /// Application entry point. Bootstraps the WinForms message loop and opens the main window.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // Standard WinForms bootstrap — must be called before any UI is created.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
