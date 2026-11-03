# Makes, or brings up to date, SMSAndroids' test copy of the game, beside the real one. The author's game folder
# is only read. The copy carries the game, BepInEx's core and configuration and ModForge with what it needs - what
# a player of the Androids pack has, since the pack needs no plugin of its own (2026-11-02) - plus SMSAndroids'
# test bridge. Not the author's debugging tools (UnityExplorer, RuntimeUnityEditor, SMSDiagDebug) nor the Gallery,
# so a check sees ModForge and the pack alone.
#
#   powershell -File Tools\Game\setup-test-game.ps1 -Build
#   powershell -File Tools\Game\setup-test-game.ps1 -Build -WithPlugin    # and SMSAndroidsCore (the massage minigame)
#
# -Build builds into the copy (never into the author's game): ModForge's plugin in Debug, which carries its test
# bridge, and SMSAndroids' test bridge (Tools/Game/TestBridge, test-only). Both bridges sleep unless launch.ps1
# started the game. -WithPlugin also builds SMSAndroidsCore there; without it any copy of it is taken out.
param([switch]$Build, [switch]$WithPlugin)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'test-env.ps1')
if (-not (Test-Path "$Game\Starmaker Story.exe")) { throw "No game at $Game" }
if (GamesRunning | Where-Object { $_.Path -like "$Test*" }) { throw "The test copy is running; stop.ps1 first." }

function Mirror($from, $to) {
    robocopy $from $to /E /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "copy failed: $from" }
}

New-Item -ItemType Directory -Force $Test, "$Test\BepInEx\plugins\SMSModForge", "$Test\Mods" | Out-Null
foreach ($f in 'Starmaker Story.exe', 'UnityPlayer.dll', 'UnityCrashHandler64.exe', 'winhttp.dll',
               '.doorstop_version', 'doorstop_config.ini') {
    if (Test-Path -LiteralPath (Join-Path $Game $f)) { Copy-Item -LiteralPath (Join-Path $Game $f) -Destination $Test -Force }
}
foreach ($d in 'Starmaker Story_Data', 'MonoBleedingEdge', 'D3D12') { Mirror (Join-Path $Game $d) (Join-Path $Test $d) }
# The copy above puts the game's own name back each time.
RenameProduct $AuthorsProduct $TestProduct

foreach ($d in 'core', 'config', 'patchers') {
    if (Test-Path "$Game\BepInEx\$d") { Mirror "$Game\BepInEx\$d" "$Test\BepInEx\$d" }
}
# Never a console window: it would open on the author's screen and take the focus.
$cfg = "$Test\BepInEx\config\BepInEx.cfg"
$text = Get-Content $cfg -Raw
$text = $text -replace '(?ms)(\[Logging\.Console\].*?^Enabled\s*=\s*)true', '${1}false'
Set-Content $cfg $text -NoNewline -Encoding UTF8
Get-ChildItem "$Test\BepInEx\config\*.cfg" | Where-Object { $_.Name -match 'unityexplorer|RuntimeUnityEditor' } | Remove-Item

# What the two plugins load beside them.
foreach ($dll in 'Newtonsoft.Json.dll', 'Unity.InputSystem.dll', 'Unity.InputSystem.ForUI.dll', 'Unity.TextMeshPro.dll',
                 'UnityEngine.AssetBundleModule.dll', 'UnityEngine.JSONSerializeModule.dll', 'UnityEngine.ParticleSystemModule.dll') {
    Copy-Item "$Game\BepInEx\plugins\$dll" "$Test\BepInEx\plugins\" -Force
}
Mirror "$Game\BepInEx\plugins\VanillaFrames" "$Test\BepInEx\plugins\VanillaFrames"
if ($WithPlugin) { Mirror "$Game\BepInEx\plugins\SMSAndroidsCore" "$Test\BepInEx\plugins\SMSAndroidsCore" }
else {
    # What players have: no SMSAndroidsCore. Its folder keeps nothing a check needs (the test bridge has its own).
    Remove-Item "$Test\BepInEx\plugins\SMSAndroidsCore.dll", "$Test\BepInEx\plugins\SMSAndroidsCore.pdb" -ErrorAction SilentlyContinue
    Remove-Item "$Test\BepInEx\plugins\SMSAndroidsCore" -Recurse -Force -ErrorAction SilentlyContinue
}
Mirror "$ModForge\SMSModForge\Languages" "$Test\BepInEx\plugins\SMSModForge\Languages"
Remove-Item "$Test\BepInEx\plugins\SMSModForge\Languages\en.txt" -ErrorAction SilentlyContinue

if ($Build) {
    $msbuild = Get-ChildItem 'C:\Program Files\Microsoft Visual Studio\2022\*\MSBuild\Current\Bin\MSBuild.exe' | Select-Object -First 1
    if (-not $msbuild) { throw "No MSBuild (Visual Studio 2022) found." }
    # A path argument ends in \: a lone \ before the closing quote would escape it.
    $plugins = "$Test\BepInEx\plugins"
    # Intermediate files in a folder of their own: ModForge's agent may be building the same project.
    & $msbuild.FullName "$ModForge\SMSModForge.PackPlugin\SMSModForge.PackPlugin.csproj" -p:Configuration=Debug `
        "-p:PluginOutputPath=$plugins\\" "-p:IntermediateOutputPath=$Scratch\obj\PackPlugin\\" -v:m -nologo |
        Select-String ' error |-> '
    if ($LASTEXITCODE -ne 0) { throw "ModForge's plugin did not build." }
    & $msbuild.FullName "$Androids\Tools\Game\TestBridge\SMSAndroidsTestBridge.csproj" -restore -p:Configuration=Debug `
        "-p:OutputPath=$plugins\\" "-p:BaseIntermediateOutputPath=$Scratch\obj\TestBridge\\" -v:m -nologo |
        Select-String ' error |-> '
    if ($LASTEXITCODE -ne 0) { throw "The test bridge did not build." }
    if ($WithPlugin) {
        & $msbuild.FullName "$Androids\SMSAndroids\SMSAndroidsCore.csproj" -p:Configuration=Debug "-p:OutputPath=$plugins\\" `
            "-p:ReferencePath=$Game\BepInEx\plugins\\" "-p:IntermediateOutputPath=$Scratch\obj\SMSAndroidsCore\\" -v:m -nologo |
            Select-String ' error |-> '
        if ($LASTEXITCODE -ne 0) { throw "SMSAndroidsCore did not build." }
    }
}
"test copy ready: $Test"
