param(
    [Parameter(Mandatory=$true)][string]$GameFolder,
    [string]$DnlibPath = "$env:USERPROFILE\.nuget\packages\dnlib\4.5.0\lib\net45\dnlib.dll"
)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot
Push-Location $repoRoot
try {
    New-Item -ItemType Directory -Force validation | Out-Null
    $gameRoot = (Resolve-Path -LiteralPath $GameFolder).Path
    $readFiles = @(Get-ChildItem "$gameRoot\bin\Win64_Shipping_Client\TaleWorlds.*.dll")
    foreach ($module in @('Native','SandBox','SandBoxCore','StoryMode','NavalDLC','Bannerlord.Harmony','Bannerlord.MBOptionScreen')) {
        $readFiles += @(Get-ChildItem "$gameRoot\Modules\$module\bin\Win64_Shipping_Client\*.dll" -ErrorAction SilentlyContinue)
        $manifest = "$gameRoot\Modules\$module\SubModule.xml"
        if (Test-Path -LiteralPath $manifest) { $readFiles += Get-Item -LiteralPath $manifest }
    }
    $before = @($readFiles | Sort-Object FullName -Unique | ForEach-Object {
        [pscustomobject]@{ Path=$_.FullName; FileVersion=$_.VersionInfo.FileVersion; SHA256=(Get-FileHash -LiteralPath $_.FullName).Hash }
    })
    $before | Export-Csv validation/installed-dependencies.csv -NoTypeInformation
    & dotnet build src/TimeLord.csproj -c Release -p:Platform=x64 -p:UseSharedCompilation=false "-p:GameFolder=$gameRoot" --nologo -v:minimal *> validation/build-release.txt
    if ($LASTEXITCODE -ne 0) { throw 'Release build failed; see validation/build-release.txt' }
    & dotnet build tools/Validation/Validation.csproj -c Release "-p:GameFolder=$gameRoot" "-p:DnlibPath=$DnlibPath" --nologo -v:minimal *> validation/test-build.txt
    if ($LASTEXITCODE -ne 0) { throw 'Validation harness build failed; see validation/test-build.txt' }
    & ./tools/Validation/bin/Release/net472/Validation.exe (Resolve-Path src/bin/x64/Release/net472/TimeLord.dll).Path $gameRoot *> validation/checks.txt
    if ($LASTEXITCODE -ne 0) { throw 'Compatibility checks failed; see validation/checks.txt' }
    $changed = @($before | Where-Object { (Get-FileHash -LiteralPath $_.Path).Hash -ne $_.SHA256 })
    "Inspected installed DLL/manifest hash changes: $($changed.Count)" | Set-Content validation/integrity.txt
    if ($changed.Count -ne 0) { throw 'Installed file hashes changed during validation.' }
    Get-FileHash src/bin/x64/Release/net472/TimeLord.dll | Format-List | Out-String | Set-Content validation/output-sha256.txt
    Get-Content validation/build-release.txt
    Get-Content validation/checks.txt | Select-Object -Last 1
    Get-Content validation/integrity.txt
} finally { Pop-Location }
