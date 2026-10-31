param([string]$ElectronRoot,[string]$Csc)
$ErrorActionPreference='Stop';$vectorRoot=Split-Path $PSScriptRoot
if(!$ElectronRoot){$ElectronRoot=Join-Path $PSScriptRoot 'node_modules/electron/dist'}
if(!$Csc){$Csc=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'}
if(!(Test-Path (Join-Path $ElectronRoot 'electron.exe'))){throw 'Provide Electron Windows x64 runtime with -ElectronRoot.'}
if(!(Test-Path (Join-Path $PSScriptRoot 'ViGEmClient.dll')) -or !(Test-Path (Join-Path $PSScriptRoot 'vendor/ViGEmBus_1.22.0.exe'))){throw 'Run windows/Fetch-XboxSDK.ps1 to prepare the Xbox SDK and driver package.'}
& $Csc /nologo /optimize+ /platform:x64 /target:exe ('/out:'+(Join-Path $PSScriptRoot 'VectorInput.exe')) (Join-Path $PSScriptRoot 'VectorInput.cs') (Join-Path $PSScriptRoot 'XboxInput.cs')
if($LASTEXITCODE -ne 0){throw 'Native input build failed'}
& (Join-Path $PSScriptRoot 'VectorInput.exe') --self-test
if($LASTEXITCODE -ne 0){throw 'Native ABI self-test failed'}
$release=Join-Path $vectorRoot 'release/windows-0.6';$null=New-Item -ItemType Directory -Force -Path $release
Copy-Item -Path (Join-Path $ElectronRoot '*') -Destination $release -Recurse -Force
$exe=Join-Path $release 'electron.exe';$target=Join-Path $release 'VECTOR Host.exe'
if(Test-Path $target){Remove-Item -LiteralPath $target};Move-Item -LiteralPath $exe -Destination $target
$appRoot=Join-Path $release 'resources/app';$null=New-Item -ItemType Directory -Force -Path $appRoot,(Join-Path $appRoot 'ui')
Get-ChildItem -LiteralPath $PSScriptRoot -File | Where-Object {$_.Extension -in '.cjs','.json','.exe','.dll'} | Copy-Item -Destination $appRoot -Force
$null=New-Item -ItemType Directory -Force -Path (Join-Path $appRoot 'vendor')
Get-ChildItem (Join-Path $PSScriptRoot 'vendor') -File | Where-Object {$_.Extension -in '.exe','.txt'} | Copy-Item -Destination (Join-Path $appRoot 'vendor') -Force
Copy-Item -Path (Join-Path $vectorRoot 'ui/*') -Destination (Join-Path $appRoot 'ui') -Recurse -Force
Copy-Item -LiteralPath (Join-Path $vectorRoot 'README_UK.md') -Destination $release -Force
Write-Host "Windows app ready: $target"
