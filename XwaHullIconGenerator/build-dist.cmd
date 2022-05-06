@echo off
setlocal

cd "%~dp0"

For %%a in (
"XwaHullIconGenerator\bin\Release\net48\*.dll"
"XwaHullIconGenerator\bin\Release\net48\*.exe"
"XwaHullIconGenerator\bin\Release\net48\*.config"
"XwaHullIconGenerator\bin\Release\net48\*.cso"
) do (
xcopy /s /d "%%~a" dist\
)
