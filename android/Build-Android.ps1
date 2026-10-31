param([string]$SdkRoot,[string]$JdkRoot)
$ErrorActionPreference='Stop';$vectorRoot=Split-Path $PSScriptRoot
if(!$SdkRoot){$SdkRoot=$env:ANDROID_SDK_ROOT;if(!$SdkRoot){$SdkRoot=$env:ANDROID_HOME};if(!$SdkRoot){throw 'Set ANDROID_SDK_ROOT or provide -SdkRoot.'}}
if(!$JdkRoot){$JdkRoot=$env:JAVA_HOME;if(!$JdkRoot){throw 'Set JAVA_HOME or provide -JdkRoot.'}}
function Run([string]$exe,[string[]]$arguments){& $exe @arguments;if($LASTEXITCODE -ne 0){throw "$exe failed ($LASTEXITCODE)"}}
$java=Join-Path $JdkRoot 'bin/java.exe';$javac=Join-Path $JdkRoot 'bin/javac.exe';$jar=Join-Path $JdkRoot 'bin/jar.exe';$keytool=Join-Path $JdkRoot 'bin/keytool.exe'
$androidJar=Join-Path $SdkRoot 'platforms/android-35/android.jar';$tools=Join-Path $SdkRoot 'build-tools/35.0.0'
if(!(Test-Path $androidJar)){throw 'Provide Android SDK platform 35 and build-tools 35.0.0 with -SdkRoot.'}
if(!(Test-Path $java)){throw 'Provide JDK 17 or newer with -JdkRoot.'}
$stage=Join-Path $PSScriptRoot ('build/'+[Guid]::NewGuid().ToString('N'));$null=New-Item -ItemType Directory -Force -Path $stage,(Join-Path $stage 'generated'),(Join-Path $stage 'classes'),(Join-Path $stage 'dex'),(Join-Path $stage 'assets')
Copy-Item -Path (Join-Path $vectorRoot 'ui/*') -Destination (Join-Path $stage 'assets') -Recurse
Run (Join-Path $tools 'aapt2.exe') @('compile','--no-crunch','--dir',(Join-Path $PSScriptRoot 'res'),'-o',(Join-Path $stage 'res.zip'))
Run (Join-Path $tools 'aapt2.exe') @('link','-o',(Join-Path $stage 'base.apk'),'-I',$androidJar,'--manifest',(Join-Path $PSScriptRoot 'AndroidManifest.xml'),'--java',(Join-Path $stage 'generated'),'-A',(Join-Path $stage 'assets'),'--min-sdk-version','26','--target-sdk-version','35',(Join-Path $stage 'res.zip'))
$sources=@(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'src'),(Join-Path $stage 'generated') -Filter '*.java' -Recurse | ForEach-Object FullName)
Run $javac (@('-encoding','UTF-8','--release','8','-classpath',$androidJar,'-d',(Join-Path $stage 'classes'))+$sources)
Run $jar @('cf',(Join-Path $stage 'classes.jar'),'-C',(Join-Path $stage 'classes'),'.')
Run $java @('-cp',(Join-Path $tools 'lib/d8.jar'),'com.android.tools.r8.D8','--lib',$androidJar,'--min-api','26','--output',(Join-Path $stage 'dex'),(Join-Path $stage 'classes.jar'))
Add-Type -AssemblyName System.IO.Compression,System.IO.Compression.FileSystem
$zip=[IO.Compression.ZipFile]::Open((Join-Path $stage 'base.apk'),[IO.Compression.ZipArchiveMode]::Update)
try{foreach($dex in Get-ChildItem (Join-Path $stage 'dex') -Filter '*.dex'){$null=[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,$dex.FullName,$dex.Name)}}finally{$zip.Dispose()}
Run (Join-Path $tools 'zipalign.exe') @('-f','-p','4',(Join-Path $stage 'base.apk'),(Join-Path $stage 'aligned.apk'))
$key=Join-Path $PSScriptRoot 'signing/vector-development.jks'
if(!(Test-Path $key)){$null=New-Item -ItemType Directory -Force -Path (Split-Path $key);Run $keytool @('-genkeypair','-keystore',$key,'-storepass','android','-keypass','android','-alias','vector','-keyalg','RSA','-keysize','2048','-validity','10000','-dname','CN=VECTOR Development')}
$null=New-Item -ItemType Directory -Force -Path (Join-Path $vectorRoot 'release');$apk=Join-Path $vectorRoot 'release/VECTOR-Android-0.6.0.apk'
Run $java @('-jar',(Join-Path $tools 'lib/apksigner.jar'),'sign','--ks',$key,'--ks-key-alias','vector','--ks-pass','pass:android','--key-pass','pass:android','--out',$apk,(Join-Path $stage 'aligned.apk'))
Run $java @('-jar',(Join-Path $tools 'lib/apksigner.jar'),'verify','--verbose',$apk)
Write-Host "APK ready: $apk"
