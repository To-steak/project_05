<#
.SYNOPSIS
    Unity WebGL 빌드를 개인 사이트(To-steak.github.io, VitePress)에 올린다.

.DESCRIPTION
    1. 빌드 폴더(index.html, Build/)를 확인한다.
    2. 사이트 저장소가 없으면 clone, 있으면 main을 pull한다.
    3. 사이트의 docs/public/<SiteFolder>/ 를 비우고 빌드를 복사한다.
       VitePress는 docs/public/ 을 사이트 루트로 그대로 복사하므로 https://to-steak.github.io/<SiteFolder>/ 로 열린다.
    4. 그 폴더만 커밋하고 push 한다. 사이트 저장소의 GitHub Actions(deploy.yml)가 사이트를 다시 빌드해 배포한다.

    GitHub Pages는 압축 응답 헤더(Content-Encoding)를 설정할 수 없다.
    Player Settings의 Decompression Fallback이 켜져 있어야(지금 켜져 있음) 압축 빌드가 브라우저에서 풀린다.

.EXAMPLE
    Tools\deploy-pages.cmd "D:\Builds\project05\WebGL"
    powershell -ExecutionPolicy Bypass -File Tools\deploy-pages.ps1 "D:\Builds\project05\WebGL"
    powershell -ExecutionPolicy Bypass -File Tools\deploy-pages.ps1 -BuildPath "D:\Builds\project05\WebGL" -NoPush
#>
param(
    # Unity WebGL 빌드 폴더 (index.html이 있는 폴더). 필수, 첫 번째 인자로 받는다
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$BuildPath,
    # 사이트 저장소를 둘 로컬 경로. 없으면 clone 한다. 기본값은 이 저장소 옆 폴더
    [string]$SiteRepoPath = (Join-Path $PSScriptRoot "..\..\To-steak.github.io"),
    # 사이트 안의 게임 폴더 이름 (주소: https://to-steak.github.io/<SiteFolder>/)
    [string]$SiteFolder = "project05",
    # 사이트 저장소 주소
    [string]$SiteRepoUrl = "https://github.com/To-steak/To-steak.github.io.git",
    # 커밋만 하고 push 하지 않는다 (확인용)
    [switch]$NoPush
)

$ErrorActionPreference = "Stop"
# GitHub 파일 크기 제한 (100MB). 넘으면 push가 거절된다
$MaxFileBytes = 100MB

function Invoke-Git {
    param([string]$RepoPath, [string[]]$Arguments)
    & git -C $RepoPath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "git $($Arguments -join ' ') 실패 (종료 코드 $LASTEXITCODE)"
    }
}

# 1. 빌드 확인
$BuildPath = [System.IO.Path]::GetFullPath($BuildPath)
if (-not (Test-Path (Join-Path $BuildPath "index.html")) -or -not (Test-Path (Join-Path $BuildPath "Build"))) {
    throw "빌드 폴더에 index.html과 Build 폴더가 없습니다: $BuildPath`nUnity에서 WebGL로 빌드한 폴더를 -BuildPath로 지정하세요."
}

$largeFiles = Get-ChildItem -Path $BuildPath -Recurse -File | Where-Object { $_.Length -gt $MaxFileBytes }
if ($largeFiles) {
    $names = ($largeFiles | ForEach-Object { "$($_.Name) ($([math]::Round($_.Length / 1MB, 1))MB)" }) -join ", "
    throw "GitHub 파일 크기 제한(100MB)을 넘는 파일이 있습니다: $names`nPlayer Settings에서 압축(Brotli/Gzip)을 켜거나 에셋을 줄이세요."
}

# 2. 사이트 저장소 준비
$SiteRepoPath = [System.IO.Path]::GetFullPath($SiteRepoPath)
if (-not (Test-Path (Join-Path $SiteRepoPath ".git"))) {
    Write-Host "사이트 저장소를 clone 합니다: $SiteRepoPath"
    & git clone $SiteRepoUrl $SiteRepoPath
    if ($LASTEXITCODE -ne 0) { throw "git clone 실패" }
}
else {
    Write-Host "사이트 저장소를 최신으로 맞춥니다: $SiteRepoPath"
    Invoke-Git $SiteRepoPath @("checkout", "main")
    Invoke-Git $SiteRepoPath @("pull", "--ff-only", "origin", "main")
}

# 3. 빌드 복사 (게임 폴더만 통째로 바꾼다)
$targetRelativePath = "docs/public/$SiteFolder"
$targetPath = Join-Path $SiteRepoPath $targetRelativePath
if (Test-Path $targetPath) {
    Remove-Item -Path $targetPath -Recurse -Force
}
New-Item -ItemType Directory -Path $targetPath | Out-Null
Copy-Item -Path (Join-Path $BuildPath "*") -Destination $targetPath -Recurse -Force
Write-Host "빌드를 복사했습니다: $targetRelativePath"

# 4. 커밋과 push (게임 폴더만 스테이징한다. 사이트의 다른 변경은 건드리지 않는다)
Invoke-Git $SiteRepoPath @("add", "--all", "--", $targetRelativePath)
& git -C $SiteRepoPath diff --cached --quiet -- $targetRelativePath
if ($LASTEXITCODE -eq 0) {
    Write-Host "바뀐 파일이 없습니다. 배포를 건너뜁니다."
    exit 0
}

$projectCommit = (& git -C $PSScriptRoot rev-parse --short HEAD).Trim()
Invoke-Git $SiteRepoPath @("commit", "-m", "deploy: $SiteFolder WebGL 빌드 (project_05 $projectCommit)", "--", $targetRelativePath)

if ($NoPush) {
    Write-Host "커밋만 했습니다 (-NoPush). 확인 후 사이트 저장소에서 git push 하세요."
    exit 0
}

Invoke-Git $SiteRepoPath @("push", "origin", "main")
Write-Host ""
Write-Host "push 완료. 사이트 저장소의 Actions 배포가 끝나면(보통 1~2분) 열립니다:"
Write-Host "https://to-steak.github.io/$SiteFolder/"
