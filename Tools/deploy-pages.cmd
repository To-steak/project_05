@echo off
rem Usage: deploy-pages.cmd "<WebGL build folder>" [-NoPush]
rem        or drag the WebGL build folder onto this file.
rem Deploys the build to https://to-steak.github.io/project05/ (see deploy-pages.ps1)
if "%~1"=="" (
    echo Usage: deploy-pages.cmd "WebGL build folder" [-NoPush]
    echo        or drag the WebGL build folder onto this file.
    pause
    exit /b 1
)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0deploy-pages.ps1" %*
pause
