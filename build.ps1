# build.ps1
# Jalankan script ini di PowerShell (tidak perlu Run as Administrator) dari dalam folder KinsBrowser.
# Hasil akhir berupa satu file KinsBrowser.exe portable di folder .\dist

Write-Host "== Kins Browser - Build Portable EXE ==" -ForegroundColor Cyan

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Host "dotnet SDK tidak ditemukan. Silakan install .NET 6 SDK dulu dari:" -ForegroundColor Yellow
    Write-Host "https://dotnet.microsoft.com/download/dotnet/6.0"
    exit 1
}

dotnet publish .\KinsBrowser.csproj -c Release -r win-x86 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -o .\dist

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Build sukses! File portable ada di: .\dist\KinsBrowser.exe" -ForegroundColor Green
    Write-Host "Tinggal double-click KinsBrowser.exe -- tidak perlu instalasi, tidak perlu admin." -ForegroundColor Green
} else {
    Write-Host "Build gagal, cek pesan error di atas." -ForegroundColor Red
}
