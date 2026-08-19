# Build Otomatis Lewat GitHub Actions (Tanpa Perlu Windows Sendiri)

Cara ini memakai komputer Windows **gratis milik GitHub** untuk meng-compile
`KinsBrowser.exe` untukmu. Kamu cuma perlu browser & akun GitHub (gratis).

## Langkah 1 — Buat akun & repository
1. Buka https://github.com dan daftar (kalau belum punya akun).
2. Klik tombol hijau **"New"** / **"+"** → **New repository**.
3. Isi nama, misalnya `kins-browser`. Boleh **Public** atau **Private**, sama-sama gratis untuk fitur ini.
4. Klik **Create repository**.

## Langkah 2 — Upload isi folder `KinsBrowser` ke repo
Paling gampang lewat browser, tanpa install git:
1. Di halaman repo yang baru dibuat, klik **"uploading an existing file"**.
2. Extract `KinsBrowser.zip` di komputermu, lalu **drag & drop semua isi folder**
   `KinsBrowser` (termasuk folder tersembunyi `.github`) ke halaman upload itu.
   - Kalau drag & drop tidak menyertakan folder `.github` (kadang browser
     menyembunyikannya), upload dulu semua file lain, lalu ulangi upload khusus
     untuk `.github/workflows/build.yml` lewat menu **Add file → Create new file**
     dan beri nama path `.github/workflows/build.yml`, isi dengan konten file tersebut.
3. Scroll ke bawah, klik **Commit changes**.

## Langkah 3 — Tunggu build otomatis jalan
1. Klik tab **Actions** di bagian atas repo.
2. Akan muncul proses **"Build Kins Browser (Windows EXE)"** yang sedang berjalan
   (ikon kuning berputar). Ini otomatis terpicu begitu kamu commit ke langkah 2.
3. Tunggu sekitar 1–3 menit sampai ikonnya jadi centang hijau ✅.

## Langkah 4 — Download EXE hasil build
1. Klik run workflow yang sudah selesai (yang centang hijau).
2. Scroll ke bagian **Artifacts** di bawah halaman.
3. Klik **KinsBrowser-portable** untuk download — isinya file `KinsBrowser.exe`.
4. Extract, lalu double-click `KinsBrowser.exe` — langsung jalan, tanpa instalasi, tanpa admin.

## Build ulang di kemudian hari
Setiap kali kamu edit source code dan commit lagi ke branch `main`, workflow ini
otomatis jalan lagi dan menghasilkan exe versi terbaru di tab **Actions**.

Kalau mau bikin versi "resmi" yang muncul di halaman **Releases** (biar lebih
rapi untuk dibagikan), tinggal buat **tag** versi, misalnya `v1.0.0`, lewat
menu **Releases → Draft a new release** di GitHub — exe otomatis terlampir di situ.

## Kalau ada error saat build
Klik run yang gagal (ikon silang merah) di tab Actions, lalu buka log step yang
merah untuk lihat pesan errornya — paling sering karena ada file yang ke-skip
saat upload manual (terutama `.github/workflows/build.yml` atau `app.manifest`).
Pastikan struktur file di GitHub persis sama dengan struktur di dalam zip.
