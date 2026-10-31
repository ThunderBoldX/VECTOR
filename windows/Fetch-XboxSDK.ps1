param()
$ErrorActionPreference='Stop'
$vendor=Join-Path $PSScriptRoot 'vendor';$null=New-Item -ItemType Directory -Force -Path $vendor
$package=Join-Path $vendor 'client.nupkg';$driver=Join-Path $vendor 'ViGEmBus_1.22.0.exe'
Invoke-WebRequest 'https://api.nuget.org/v3-flatcontainer/nefarius.vigem.client/1.21.256/nefarius.vigem.client.1.21.256.nupkg' -OutFile $package
if((Get-FileHash $package -Algorithm SHA256).Hash -ne 'FF460031B33E0882CD1166FE2D072D0FE97578480C61ED85DCCFA6C930FDB55E'){throw 'Unexpected SDK package hash'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip=[IO.Compression.ZipFile]::OpenRead($package)
try{[IO.Compression.ZipFileExtensions]::ExtractToFile($zip.GetEntry('lib/netstandard2.0/Nefarius.ViGEm.Client.dll'),(Join-Path $vendor 'Nefarius.ViGEm.Client.dll'),$true)}finally{$zip.Dispose()}
$assembly=[Reflection.Assembly]::LoadFile((Join-Path $vendor 'Nefarius.ViGEm.Client.dll'))
$stream=$assembly.GetManifestResourceStream('costura64.vigemclient.dll');$out=[IO.File]::Create((Join-Path $PSScriptRoot 'ViGEmClient.dll'))
try{$stream.CopyTo($out)}finally{$out.Dispose();$stream.Dispose()}
if((Get-FileHash (Join-Path $PSScriptRoot 'ViGEmClient.dll') -Algorithm SHA256).Hash -ne '00303F99E1968D523FBD08A7C691D60D145B7335059EC593947841D124C10875'){throw 'Unexpected native SDK hash'}
Invoke-WebRequest 'https://github.com/nefarius/ViGEmBus/releases/download/v1.22.0/ViGEmBus_1.22.0_x64_x86_arm64.exe' -OutFile $driver
if((Get-FileHash $driver -Algorithm SHA256).Hash -ne '89220A7865076B342892F98865F3499FB7C4CFD673159E89D352C360FD014C6A'){throw 'Unexpected driver hash'}
$signature=Get-AuthenticodeSignature $driver
if($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch 'Nefarius Software Solutions'){throw 'Driver signature validation failed'}
Write-Host 'Official SDK and signed driver ready. Driver was not installed.'
