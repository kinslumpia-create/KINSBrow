# Kins Browser

Browser ringan untuk Windows 11 dengan tampilan modern. Punya **dua mesin render**:

- **Mode Modern** (default): mesin Chromium/Edge lewat WebView2 — dipakai untuk
  browsing sehari-hari, bisa buka situs modern apa pun.
- **Mode IE**: mesin Internet Explorer lama (Trident/MSHTML) lewat kontrol
  `WebBrowser` bawaan .NET — satu-satunya mesin yang bisa menjalankan **ActiveX**
  seperti **OWC (Office Web Components)**. Pakai tombol **"IE"** di toolbar, atau
  daftarkan URL tertentu supaya otomatis selalu dibuka di mode ini (menu ☰ →
  "Selalu Buka Situs Ini di Mode IE...").

Sifatnya **portable**: satu file `.exe`, tanpa installer, tanpa perlu hak
akses Administrator.

> **Catatan penting soal OWC:** OWC adalah ActiveX 32-bit, sehingga project ini
> dibuild sebagai **win-x86 (32-bit)**, bukan 64-bit. Kalau di-build sebagai
> 64-bit, tab Mode IE tidak akan bisa memuat OWC sama sekali. Jangan ubah
> `RuntimeIdentifier` di `KinsBrowser.csproj` kecuali kamu yakin OWC tidak lagi
> dipakai.
>
> **Runtime WebView2:** untuk Mode Modern, komputer tujuan butuh WebView2
> Runtime — ini sudah terpasang bawaan di hampir semua Windows 10/11 (dari
> update Edge). Kalau ternyata belum ada, unduh gratis dari
> https://developer.microsoft.com/microsoft-edge/webview2/.

---

## 1. Cara build (sekali saja, butuh Windows)

Karena ini aplikasi Windows Desktop (WinForms), file `.exe`-nya harus
dikompilasi di mesin Windows — saya sertakan source code lengkap + script
build otomatis supaya prosesnya tinggal 1 perintah.

**Prasyarat (hanya untuk proses build, bukan untuk pemakaian akhir):**
- Windows 10/11
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) — instalasi ini butuh admin,
  tapi ini cuma sekali, di komputer developer/yang build. Hasil akhirnya (KinsBrowser.exe)
  bisa dipakai di komputer lain **tanpa** perlu install apa pun.

**Langkah build:**
1. Extract folder `KinsBrowser` ini.
2. Buka PowerShell **biasa** (tidak perlu "Run as Administrator") di dalam folder tsb.
3. Jalankan:
   ```powershell
   .\build.ps1
   ```
4. Setelah selesai, file portable ada di `dist\KinsBrowser.exe`.

Alternatif: buka `KinsBrowser.csproj` langsung di Visual Studio 2022 (Community, gratis),
lalu klik *Publish* dengan target `win-x86` (BUKAN x64 — lihat catatan OWC di atas),
self-contained, single file.

---

## 2. Cara pakai (di komputer pengguna akhir)

1. Copy **hanya** file `KinsBrowser.exe` (dari folder `dist`) ke komputer tujuan —
   lewat flashdisk, folder shared, email, apa saja.
2. Double-click `KinsBrowser.exe`.
3. Selesai — tidak ada wizard instalasi, tidak ada prompt UAC/admin, karena:
   - Aplikasi dipublish sebagai **self-contained single file** (runtime .NET sudah
     dibundel di dalam exe, jadi tidak perlu install .NET terpisah).
   - `app.manifest` memaksa `requestedExecutionLevel = asInvoker`, artinya app
     jalan dengan hak akses user biasa.
   - Pengaturan mode emulasi IE11 ditulis ke registry **HKEY_CURRENT_USER**
     (bukan HKEY_LOCAL_MACHINE), yang memang bisa ditulis oleh user biasa
     tanpa admin.

Kalau mau, taruh shortcut `KinsBrowser.exe` di Desktop — itu juga tidak butuh admin.

---

## 3. Fitur yang sudah ada

- Multi-tab browsing (tombol **+**, klik-tengah tab untuk tutup, atau klik kanan → Tutup Tab)
- Address bar dengan pencarian otomatis (kalau input bukan URL, otomatis dicari lewat Bing)
- Back / Forward / Refresh / Home
- Status bar bergaya IE klasik (status halaman + info zona "Internet"/"Komputer Lokal")
- Menu (ikon ☰): Favorit, Riwayat, Zoom In/Out/Reset, Lihat Source, Cetak, Tentang
- Favorit tersimpan otomatis di `%AppData%\KinsBrowser\favorites.json`
- Tema modern flat: putih bersih, aksen biru, tanpa border tebal ala IE lama

## 4. Batasan yang perlu diketahui (jujur, biar tidak salah ekspektasi)

- WinForms `WebBrowser` control menggunakan mesin **mshtml.dll (Trident)** yang
  masih ada di Windows 11 untuk kompatibilitas aplikasi lama, dikonfigurasi ke
  mode **IE11**. Ini beda dari Edge/Chrome (Chromium) — cocok untuk situs lama,
  intranet perusahaan, atau ActiveX legacy, tapi situs modern yang berat JavaScript
  bisa saja tidak seratus persen mulus, sama seperti IE11 aslinya dulu.
- Microsoft sudah pensiunkan aplikasi `iexplore.exe` itu sendiri, tapi komponen
  mesin render (mshtml.dll) yang dipakai `WebBrowser` control ini masih disertakan
  Windows untuk keperluan kompatibilitas — itulah yang dimanfaatkan project ini.
- Kalau ke depan butuh dukungan situs modern yang lebih penuh sambil tetap punya
  "mode IE" untuk situs lama, opsi lanjutannya adalah menambahkan **WebView2**
  (engine Chromium/Edge, sudah terpasang bawaan di Windows 11) sebagai mesin utama,
  dengan tombol "Buka di Mode IE" untuk situs lama — beri tahu saya kalau mau versi ini.

## 5. Struktur file

```
KinsBrowser/
├── KinsBrowser.csproj   # Project file (.NET 6, WinForms, single-file publish)
├── app.manifest         # Memaksa jalan tanpa admin (asInvoker) + dukungan Win 10/11
├── Program.cs           # Entry point
├── MainForm.cs          # Seluruh UI: tab, toolbar, address bar, menu, favorit, riwayat
├── IeEmulation.cs       # Set registry HKCU supaya WebBrowser pakai mode IE11
├── build.ps1            # Script build 1-klik -> dist\KinsBrowser.exe
└── README.md            # File ini
```
