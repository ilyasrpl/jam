@echo off
REM Set the path to csc.exe as a variable
set CSC_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

REM Compile jam.cs as a GUI application (winexe)
%CSC_PATH% /target:winexe /out:jam.exe jam.cs

REM Check if compilation was successful
if %errorlevel% equ 0 (
    echo Compilation successful!
) else (
    echo Compilation failed!
)

pause
