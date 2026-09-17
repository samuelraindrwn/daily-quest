[CmdletBinding()]
param(
    [string]$Version,
    [string]$PublishedExe,
    [string]$ExpectedPublishedExeSha256,
    [string]$OutputRoot,
    [string]$IsccPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$projectPath = Join-Path $repoRoot 'DailyQuest.csproj'
$installerScript = Join-Path $PSScriptRoot 'DailyQuest.iss'

function Resolve-RepoPath {
    param([Parameter(Mandatory)][string]$Path)

    if ([IO.Path]::IsPathRooted($Path)) {
        return [IO.Path]::GetFullPath($Path)
    }

    return [IO.Path]::GetFullPath((Join-Path $repoRoot $Path))
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    [xml]$project = Get-Content -LiteralPath $projectPath -Raw
    $versionNode = $project.SelectSingleNode('/Project/PropertyGroup/Version')
    if ($null -eq $versionNode) {
        throw 'DailyQuest.csproj does not define a Version property.'
    }

    $Version = $versionNode.InnerText.Trim()
}

if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    throw "Installer versions must use numeric major.minor.patch format. Received: $Version"
}

if (![string]::IsNullOrWhiteSpace($ExpectedPublishedExeSha256) -and
    $ExpectedPublishedExeSha256 -notmatch '^[0-9A-Fa-f]{64}$') {
    throw '-ExpectedPublishedExeSha256 must contain exactly 64 hexadecimal characters.'
}

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot "artifacts\installer\v$Version"
}
else {
    $OutputRoot = Resolve-RepoPath $OutputRoot
}

if (Test-Path -LiteralPath $OutputRoot) {
    throw "Output already exists. Choose a fresh -OutputRoot: $OutputRoot"
}

