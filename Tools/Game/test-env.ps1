# Shared by the scripts here (dot-sourced): where SMSAndroids' test copy of the game is and how its saves are
# kept. The arrangement is ModForge's (SMSModForge/IN_GAME_LOOK.md, "Testing in the game without touching the
# author's saves"), with a copy, a name and a colour of SMSAndroids' own.
#
# The author's saves must never change (2026-10-24). Every copy of the game saves into
# LocalLow\Arvus Games\<product>\Saves, and the game keeps its list of save slots inside slot 0, shared by every
# slot, so a copy saving beside the author's would write their slot 0. So the test copy is renamed: its saves,
# settings and window state are its own, and its saves are a copy of the author's slots 0 and 1, put back clean
# before every start.

$Game = 'D:\Downloads\SkyrimM\spec\uTorrent\Starmaker Story 1.8E'
# Ends in " test": ModForge's and the Gallery's launchers know a folder named so is another check's copy, not
# the author's game.
$Test = 'D:\Downloads\SkyrimM\spec\uTorrent\Starmaker Story 1.8E - Androids test'
$Repos = Split-Path (Split-Path (Split-Path $PSScriptRoot))
$ModForge = Join-Path $Repos 'SMSModForge'
$Androids = Join-Path $Repos 'SMSAndroids'
# The folder the author edits the pack in; the repo's SMSAndroidsPack is brought up to date from it.
$WorkingPack = Join-Path $Repos '- AI Gen\sd-webui-forge-neo\output\- STARMAKER MOD-\SMSAndroidsPack'
$RepoPack = Join-Path $Androids 'SMSAndroidsPack'
$Scratch = Join-Path ([IO.Path]::GetTempPath()) 'SMSAndroids-ingame'

$AuthorsProduct = 'Starmaker Story'
# The same length as the name it replaces, so nothing after it in the game's data moves.
$TestProduct = 'SMSAndroidTests'
$TestSlot = 1
$TestSaveTitle = '<color=#4FD1C5>Androids test save</color>'

function LocalLow { Join-Path (Split-Path ([Environment]::GetFolderPath('ApplicationData'))) 'LocalLow\Arvus Games' }
function AuthorsSaves { Join-Path (LocalLow) "$AuthorsProduct\Saves" }
function TestSaves { Join-Path (LocalLow) "$TestProduct\Saves" }
function CleanTestSaves { Join-Path $Test 'TestSave\Saves' }

# The test copy's saves as the clean copy has them, whatever the last check did. Never the author's folder.
function RestoreTestSave {
    $clean = CleanTestSaves
    if (-not (Test-Path (Join-Path $clean ('NANOSAVE_{0:D4}\SAVE.GZ' -f $TestSlot)))) {
        throw "No clean test save at $clean - run make-test-save.ps1 first."
    }
    $saves = TestSaves
    if ($saves -eq (AuthorsSaves) -or $saves -notlike "*\$TestProduct\Saves") { throw "Not the test copy's saves: $saves" }
    if (Test-Path $saves) { Remove-Item $saves -Recurse -Force }
    New-Item -ItemType Directory -Force (Split-Path $saves) | Out-Null
    Copy-Item $clean $saves -Recurse
    "test saves put back clean ($saves)"
}

# The product name in the test copy's game data, from one name to the other.
function RenameProduct([string]$from, [string]$to) {
    if ($from.Length -ne $to.Length) { throw "The names must be the same length." }
    $data = Join-Path $Test 'Starmaker Story_Data'
    foreach ($file in 'globalgamemanagers', 'app.info') {
        $path = Join-Path $data $file
        $bytes = [IO.File]::ReadAllBytes($path)
        $text = [Text.Encoding]::ASCII.GetString($bytes)
        $at = $text.IndexOf($from)
        if ($at -lt 0) {
            if ($text.IndexOf($to) -ge 0) { continue }   # already renamed
            throw "No '$from' in $path - the game's data has changed."
        }
        if ($text.IndexOf($from, $at + 1) -ge 0) { throw "'$from' is in $path more than once; not guessing which." }
        [Text.Encoding]::ASCII.GetBytes($to).CopyTo($bytes, $at)
        [IO.File]::WriteAllBytes($path, $bytes)
    }
}

function GamesRunning { @(Get-Process -Name 'Starmaker Story' -ErrorAction SilentlyContinue) }

# Another check's copy (a folder ending " test") is not the author's game.
function IsAnotherTestCopy($process) {
    $process.Path -and ((Split-Path (Split-Path $process.Path) -Leaf) -match ' test$')
}
