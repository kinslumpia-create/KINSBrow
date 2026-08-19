using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace KinsBrowser
{
    public class MainForm : Form
    {
        // ---------- Tema warna (modern, enak dilihat) ----------
        private static readonly Color ColorBg = Color.FromArgb(246, 247, 250);
        private static readonly Color ColorToolbar = Color.White;
        private static readonly Color ColorAccent = Color.FromArgb(0, 103, 199);   // biru
        private static readonly Color ColorAccentDark = Color.FromArgb(0, 78, 152);
        private static readonly Color ColorBorder = Color.FromArgb(224, 226, 231);
        private static readonly Color ColorTextMuted = Color.FromArgb(110, 116, 128);
        private static readonly Color ColorLegacyBadge = Color.FromArgb(180, 95, 6);

        private const string HomePage = "https://www.bing.com";

        // ---------- Kontrol utama ----------
        private readonly TabControl _tabs = new();
        private readonly Panel _toolbar = new();
        private readonly Panel _addressWrap = new();
        private readonly TextBox _address = new();
        private readonly Button _btnBack = new();
        private readonly Button _btnForward = new();
        private readonly Button _btnRefresh = new();
        private readonly Button _btnHome = new();
        private readonly Button _btnNewTab = new();
        private readonly Button _btnGo = new();
        private readonly Button _btnMenu = new();
        private readonly Button _btnIeMode = new();
        private readonly Panel _statusBar = new();
        private readonly Label _statusText = new();
        private readonly Label _statusZone = new();
        private readonly ProgressBar _progress = new();

        private readonly string _dataDir;
        private readonly string _favoritesPath;
        private readonly string _ieSitesPath;
        private readonly List<string> _favorites = new();
        private readonly List<string> _history = new();

        // Daftar awalan URL yang otomatis dibuka pakai mesin lawas (IE/Trident), mirip
        // "IE mode site list" di Microsoft Edge. Cocok untuk aplikasi internal yang pakai OWC.
        private readonly List<string> _ieSites = new();

        public MainForm()
        {
            _dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KinsBrowser");
            Directory.CreateDirectory(_dataDir); // AppData milik user -> tidak butuh admin
            _favoritesPath = Path.Combine(_dataDir, "favorites.json");
            _ieSitesPath = Path.Combine(_dataDir, "ie-sites.json");
            LoadFavorites();
            LoadIeSites();

            Text = "Kins Browser";
            Width = 1200;
            Height = 800;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ColorBg;
            Font = new Font("Segoe UI", 9f);
            Icon = SystemIcons.Application;

            BuildToolbar();
            BuildTabs();
            BuildStatusBar();

            Controls.Add(_tabs);
            Controls.Add(_statusBar);
            Controls.Add(_toolbar);

            NewTab(HomePage, EngineKind.Modern);
        }

        // =========================================================
        //  TOOLBAR
        // =========================================================
        private void BuildToolbar()
        {
            _toolbar.Dock = DockStyle.Top;
            _toolbar.Height = 54;
            _toolbar.BackColor = ColorToolbar;
            _toolbar.Padding = new Padding(10, 8, 10, 8);

            var bottomLine = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = ColorBorder };

            _btnBack.Text = "\u2190";
            _btnForward.Text = "\u2192";
            _btnRefresh.Text = "\u27F3";
            _btnHome.Text = "\u2302";
            _btnNewTab.Text = "+";
            _btnMenu.Text = "\u2261";
            _btnGo.Text = "Buka";
            _btnIeMode.Text = "IE";

            foreach (var b in new[] { _btnBack, _btnForward, _btnRefresh, _btnHome, _btnMenu })
            {
                StyleNavButton(b);
            }
            StyleNavButton(_btnNewTab);
            StyleNavButton(_btnIeMode);
            _btnIeMode.ForeColor = ColorLegacyBadge;
            _btnIeMode.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            _btnBack.Click += (_, _) => { if (CurrentTab?.CanGoBack == true) CurrentTab.GoBack(); };
            _btnForward.Click += (_, _) => { if (CurrentTab?.CanGoForward == true) CurrentTab.GoForward(); };
            _btnRefresh.Click += (_, _) => CurrentTab?.Refresh();
            _btnHome.Click += (_, _) => Navigate(HomePage);
            _btnNewTab.Click += (_, _) => NewTab(HomePage, EngineKind.Modern);
            _btnMenu.Click += (_, _) => ShowMainMenu();
            _btnIeMode.Click += (_, _) => OpenCurrentInIeMode();

            // Address bar dibungkus panel supaya terlihat rounded/modern
            _addressWrap.BackColor = Color.FromArgb(240, 242, 246);
            _addressWrap.Padding = new Padding(10, 6, 10, 6);

            _address.BorderStyle = BorderStyle.None;
            _address.Font = new Font("Segoe UI", 10f);
            _address.BackColor = _addressWrap.BackColor;
            _address.Dock = DockStyle.Fill;
            _address.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    Navigate(_address.Text);
                }
            };
            _addressWrap.Controls.Add(_address);

            _btnGo.FlatStyle = FlatStyle.Flat;
            _btnGo.FlatAppearance.BorderSize = 0;
            _btnGo.BackColor = ColorAccent;
            _btnGo.ForeColor = Color.White;
            _btnGo.Width = 70;
            _btnGo.Click += (_, _) => Navigate(_address.Text);

            var flow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
            };
            flow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
            flow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
            flow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
            flow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
            flow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            flow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 176));

            _btnBack.Dock = DockStyle.Fill;
            _btnForward.Dock = DockStyle.Fill;
            _btnRefresh.Dock = DockStyle.Fill;
            _btnHome.Dock = DockStyle.Fill;
            _addressWrap.Dock = DockStyle.Fill;
            _addressWrap.Margin = new Padding(6, 4, 6, 4);

            var rightGroup = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
            rightGroup.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            rightGroup.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 46));
            rightGroup.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            rightGroup.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            _btnIeMode.Dock = DockStyle.Fill;
            _btnGo.Dock = DockStyle.Fill;
            _btnNewTab.Dock = DockStyle.Fill;
            _btnMenu.Dock = DockStyle.Fill;
            rightGroup.Controls.Add(_btnIeMode, 0, 0);
            rightGroup.Controls.Add(_btnGo, 1, 0);
            rightGroup.Controls.Add(_btnNewTab, 2, 0);
            rightGroup.Controls.Add(_btnMenu, 3, 0);

            flow.Controls.Add(_btnBack, 0, 0);
            flow.Controls.Add(_btnForward, 1, 0);
            flow.Controls.Add(_btnRefresh, 2, 0);
            flow.Controls.Add(_btnHome, 3, 0);
            flow.Controls.Add(_addressWrap, 4, 0);
            flow.Controls.Add(rightGroup, 5, 0);

            _toolbar.Controls.Add(flow);
            _toolbar.Controls.Add(bottomLine);

            var tip = new ToolTip();
            tip.SetToolTip(_btnIeMode, "Buka halaman ini di Mode IE (untuk situs/OWC lama)");
        }

        private void StyleNavButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(233, 236, 241);
            b.BackColor = ColorToolbar;
            b.ForeColor = Color.FromArgb(50, 54, 61);
            b.Font = new Font("Segoe UI", 12f);
            b.Cursor = Cursors.Hand;
            b.Margin = new Padding(2);
        }

        // =========================================================
        //  TABS
        // =========================================================
        private void BuildTabs()
        {
            _tabs.Dock = DockStyle.Fill;
            _tabs.Appearance = TabAppearance.Normal;
            _tabs.ItemSize = new Size(190, 30);
            _tabs.SizeMode = TabSizeMode.Fixed;
            _tabs.SelectedIndexChanged += (_, _) => SyncToolbarWithActiveTab();
            _tabs.MouseUp += (_, e) =>
            {
                if (e.Button == MouseButtons.Middle)
                {
                    for (int i = 0; i < _tabs.TabPages.Count; i++)
                    {
                        if (_tabs.GetTabRect(i).Contains(e.Location))
                        {
                            CloseTab(_tabs.TabPages[i]);
                            break;
                        }
                    }
                }
            };

            var ctx = new ContextMenuStrip();
            ctx.Items.Add("Tutup Tab", null, (_, _) => { if (_tabs.SelectedTab != null) CloseTab(_tabs.SelectedTab); });
            ctx.Items.Add("Tab Baru (Modern)", null, (_, _) => NewTab(HomePage, EngineKind.Modern));
            ctx.Items.Add("Tab Baru (Mode IE)", null, (_, _) => NewTab(HomePage, EngineKind.Legacy));
            _tabs.ContextMenuStrip = ctx;
        }

        private BrowserTab? CurrentTab => _tabs.SelectedTab?.Tag as BrowserTab;

        private void NewTab(string url, EngineKind engine)
        {
            bool isLegacy = engine == EngineKind.Legacy;
            var page = new TabPage(isLegacy ? "[IE] Tab Baru" : "Tab Baru") { BackColor = Color.White };
            if (isLegacy) page.ForeColor = ColorLegacyBadge;

            var tab = new BrowserTab(engine);
            page.Tag = tab;
            page.Controls.Add(tab.HostControl);

            tab.TitleChanged += title =>
            {
                var prefix = isLegacy ? "[IE] " : "";
                var shown = string.IsNullOrWhiteSpace(title) ? "Tab Baru" : title;
                var full = prefix + shown;
                page.Text = full.Length > 24 ? full[..24] + "\u2026" : full;
            };
            tab.NavigationStarted += () =>
            {
                _progress.Visible = true;
                _progress.Style = ProgressBarStyle.Marquee;
            };
            tab.NavigationCompleted += finalUrl =>
            {
                _progress.Visible = false;
                if (_tabs.SelectedTab == page)
                {
                    _address.Text = finalUrl;
                    UpdateZoneLabel(finalUrl, isLegacy);
                    _statusText.Text = "Selesai";
                }
                AddHistory(finalUrl);
            };

            _tabs.TabPages.Add(page);
            _tabs.SelectedTab = page;

            tab.Navigate(url);
        }

        private void CloseTab(TabPage page)
        {
            if (_tabs.TabPages.Count == 1)
            {
                NewTab(HomePage, EngineKind.Modern);
            }
            _tabs.TabPages.Remove(page);
        }

        private void SyncToolbarWithActiveTab()
        {
            var tab = CurrentTab;
            if (tab?.CurrentUrl != null)
            {
                _address.Text = tab.CurrentUrl;
                UpdateZoneLabel(tab.CurrentUrl, tab.Engine == EngineKind.Legacy);
            }
        }

        // =========================================================
        //  STATUS BAR (gaya IE klasik: teks status + info zona)
        // =========================================================
        private void BuildStatusBar()
        {
            _statusBar.Dock = DockStyle.Bottom;
            _statusBar.Height = 26;
            _statusBar.BackColor = ColorToolbar;

            var topLine = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = ColorBorder };

            _statusText.Text = "Siap";
            _statusText.ForeColor = ColorTextMuted;
            _statusText.AutoSize = false;
            _statusText.Dock = DockStyle.Left;
            _statusText.Width = 500;
            _statusText.TextAlign = ContentAlignment.MiddleLeft;
            _statusText.Padding = new Padding(10, 0, 0, 0);

            _statusZone.Text = "Internet";
            _statusZone.ForeColor = ColorTextMuted;
            _statusZone.AutoSize = false;
            _statusZone.Dock = DockStyle.Right;
            _statusZone.Width = 180;
            _statusZone.TextAlign = ContentAlignment.MiddleRight;
            _statusZone.Padding = new Padding(0, 0, 10, 0);

            _progress.Dock = DockStyle.Right;
            _progress.Width = 120;
            _progress.Visible = false;
            _progress.Style = ProgressBarStyle.Marquee;

            _statusBar.Controls.Add(_statusText);
            _statusBar.Controls.Add(_progress);
            _statusBar.Controls.Add(_statusZone);
            _statusBar.Controls.Add(topLine);
        }

        private void UpdateZoneLabel(string url, bool isLegacy)
        {
            string baseZone = url.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
                ? "Komputer Lokal"
                : url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                    ? "Internet (Aman)"
                    : "Internet";
            _statusZone.Text = isLegacy ? baseZone + " \u00b7 Mode IE" : baseZone;
        }

        // =========================================================
        //  NAVIGASI
        // =========================================================
        private void Navigate(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;
            string url = input.Trim();

            bool looksLikeUrl = url.Contains('.') && !url.Contains(' ');
            if (!url.StartsWith("http://") && !url.StartsWith("https://") && !url.StartsWith("file://"))
            {
                url = looksLikeUrl ? "https://" + url : "https://www.bing.com/search?q=" + Uri.EscapeDataString(url);
            }

            // Kalau URL ini cocok dengan salah satu awalan di daftar "Situs Mode IE",
            // otomatis dibuka pakai mesin lawas -- persis seperti fitur "IE mode" di Edge.
            if (MatchesIeSiteList(url))
            {
                NewTab(url, EngineKind.Legacy);
                return;
            }

            var tab = EnsureTab();
            tab.Navigate(url);
        }

        private bool MatchesIeSiteList(string url)
        {
            return _ieSites.Any(prefix =>
                !string.IsNullOrWhiteSpace(prefix) &&
                url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        private BrowserTab EnsureTab()
        {
            if (CurrentTab is { } tab) return tab;
            NewTab(HomePage, EngineKind.Modern);
            return CurrentTab!;
        }

        private void OpenCurrentInIeMode()
        {
            var url = CurrentTab?.CurrentUrl ?? _address.Text;
            if (string.IsNullOrWhiteSpace(url)) url = HomePage;
            NewTab(url, EngineKind.Legacy);
        }

        // =========================================================
        //  MENU (favorit, riwayat, zoom, situs mode IE, dsb)
        // =========================================================
        private void ShowMainMenu()
        {
            var menu = new ContextMenuStrip { Font = new Font("Segoe UI", 9.5f) };

            menu.Items.Add("Tab Baru (Modern)", null, (_, _) => NewTab(HomePage, EngineKind.Modern));
            menu.Items.Add("Tab Baru (Mode IE)", null, (_, _) => NewTab(HomePage, EngineKind.Legacy));
            menu.Items.Add("Tutup Tab Ini", null, (_, _) => { if (_tabs.SelectedTab != null) CloseTab(_tabs.SelectedTab); });
            menu.Items.Add(new ToolStripSeparator());

            var zoomIn = new ToolStripMenuItem("Perbesar (+)");
            zoomIn.Click += (_, _) => CurrentTab?.Zoom(10);
            var zoomOut = new ToolStripMenuItem("Perkecil (-)");
            zoomOut.Click += (_, _) => CurrentTab?.Zoom(-10);
            var zoomReset = new ToolStripMenuItem("Reset Zoom");
            zoomReset.Click += (_, _) => CurrentTab?.Zoom(0, reset: true);
            menu.Items.Add(zoomIn);
            menu.Items.Add(zoomOut);
            menu.Items.Add(zoomReset);
            menu.Items.Add(new ToolStripSeparator());

            menu.Items.Add("Tambah ke Favorit", null, (_, _) => AddFavorite());
            var favMenu = new ToolStripMenuItem("Favorit");
            foreach (var f in _favorites)
                favMenu.DropDownItems.Add(f, null, (_, _) => Navigate(f));
            if (_favorites.Count == 0)
                favMenu.DropDownItems.Add("(kosong)").Enabled = false;
            menu.Items.Add(favMenu);

            var histMenu = new ToolStripMenuItem("Riwayat");
            foreach (var h in _history.AsEnumerable().Reverse().Take(15))
                histMenu.DropDownItems.Add(h, null, (_, _) => Navigate(h));
            if (_history.Count == 0)
                histMenu.DropDownItems.Add("(kosong)").Enabled = false;
            menu.Items.Add(histMenu);
            menu.Items.Add(new ToolStripSeparator());

            menu.Items.Add("Buka Halaman Ini di Mode IE", null, (_, _) => OpenCurrentInIeMode());
            menu.Items.Add("Selalu Buka Situs Ini di Mode IE\u2026", null, (_, _) => AddCurrentToIeSiteList());
            menu.Items.Add("Kelola Daftar Situs Mode IE\u2026", null, (_, _) => ManageIeSiteList());
            menu.Items.Add(new ToolStripSeparator());

            menu.Items.Add("Lihat Source Halaman", null, (_, _) => ViewSource());
            menu.Items.Add("Cetak Halaman", null, (_, _) => CurrentTab?.Print());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Tentang Kins Browser", null, (_, _) => ShowAbout());

            menu.Show(_btnMenu, new Point(0, _btnMenu.Height));
        }

        private void ViewSource()
        {
            var tab = CurrentTab;
            if (tab == null) return;

            if (tab.Engine == EngineKind.Legacy)
            {
                string html = tab.GetOuterHtml() ?? "(tidak ada konten)";
                ShowSourceWindow(html);
                return;
            }

            var core = tab.Core;
            if (core == null) return;
            core.ExecuteScriptAsync("document.documentElement.outerHTML").ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    string raw = t.Result;
                    string html = JsonSerializer.Deserialize<string>(raw) ?? raw;
                    if (IsHandleCreated) Invoke(new MethodInvoker(() => ShowSourceWindow(html)));
                }
            });
        }

        private void ShowSourceWindow(string html)
        {
            var f = new Form { Text = "Lihat Source", Width = 800, Height = 600, StartPosition = FormStartPosition.CenterParent };
            var box = new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 9f), Text = html, ReadOnly = true };
            f.Controls.Add(box);
            f.Show(this);
        }

        private void ShowAbout()
        {
            MessageBox.Show(this,
                "Kins Browser\n" +
                "Browser dua-mesin: modern (Chromium/Edge via WebView2) untuk web biasa,\n" +
                "dan Mode IE (Trident/MSHTML) untuk situs/aplikasi lama yang butuh ActiveX (OWC).\n" +
                "Berjalan tanpa instalasi & tanpa hak akses admin (portable).\n\nVersi 2.0",
                "Tentang Kins Browser", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // =========================================================
        //  FAVORIT & RIWAYAT (disimpan di %AppData%\KinsBrowser, tidak butuh admin)
        // =========================================================
        private void AddFavorite()
        {
            var url = CurrentTab?.CurrentUrl;
            if (string.IsNullOrEmpty(url)) return;
            if (!_favorites.Contains(url))
            {
                _favorites.Add(url);
                SaveFavorites();
                MessageBox.Show(this, "Halaman ditambahkan ke Favorit.", "Kins Browser");
            }
        }

        private void AddHistory(string url)
        {
            if (_history.Count == 0 || _history[^1] != url)
                _history.Add(url);
        }

        private void LoadFavorites()
        {
            try
            {
                if (File.Exists(_favoritesPath))
                {
                    var json = File.ReadAllText(_favoritesPath);
                    var items = JsonSerializer.Deserialize<List<string>>(json);
                    if (items != null) _favorites.AddRange(items);
                }
            }
            catch
            {
                // Abaikan file favorit yang korup; mulai dari kosong.
            }
        }

        private void SaveFavorites()
        {
            try
            {
                File.WriteAllText(_favoritesPath, JsonSerializer.Serialize(_favorites));
            }
            catch
            {
                // Non-fatal: gagal simpan favorit tidak boleh mematikan browser.
            }
        }

        // =========================================================
        //  DAFTAR SITUS MODE IE (auto-buka pakai mesin lawas, mis. untuk aplikasi OWC)
        // =========================================================
        private void LoadIeSites()
        {
            try
            {
                if (File.Exists(_ieSitesPath))
                {
                    var json = File.ReadAllText(_ieSitesPath);
                    var items = JsonSerializer.Deserialize<List<string>>(json);
                    if (items != null) _ieSites.AddRange(items);
                }
            }
            catch
            {
                // Abaikan file yang korup; mulai dari kosong.
            }
        }

        private void SaveIeSites()
        {
            try
            {
                File.WriteAllText(_ieSitesPath, JsonSerializer.Serialize(_ieSites));
            }
            catch
            {
                // Non-fatal.
            }
        }

        private void AddCurrentToIeSiteList()
        {
            var url = CurrentTab?.CurrentUrl;
            if (string.IsNullOrEmpty(url)) return;

            using var f = new Form
            {
                Text = "Tambah ke Daftar Situs Mode IE",
                Width = 520,
                Height = 160,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
            };
            var label = new Label { Text = "Awalan URL (semua alamat yang diawali teks ini akan otomatis dibuka di Mode IE):", Left = 12, Top = 12, Width = 480, Height = 40 };
            var input = new TextBox { Left = 12, Top = 55, Width = 480, Text = url };
            var btnOk = new Button { Text = "Simpan", Left = 320, Top = 85, Width = 80, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Batal", Left = 410, Top = 85, Width = 80, DialogResult = DialogResult.Cancel };
            f.Controls.Add(label);
            f.Controls.Add(input);
            f.Controls.Add(btnOk);
            f.Controls.Add(btnCancel);
            f.AcceptButton = btnOk;
            f.CancelButton = btnCancel;

            if (f.ShowDialog(this) == DialogResult.OK)
            {
                var prefix = input.Text.Trim();
                if (!string.IsNullOrEmpty(prefix) && !_ieSites.Contains(prefix))
                {
                    _ieSites.Add(prefix);
                    SaveIeSites();
                    MessageBox.Show(this, "Ditambahkan. Mulai sekarang alamat itu otomatis dibuka di Mode IE.", "Kins Browser");
                }
            }
        }

        private void ManageIeSiteList()
        {
            using var f = new Form
            {
                Text = "Daftar Situs Mode IE",
                Width = 560,
                Height = 420,
                StartPosition = FormStartPosition.CenterParent,
            };
            var listBox = new ListBox { Left = 12, Top = 12, Width = 520, Height = 300, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };
            listBox.Items.AddRange(_ieSites.Cast<object>().ToArray());

            var btnRemove = new Button { Text = "Hapus Terpilih", Left = 12, Top = 322, Width = 130, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnRemove.Click += (_, _) =>
            {
                if (listBox.SelectedItem is string sel)
                {
                    _ieSites.Remove(sel);
                    listBox.Items.Remove(sel);
                    SaveIeSites();
                }
            };

            var btnClose = new Button { Text = "Tutup", Left = 402, Top = 322, Width = 130, Anchor = AnchorStyles.Bottom | AnchorStyles.Right, DialogResult = DialogResult.OK };

            f.Controls.Add(listBox);
            f.Controls.Add(btnRemove);
            f.Controls.Add(btnClose);
            f.AcceptButton = btnClose;
            f.ShowDialog(this);
        }
    }
}
