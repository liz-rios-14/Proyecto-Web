@echo off
setlocal
set "DOCKER=%LocalAppData%\Programs\DockerDesktop\resources\bin\docker.exe"
if not exist "%DOCKER%" set "DOCKER=docker"

pushd "%~dp0.."
"%DOCKER%" compose --env-file docker\.env -f docker\compose.yml ps
popd
