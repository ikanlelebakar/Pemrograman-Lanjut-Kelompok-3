using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ProjekPL_Gabungan
{
    internal static class Program
    {
        // DPI awareness dari projek Habib, dipertahankan agar form kasir tampil seperti sumbernya.
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormKasir());
        }
    }
}
