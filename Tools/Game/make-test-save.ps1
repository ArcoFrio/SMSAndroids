# Makes the save the in-game checks play in, from the author's slot 0 (the game's list of saves) and one of
# their slots (1 by default) - whole folders, so ModForge's per-pack files come too. The author's files are only
# read: hashed before and after, and the script fails if one changed. Kept clean in TestSave\ beside the test
# copy; launch.ps1 puts it back before every start, so no check is affected by the one before it.
#
#   powershell -File Tools\Game\make-test-save.ps1 [-Refresh] [-From 2]
param([int]$From = 1, [switch]$Refresh, [switch]$NotTheAuthors)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'test-env.ps1')
$authors = AuthorsSaves
$global = Join-Path $authors 'NANOSAVE_0000'
$source = Join-Path $authors ('NANOSAVE_{0:D4}' -f $From)
if ($From -lt 1) { throw "Slot 0 is the game's list of saves, not a save." }
if (-not (Test-Path (Join-Path $source 'SAVE.GZ'))) { throw "No save in slot $From ($source)." }
foreach ($p in GamesRunning) {
    if ($p.Path -like "$Test*") { throw "The test copy is running; stop.ps1 first." }
    # Only the author's game may be writing the very save being copied.
    if (-not $NotTheAuthors -and -not (IsAnotherTestCopy $p)) { throw "A copy of the game is running (pid $($p.Id)); not copying." }
}

$clean = CleanTestSaves
if ((Test-Path $clean) -and -not $Refresh) { "clean test saves already kept ($clean)"; RestoreTestSave; return }

function Hashes($dir) { Get-ChildItem $dir -File | ForEach-Object { $_.FullName + ' ' + (Get-FileHash $_.FullName -Algorithm SHA256).Hash } }
$before = @(Hashes $global) + @(Hashes $source)

if (Test-Path $clean) { Remove-Item $clean -Recurse -Force }
New-Item -ItemType Directory -Force $clean | Out-Null
Copy-Item $global (Join-Path $clean 'NANOSAVE_0000') -Recurse
$slot = Join-Path $clean ('NANOSAVE_{0:D4}' -f $TestSlot)
Copy-Item $source $slot -Recurse
# The Gallery's file belongs to a mod this copy does not carry.
Remove-Item (Join-Path $slot 'GalleryModSave.txt') -ErrorAction SilentlyContinue

# Its name in the Load window. Written in place: ConvertTo-Json would escape the tag's brackets.
$meta = Join-Path $slot 'game_data.meta'
$raw = [IO.File]::ReadAllText($meta)
$titled = [Text.RegularExpressions.Regex]::Replace($raw, '"m_Title":"[^"]*"', '"m_Title":"' + $TestSaveTitle + '"')
if ($titled -eq $raw) { throw "No m_Title in $meta - the game's save format has changed." }
[IO.File]::WriteAllText($meta, $titled, (New-Object Text.UTF8Encoding($false)))

$after = @(Hashes $global) + @(Hashes $source)
if (Compare-Object $before $after) { throw "The author's save changed while it was copied - stop and tell the author." }
"clean test saves kept from the author's slots 0 and $From ($($before.Count) files, unchanged): $clean"
RestoreTestSave
