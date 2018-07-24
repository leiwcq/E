@echo off
for /R %%s in (*.nupkg) do (echo %%s|findstr "Release" && copy %%s .\NuPkg\ || echo no)
pause 