if ([string]::IsNullOrWhiteSpace($IsccPath)) {
    $isccCommand = Get-Command 'ISCC.exe' -ErrorAction SilentlyContinue
    $isccCandidates = @(
        if ($null -ne $isccCommand) { $isccCommand.Source }
        (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe')
        (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe')
        (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe')
    ) | Where-Object { ![string]::IsNullOrWhiteSpace($_) }

    $IsccPath = $isccCandidates |
        Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } |
        Select-Object -First 1
}
else {
    $IsccPath = Resolve-RepoPath $IsccPath
}

if ([string]::IsNullOrWhiteSpace($IsccPath) -or !(Test-Path -LiteralPath $IsccPath -PathType Leaf)) {
    throw 'Inno Setup Compiler was not found. Install it with: winget install --id JRSoftware.InnoSetup --exact --scope user'
}

$isccVersionText = (Get-Item -LiteralPath $IsccPath).VersionInfo.FileVersion.Trim()
$parsedIsccVersion = $null
if ([version]::TryParse($isccVersionText, [ref]$parsedIsccVersion) -and
    $parsedIsccVersion.Major -ne 0 -and
    $parsedIsccVersion -lt [version]'6.3') {
    throw "Inno Setup 6.3 or newer is required. Found: $isccVersionText"
}

if ([string]::IsNullOrWhiteSpace($PublishedExe)) {
    $ringtonePath = Join-Path $repoRoot 'Assets\ringtone\mixkit-facility-alarm-sound-999.wav'
    if (!(Test-Path -LiteralPath $ringtonePath -PathType Leaf)) {
        throw 'The official ringtone is missing. Add the WAV described in Assets\ringtone\README.md, or pass -PublishedExe with an official release executable.'
    }

    $previousRingtoneRequirement = $env:DAILYQUEST_REQUIRE_RINGTONE
    $env:DAILYQUEST_REQUIRE_RINGTONE = '1'
    try {
        & dotnet run --project (Join-Path $repoRoot 'tests\DailyQuest.LogicTests\DailyQuest.LogicTests.csproj') -c Release
        if ($LASTEXITCODE -ne 0) {
            throw "Logic tests failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        if ($null -eq $previousRingtoneRequirement) {
            Remove-Item Env:DAILYQUEST_REQUIRE_RINGTONE -ErrorAction SilentlyContinue
        }
        else {
            $env:DAILYQUEST_REQUIRE_RINGTONE = $previousRingtoneRequirement
        }
    }

    $sourceExe = $null
}
else {
    $sourceExe = Resolve-RepoPath $PublishedExe
    if (!(Test-Path -LiteralPath $sourceExe -PathType Leaf)) {
        throw "Published executable was not found: $sourceExe"
    }
}

$outputParent = Split-Path -Parent $OutputRoot
New-Item -ItemType Directory -Path $outputParent -Force | Out-Null
$stagingRoot = Join-Path $outputParent ('.' + [IO.Path]::GetFileName($OutputRoot) + '.staging-' + [guid]::NewGuid().ToString('N'))
$publishDir = Join-Path $stagingRoot 'publish'
$setupDir = Join-Path $stagingRoot 'output'
New-Item -ItemType Directory -Path $publishDir, $setupDir | Out-Null

try {
    if ($null -eq $sourceExe) {
        $publishArguments = @(
            'publish', $projectPath,
            '-c', 'Release',
            '-r', 'win-x64',
            '--self-contained', 'true',
            '-p:PublishSingleFile=true',
            '-p:IncludeNativeLibrariesForSelfExtract=true',
            '-p:EnableCompressionInSingleFile=true',
            '-o', $publishDir
        )
        & dotnet @publishArguments
        if ($LASTEXITCODE -ne 0) {
            throw "Publish failed with exit code $LASTEXITCODE."
        }

        $sourceExe = Join-Path $publishDir 'DailyQuest.exe'
    }

    if (!(Test-Path -LiteralPath $sourceExe -PathType Leaf)) {
        throw "Published executable was not found: $sourceExe"
    }

    $sourceVersion = (Get-Item -LiteralPath $sourceExe).VersionInfo
    if ($sourceVersion.FileVersion -ne "$Version.0" -or $sourceVersion.ProductVersion -notlike "$Version*") {
        throw "Executable version does not match v$Version. Found FileVersion=$($sourceVersion.FileVersion), ProductVersion=$($sourceVersion.ProductVersion)."
    }

    if (![string]::IsNullOrWhiteSpace($ExpectedPublishedExeSha256)) {
        $sourceHash = (Get-FileHash -LiteralPath $sourceExe -Algorithm SHA256).Hash
        if ($sourceHash -ne $ExpectedPublishedExeSha256.ToUpperInvariant()) {
            throw "Published executable SHA-256 mismatch. Expected $($ExpectedPublishedExeSha256.ToUpperInvariant()), found $sourceHash."
        }
    }

    $setupBaseName = "DailyQuest-v$Version-win-x64-setup"
    $compilerArguments = @(
        '/Qp',
        "/DAppVersion=$Version",
        "/DSourceExe=$sourceExe",
        "/O$setupDir",
        "/F$setupBaseName",
        $installerScript
    )
    & $IsccPath @compilerArguments
    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup compilation failed with exit code $LASTEXITCODE."
    }

    $setupPath = Join-Path $setupDir "$setupBaseName.exe"
    if (!(Test-Path -LiteralPath $setupPath -PathType Leaf)) {
        throw "Installer output was not created: $setupPath"
    }

    $setupVersion = (Get-Item -LiteralPath $setupPath).VersionInfo
    $setupFileVersion = $setupVersion.FileVersion.Trim()
    $setupProductVersion = $setupVersion.ProductVersion.Trim()
    $acceptedInstallerVersions = @($Version, "$Version.0")
    if ($setupFileVersion -notin $acceptedInstallerVersions -or $setupProductVersion -notin $acceptedInstallerVersions) {
        throw "Installer metadata does not match v$Version. Found FileVersion=$($setupVersion.FileVersion), ProductVersion=$($setupVersion.ProductVersion)."
    }

    $setupHash = Get-FileHash -LiteralPath $setupPath -Algorithm SHA256
    $checksumPath = Join-Path $setupDir 'SHA256SUMS-installer.txt'
    $checksumLine = "$($setupHash.Hash)  $([IO.Path]::GetFileName($setupPath))`n"
    [IO.File]::WriteAllText($checksumPath, $checksumLine, [Text.UTF8Encoding]::new($false))

    $signatureStatus = (Get-AuthenticodeSignature -LiteralPath $setupPath).Status
    $setupSize = (Get-Item -LiteralPath $setupPath).Length

    Move-Item -LiteralPath $stagingRoot -Destination $OutputRoot
    $setupPath = Join-Path $OutputRoot "output\$setupBaseName.exe"
    $checksumPath = Join-Path $OutputRoot 'output\SHA256SUMS-installer.txt'

    [pscustomobject]@{
        Installer = $setupPath
        Checksum = $checksumPath
        SHA256 = $setupHash.Hash
        SizeBytes = $setupSize
        FileVersion = $setupFileVersion
        SignatureStatus = $signatureStatus
    }
}
catch {
    Write-Warning "Installer build failed. Staging files were left at: $stagingRoot"
    throw
}
