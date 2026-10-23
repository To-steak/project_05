@echo off
rem Double-click to deploy Project\Builds\WebGL to https://to-steak.github.io/project05/ (see deploy-pages.ps1)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0deploy-pages.ps1" %*
pause
