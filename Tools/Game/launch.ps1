# Starts SMSAndroids' test copy of the game for an in-game check, by the author's rules: never on their screen,
# never taking their focus, muted, only the test save, and not while their own game runs. The same launcher as
# ModForge's (SMSModForge/Tools/Game/launch.ps1, whose GameNative.cs this uses), for this mod's copy.
#
#   powershell -File Tools\Game\launch.ps1              # the test save, straight into the game, muted
#   powershell -File Tools\Game\launch.ps1 -Slot 0      # stay on the main menu
#
# It runs on a desktop of its own: Unity activates its window while it starts, and on the author's machine any
# activation takes the foreground. Both bridges answer under the name given (-Bridge): ModForge's in
# BepInEx\plugins\SMSModForge\DebugBridge\<name>, SMSAndroids' in BepInEx\plugins\SMSAndroidsCore\DebugBridge\<name>.
# Drive it with ingame_check.py (or bridge.ps1), close it with stop.ps1.
param(
    [string]$Bridge = 'a0',
    [int]$Slot = 1,
    [int]$Width = 1280,
    [int]$Height = 720,
    [switch]$NotTheAuthors
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'test-env.ps1')
Add-Type -Path (Join-Path $ModForge 'Tools\Game\GameNative.cs')

$exe = Join-Path $Test 'Starmaker Story.exe'
if (-not (Test-Path $exe)) { throw "No test copy at $Test - run setup-test-game.ps1 -Build" }
foreach ($p in GamesRunning) {
    if ($p.Path -like "$Test*") { throw "The test copy is already running (pid $($p.Id)); stop.ps1 first." }
    if ($p.Id -eq [GameNative]::ForegroundProcess()) { throw "Somebody is in the game (pid $($p.Id)); not starting." }
    if (-not $NotTheAuthors -and -not (IsAnotherTestCopy $p)) { throw "Another copy of the game is running (pid $($p.Id)) - the author's? Not starting." }
}

$info = Get-Content (Join-Path $Test 'Starmaker Story_Data\app.info') -Raw
if ($info -notmatch [Regex]::Escape($TestProduct)) { throw "The test copy is not renamed '$TestProduct' - it would save among the author's saves. Run setup-test-game.ps1." }
if ($Slot -ne 0 -and $Slot -ne $TestSlot) { throw "Checks play only the test save (slot $TestSlot of the test copy's own saves)." }
RestoreTestSave

foreach ($dir in "$Test\BepInEx\plugins\SMSModForge\DebugBridge\$Bridge", "$Test\BepInEx\plugins\SMSAndroidsCore\DebugBridge\$Bridge") {
    New-Item -ItemType Directory -Force $dir | Out-Null
    Remove-Item "$dir\commands.txt", "$dir\results.txt" -ErrorAction SilentlyContinue
}

$gameArgs = @('-screen-fullscreen', '0', '-screen-width', $Width, '-screen-height', $Height,
              '-mfBridge', $Bridge, '-logFile', "`"$Test\Player-$Bridge.log`"", '-mfMute')
if ($Slot -gt 0) { $gameArgs += @('-mfLoadSlot', $Slot) } else { $gameArgs += @('-mfTestSlot', $TestSlot) }

$gamePid = [GameNative]::StartOnOwnDesktop($exe, ($gameArgs -join ' '), $Test, "SMSAndroidsGame-$Bridge")
"started pid $gamePid (on a desktop of its own)"
# Muted as soon as it has a sound session (the bridge's -mfMute is the second lock).
$deadline = (Get-Date).AddSeconds(45)
while ((Get-Date) -lt $deadline) {
    if (-not (Get-Process -Id $gamePid -ErrorAction SilentlyContinue)) { throw "The game exited while starting - see $Test\Player-$Bridge.log" }
    if ([GameNative]::SetProcessMute($gamePid, $true) -gt 0) { "muted (its own sound only)"; break }
    Start-Sleep -Milliseconds 200
}
$gamePid
