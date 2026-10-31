param([string]$MakeNsis)
$ErrorActionPreference='Stop';$vectorRoot=Split-Path $PSScriptRoot
if(!$MakeNsis){$tool=Get-Command makensis.exe -ErrorAction SilentlyContinue;if($tool){$MakeNsis=$tool.Source}else{throw 'Install NSIS and provide -MakeNsis C:/NSIS/makensis.exe.'}}
$release=Join-Path $vectorRoot 'release/windows-0.6';$out=Join-Path $vectorRoot 'release/VECTOR-Host-Windows-0.6.0-Setup.exe'
if(!(Test-Path (Join-Path $release 'VECTOR Host.exe'))){throw 'Run Build-Windows.ps1 first'}
$uninstall=Get-ChildItem -LiteralPath $release -Recurse -File | ForEach-Object { '  Delete "$INSTDIR\'+$_.FullName.Substring($release.Length+1)+'"' }
$dirs=Get-ChildItem -LiteralPath $release -Recurse -Directory | Sort-Object {$_.FullName.Length} -Descending | ForEach-Object { '  RMDir "$INSTDIR\'+$_.FullName.Substring($release.Length+1)+'"' }
$nsis=@'
Unicode true
!include "MUI2.nsh"
Name "VECTOR Host"
OutFile "__OUTPUT__"
InstallDir "$LOCALAPPDATA\VECTOR Host"
RequestExecutionLevel user
SetCompressor /SOLID lzma
!define MUI_ABORTWARNING
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\VECTOR Host.exe"
!define MUI_FINISHPAGE_RUN_NOTCHECKED
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"
Section "VECTOR Host"
 SetOutPath "$INSTDIR"
 File /r "__PAYLOAD__\*.*"
 WriteUninstaller "$INSTDIR\Uninstall.exe"
 CreateDirectory "$SMPROGRAMS\VECTOR"
 CreateShortCut "$SMPROGRAMS\VECTOR\VECTOR Host.lnk" "$INSTDIR\VECTOR Host.exe"
 CreateShortCut "$DESKTOP\VECTOR Host.lnk" "$INSTDIR\VECTOR Host.exe"
 WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\VECTOR Host" "DisplayName" "VECTOR Host"
 WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\VECTOR Host" "DisplayVersion" "0.6.0"
 WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\VECTOR Host" "UninstallString" '"$INSTDIR\Uninstall.exe"'
 WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\VECTOR Host" "InstallLocation" "$INSTDIR"
SectionEnd
Section "Uninstall"
 Delete "$DESKTOP\VECTOR Host.lnk"
 Delete "$SMPROGRAMS\VECTOR\VECTOR Host.lnk"
 RMDir "$SMPROGRAMS\VECTOR"
 DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\VECTOR Host"
__DELETE_FILES__
__DELETE_DIRS__
 Delete "$INSTDIR\Uninstall.exe"
 RMDir "$INSTDIR"
SectionEnd
'@
$nsis=$nsis.Replace('__OUTPUT__',$out).Replace('__PAYLOAD__',$release).Replace('__DELETE_FILES__',($uninstall -join "`n")).Replace('__DELETE_DIRS__',($dirs -join "`n"))
$script=Join-Path $PSScriptRoot 'vector-setup.nsi';Set-Content -LiteralPath $script -Value $nsis -Encoding utf8
& $MakeNsis /V2 $script
if($LASTEXITCODE -ne 0){throw 'Installer build failed'}
Write-Host "Installer ready: $out"
