@echo off
REM Set the path to csc.exe as a variable
set CSC_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

REM Compile jam.cs using the variable
%CSC_PATH% jam.cs

REM Check if compilation was successful
if %errorlevel% equ 0 (
    echo Compilation successful!
) else (
    echo Compilation failed!
)

pause
