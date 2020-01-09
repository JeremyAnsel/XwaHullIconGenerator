@echo off
setlocal

cd "%~dp0"

For %%a in (
"XwaHullIconGenerator\bin\Release\net45\*.dll"
"XwaHullIconGenerator\bin\Release\net45\*.exe"
"XwaHullIconGenerator\bin\Release\net45\*.config"
"XwaHullIconGenerator\bin\Release\net45\*.cso"
) do (
xcopy /s /d "%%~a" dist\
)
