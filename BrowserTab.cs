using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace KinsBrowser
{
    /// <summary>
    /// Mesin render yang dipakai satu tab.
    /// </summary>
    internal enum EngineKind
    {
        /// <summary>Mesin modern (Chromium/Edge) via WebView2. Dipakai untuk browsing biasa.</summary>
        Modern,

        /// <summary>Mesin lawas Internet Explorer (Trident/MSHTML) via kontrol WebBrowser bawaan
        /// .NET. Satu-satunya mesin yang bisa menjalankan ActiveX seperti OWC (Office Web
        /// Components). Dipakai khusus untuk aplikasi/situs lama yang masih butuh IE.</summary>
        Legacy
    }

    /// <summary>
    /// Membungkus satu "tab" browser, entah pakai mesin modern (WebView2) atau mesin lawas
    /// (WebBrowser/IE), di balik satu API yang sama supaya MainForm tidak perlu tahu bedanya.
    /// </summary>
    internal sealed class BrowserTab
    {
        public EngineKind Engine { get; }
        public Control HostControl { get; }

        private readonly WebView2? _webView;
        private readonly WebBrowser? _webBrowser;
        private string? _pendingUrl;

        public event Action<string>? TitleChanged;
        public event Action<string>? NavigationCompleted;
        public event Action? NavigationStarted;

        public BrowserTab(EngineKind engine)
        {
            Engine = engine;

            if (engine == EngineKind.Modern)
            {
                var webView = new WebView2 { Dock = DockStyle.Fill };
                _webView = webView;
                HostControl = webView;

                webView.CoreWebView2InitializationCompleted += (_, e) =>
                {
                    if (!e.IsSuccess || webView.CoreWebView2 == null) return;

                    var core = webView.CoreWebView2;
                    core.DocumentTitleChanged += (_, _) => TitleChanged?.Invoke(core.DocumentTitle);
                    core.NavigationStarting += (_, _) => NavigationStarted?.Invoke();
                    core.NavigationCompleted += (_, _) => NavigationCompleted?.Invoke(core.Source);

                    // Kalau Navigate() dipanggil sebelum WebView2 selesai inisialisasi,
                    // URL-nya ditampung dulu di _pendingUrl lalu dijalankan di sini.
                    if (_pendingUrl != null)
                    {
                        core.Navigate(_pendingUrl);
                        _pendingUrl = null;
                    }
                };

                _ = webView.EnsureCoreWebView2Async();
            }
            else
            {
                var wb = new WebBrowser { Dock = DockStyle.Fill, ScriptErrorsSuppressed = true };
                _webBrowser = wb;
                HostControl = wb;

                wb.DocumentTitleChanged += (_, _) => TitleChanged?.Invoke(wb.DocumentTitle);
                wb.Navigating += (_, _) => NavigationStarted?.Invoke();
                wb.DocumentCompleted += (_, e) => NavigationCompleted?.Invoke(e.Url.ToString());
            }
        }

        public string? CurrentUrl => Engine == EngineKind.Modern
            ? _webView?.Source?.ToString()
            : _webBrowser?.Url?.ToString();

        public bool CanGoBack => Engine == EngineKind.Modern
            ? (_webView?.CanGoBack ?? false)
            : (_webBrowser?.CanGoBack ?? false);

        public bool CanGoForward => Engine == EngineKind.Modern
            ? (_webView?.CanGoForward ?? false)
            : (_webBrowser?.CanGoForward ?? false);

        public void GoBack()
        {
            if (Engine == EngineKind.Modern) _webView?.GoBack();
            else _webBrowser?.GoBack();
        }

        public void GoForward()
        {
            if (Engine == EngineKind.Modern) _webView?.GoForward();
            else _webBrowser?.GoForward();
        }

        public void Refresh()
        {
            if (Engine == EngineKind.Modern) _webView?.Reload();
            else _webBrowser?.Refresh();
        }

        public void Print()
        {
            if (Engine == EngineKind.Modern) _webView?.CoreWebView2?.ShowPrintUI();
            else _webBrowser?.Print();
        }

        public void Navigate(string url)
        {
            if (Engine == EngineKind.Modern)
            {
                if (_webView!.CoreWebView2 != null)
                    _webView.CoreWebView2.Navigate(url);
                else
                    _pendingUrl = url; // ditunda sampai CoreWebView2InitializationCompleted
            }
            else
            {
                _webBrowser!.Navigate(url);
            }
        }

        public void Zoom(int deltaPercent, bool reset = false)
        {
            if (Engine == EngineKind.Modern)
            {
                if (_webView == null) return;
                double current = reset ? 1.0 : _webView.ZoomFactor;
                double next = reset ? 1.0 : Math.Clamp(current + deltaPercent / 100.0, 0.3, 3.0);
                _webView.ZoomFactor = next;
                return;
            }

            var wb = _webBrowser;
            if (wb?.Document?.Body == null) return;

            if (reset)
            {
                wb.Document.Body.Style = RemoveZoomStyle(wb.Document.Body.Style ?? "");
                return;
            }

            int currentPct = 100;
            var style = wb.Document.Body.Style ?? "";
            var idx = style.IndexOf("zoom:", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                var part = style[(idx + 5)..];
                var digits = new string(part.TakeWhile(char.IsDigit).ToArray());
                if (int.TryParse(digits, out var parsed)) currentPct = parsed;
            }
            int nextPct = Math.Clamp(currentPct + deltaPercent, 30, 300);
            wb.Document.Body.Style = RemoveZoomStyle(style) + $";zoom:{nextPct}%";
        }

        private static string RemoveZoomStyle(string style)
        {
            var parts = style.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var kept = new System.Collections.Generic.List<string>();
            foreach (var p in parts)
            {
                if (!p.TrimStart().StartsWith("zoom:", StringComparison.OrdinalIgnoreCase))
                    kept.Add(p);
            }
            return string.Join(";", kept);
        }

        public string? GetOuterHtml()
        {
            if (Engine == EngineKind.Legacy)
                return _webBrowser?.Document?.Body?.Parent?.OuterHtml;
            return null; // Untuk mesin modern, "lihat source" pakai ExecuteScriptAsync (async), ditangani di MainForm.
        }

        public CoreWebView2? Core => _webView?.CoreWebView2;
    }
}
