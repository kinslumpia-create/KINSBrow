using System;
using System.Windows.Forms;

namespace KinsBrowser
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            // Terapkan mode emulasi IE11 (per-user, tanpa admin) sebelum control WebBrowser dibuat.
            // Ini hanya memengaruhi tab "Mode IE" (mesin lawas); tab modern pakai WebView2 dan
            // tidak terpengaruh oleh pengaturan ini sama sekali.
            IeEmulation.EnsureIe11EmulationForThisExe();

            ApplicationConfiguration.Initialize();
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MainForm());
        }
    }
}
