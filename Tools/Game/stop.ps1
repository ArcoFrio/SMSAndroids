# Closes SMSAndroids' test copy of the game: asks it through ModForge's bridge, then ends it if it has not gone
# in 15 s. Never touches a game somebody is in, nor any other copy.
param([string]$Bridge = 'a0')
. (Join-Path $PSScriptRoot 'test-env.ps1')
Add-Type -Path (Join-Path $ModForge 'Tools\Game\GameNative.cs')
$games = @(GamesRunning | Where-Object { $_.Path -like "$Test*" })
if ($games.Count -eq 0) { "not running"; return }
foreach ($g in $games) {
    if ($g.Id -eq [GameNative]::ForegroundProcess()) { "somebody is in the test copy (pid $($g.Id)) - leaving it"; continue }
    Set-Content (Join-Path $Test "BepInEx\plugins\SMSModForge\DebugBridge\$Bridge\commands.txt") 'quit'
    if (-not $g.WaitForExit(15000)) { Stop-Process -Id $g.Id -Force; "ended pid $($g.Id)" } else { "closed pid $($g.Id)" }
}
