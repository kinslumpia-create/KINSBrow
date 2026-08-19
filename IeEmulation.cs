using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace KinsBrowser
{
    /// <summary>
    /// Control WinForms "WebBrowser" secara default meniru Internet Explorer versi 7 (mode kompatibilitas lama)
    /// kecuali kita daftarkan exe ini di registry supaya memakai mesin IE11 (mode paling modern yang tersedia
    /// di engine Trident/MSHTML bawaan Windows). Ini murni per-user (HKEY_CURRENT_USER) sehingga TIDAK
    /// memerlukan hak akses Administrator.
    /// </summary>
    internal static class IeEmulation
    {
        private const string FeatureControlKey =
            @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";

        private const string GpuRenderingKey =
            @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_GPU_RENDERING";

        private const string AjaxKey =
            @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_AJAX_CONNECTIONEVENT";

        // 11001 = mode "IE11 Edge" (mode paling baru yang didukung WebBrowser control)
        private const int Ie11EdgeMode = 11001;

        public static void EnsureIe11EmulationForThisExe()
        {
            try
            {
                string exeName = Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName ?? "KinsBrowser.exe");

                SetHkcuDword(FeatureControlKey, exeName, Ie11EdgeMode);
                SetHkcuDword(GpuRenderingKey, exeName, 1);
                SetHkcuDword(AjaxKey, exeName, 1);
            }
            catch
            {
                // Kalau gagal set registry (mis. environment yang sangat dibatasi),
                // browser tetap jalan, hanya saja akan pakai mode emulasi IE7 default.
            }
        }

        private static void SetHkcuDword(string subKeyPath, string valueName, int value)
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(subKeyPath, writable: true);
            key.SetValue(valueName, value, RegistryValueKind.DWord);
        }
    }
}
