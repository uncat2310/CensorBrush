# Build CensorBrush.exe (single file) with the .NET Framework compiler.
# Run in this folder:  powershell -ExecutionPolicy Bypass -File build.ps1
$ErrorActionPreference = 'Stop'
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path $csc)) { throw "csc.exe not found: $csc (.NET Framework 4.x is required)" }
$buildArgs = @(
  '/nologo', '/target:winexe', '/optimize+',
  '/win32manifest:CensorBrush.manifest',
  '/reference:System.dll', '/reference:System.Drawing.dll', '/reference:System.Windows.Forms.dll',
  '/out:CensorBrush.exe', 'CensorBrush.cs'
)
if (Test-Path 'CensorBrush.ico') { $buildArgs = @('/win32icon:CensorBrush.ico') + $buildArgs }
& $csc @buildArgs
if ($LASTEXITCODE -ne 0) { throw "build failed (csc exit $LASTEXITCODE)" }
Write-Host "Built: CensorBrush.exe"
