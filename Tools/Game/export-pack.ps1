# Brings the repo's SMSAndroidsPack up to date from the folder the author edits it in, then exports it with
# ModForge's own exporter (Tools/PackTool: what the editor's Export writes, plus its issues list) into the test
# copy's Mods folder. Run before every check: a check of an old pack proves nothing.
#
#   powershell -File Tools\Game\export-pack.ps1            # sync when the working folder is newer, then export
#   powershell -File Tools\Game\export-pack.ps1 -NoSync    # export the repo's pack as it is
#
# The sync only adds and replaces files (robocopy /E, never /MIR), and skips what sync-exclude.txt names: files
# the author decided are not part of the pack but still keeps in the working folder.
param([switch]$NoSync)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'test-env.ps1')
New-Item -ItemType Directory -Force $Scratch | Out-Null

if (-not $NoSync -and (Test-Path (Join-Path $WorkingPack 'modpack.json'))) {
    $exclude = @(Get-Content (Join-Path $PSScriptRoot 'sync-exclude.txt') | Where-Object { $_ -and -not $_.StartsWith('#') })
    $rc = @($WorkingPack, $RepoPack, '/E', '/NDL', '/NJH', '/NJS', '/NP', '/NS', '/NC')
    if ($exclude.Count) { $rc += "/XF"; $rc += $exclude }
    $copied = @(robocopy @rc | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    if ($LASTEXITCODE -ge 8) { throw "sync failed" }
    if ($copied.Count) { "synced $($copied.Count) newer files from the working folder:"; $copied | Select-Object -First 30 | ForEach-Object { "  $_" } }
    else { "the repo's pack already matches the working folder" }
    # What only the repo has: the author removed it from the working folder, or it was never there.
    # robocopy's listing names them "New File" (files only the destination has are "*EXTRA File").
    $only = @(robocopy $RepoPack $WorkingPack /E /L /NDL /NJH /NJS /NP /NS /XO /XN /XC | ForEach-Object { $_.Trim() } |
              Where-Object { $_ -like 'New File*' } | ForEach-Object { ($_ -replace '^New File\s+', '') })
    if ($only.Count) { "in the repo's pack but not the working folder (left as they are): $($only.Count)"; $only | Select-Object -First 10 | ForEach-Object { "  $_" } }
}

$tool = Join-Path $Scratch 'packtool\bin\PackTool\release\PackTool.exe'
dotnet build "$Androids\Tools\PackTool\PackTool.csproj" -c Release --artifacts-path (Join-Path $Scratch 'packtool') -v q -nologo | Select-String ' error '
if ($LASTEXITCODE -ne 0) { throw "PackTool did not build." }

New-Item -ItemType Directory -Force "$Test\Mods" | Out-Null
& $tool export $RepoPack "$Test\Mods\SMSAndroidsPack.smspack" (Join-Path $Scratch 'issues.tsv')
if ($LASTEXITCODE -ne 0) { throw "The export failed." }
"the editor's issues list: $(Join-Path $Scratch 'issues.tsv')"
