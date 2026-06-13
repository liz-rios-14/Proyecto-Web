@echo off
setlocal
set "DOCKER=%LocalAppData%\Programs\DockerDesktop\resources\bin\docker.exe"
if not exist "%DOCKER%" set "DOCKER=docker"

pushd "%~dp0.."
"%DOCKER%" compose --env-file docker\.env -f docker\compose.yml up --build -d
if errorlevel 1 (
  echo.
  echo No se pudo iniciar SalesPoint. Revise que Docker Desktop este abierto.
  popd
  exit /b 1
)

echo.
echo SalesPoint esta disponible en http://localhost:8080
echo Swagger esta disponible en http://localhost:5037/swagger
popd
