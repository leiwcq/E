@echo off
for /R %%s in (*.nupkg) do (echo %%s|findstr "Debug" && copy %%s .\NuPkg\ || echo no)
pause 