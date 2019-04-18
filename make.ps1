# DebugMode?
$isDebug = $false
#$isDebug = $true

function EndMake() {
    if (!$isDebug) {
        Stop-Transcript | Out-Null
    }

    ''
    Read-Host "終了するには何かキーを教えてください..."
    exit
}

function GetBuildNo(
    [datetime]$timestamp) {
    return ([int]($timestamp.ToString("yy")) + $timestamp.Month + $timestamp.Day).ToString("00") + $timestamp.ToString("HH")
}

# 現在のディレクトリを取得する
$cd = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $cd

if (!$isDebug) {
    Start-Transcript make.log | Out-Null
}

# target
$targetClientDirectory = Get-Item .\source\RINGS
$targetDirectories = @($targetClientDirectory)
$depolyDirectory = ".\source\deploy"

# tools
$msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"

# バージョンを取得する
## ビルド番号を決定する
$timestamp = Get-Date
$buildNo = GetBuildNo($timestamp)

## リリースノートを取得する
if ($isDebug) {
    if (Test-Path .\RELEASE_NOTES.bak) {
        Copy-Item -Force .\RELEASE_NOTES.bak .\RELEASE_NOTES.xml 
    }
}
$releaseNotesXML = [xml] (Get-Content .\RELEASE_NOTES.xml -Encoding utf8)
if ($releaseNotesXML -eq $null) {
    EndMake
}
$lastestNote = $releaseNotesXML.release_notes.note | Select-Object -Last 1

## バージョン番号を決定する
$version = $lastestNote.version.Replace("#BuildNo#", $buildNo)
$channel = $lastestNote.channel
$tag = ("v" + $version + "-" + $channel)

## アプリケーション名を取得する
$appName = $releaseNotesXML.release_notes.name

## アーカイブファイル名を決定する
$archiveFileName = $appName + "_v" + $version.Replace(".", "_") + "_" + $channel + ".zip"

## リリースノートを置換する
$content = Get-Content .\RELEASE_NOTES.xml -Encoding utf8
$content = $content.Replace("#BuildNo#", $buildNo)
$content = $content.Replace("#Timestamp#", $timestamp.ToString("o"))
$content = $content.Replace("#ArchiveFileName#", $archiveFileName)
$content = $content.Replace("#Tag#", $tag)
Copy-Item -Force .\RELEASE_NOTES.xml .\RELEASE_NOTES.bak
$content | Out-File -FilePath .\RELEASE_NOTES.xml -Encoding utf8

Write-Output "***"
Write-Output ("*** " + $appName + " v" + $version + " " + $channel + " ***")
Write-Output "***"

# Version.cs を上書きする
$content = Get-Content .\source\tools\Version.master.cs -Encoding utf8
$content = $content.Replace("#VERSION#", $version)
$content = $content.Replace("#CHANNEL#", $channel)
foreach ($d in $targetDirectories) {
    $content | Out-File -FilePath (Join-Path $d "Version.cs") -Encoding utf8
}

'-> Build'
# Delete Release Directory
foreach ($d in $targetDirectories) {
    $out = Join-Path $d "bin\Release\*"
    if (Test-Path $out) {
        Remove-Item -Recurse -Force $out
    }
}

$target = Get-Item .\source\*.sln
& $msbuild $target /nologo /v:minimal /t:Clean /p:Configuration=Release
Start-Sleep -m 100

'-> Build Client'
$target = Get-Item $targetClientDirectory\*.csproj
& $msbuild $target /nologo /v:minimal /t:Build /p:Configuration=Release | Write-Output
Start-Sleep -m 100

# Successed? build
foreach ($d in $targetDirectories) {
    $out = Join-Path $d "bin\Release"
    if (!(Test-Path $out)) {
        EndMake
    }
}

# pdb を削除する
foreach ($d in $targetDirectories) {
    Remove-Item -Force (Join-Path $d "bin\Release\*.pdb")
}

'-> Deploy'
# deploy ディレクトリを作る
if (!(Test-Path $depolyDirectory)) {
    New-Item -ItemType Directory $depolyDirectory >$null
}

$deployBase = Join-Path $depolyDirectory $archiveFileName.Replace(".zip", "")
if (Test-Path $deployBase) {
    Get-ChildItem -Path $deployBase -Recurse | Remove-Item -Force -Recurse
    Remove-Item -Recurse -Force $deployBase
}

$deployClient = $deployBase
New-Item -ItemType Directory $deployClient >$null

# client を配置する
'-> Deploy Client'
Copy-Item -Force -Recurse $targetClientDirectory\bin\Release\* $deployClient

# client をアーカイブする
'-> Archive Client'
Compress-Archive -Force $deployClient\* $deployBase\..\$archiveFileName
Get-ChildItem -Path $deployBase -Recurse | Remove-Item -Force -Recurse
Remove-Item -Recurse -Force $deployBase

if (!$isDebug) {
    if (Test-Path .\RELEASE_NOTES.bak) {
        Remove-Item -Force .\RELEASE_NOTES.bak
    }
}

$tag | Out-File .\CURRENT_VERSION -Encoding utf8

Write-Output "***"
Write-Output ("*** " + $appName + " v" + $version + " " + $channel + ", Completed! ***")
Write-Output "***"

EndMake
