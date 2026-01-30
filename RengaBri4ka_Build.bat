REM Step 1: Autobuild solution in Release-mode
set VSVER=[17.0^,18.0^)

::Edit path if VS 2022 is installed on other path
call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat" x64

rmdir /s /q RengaBri4ka

::Build VinnyLibConverter
devenv RengaBri4ka.sln /Build "Release|Any CPU"

:: Help-guide is copied in root
xcopy .\docs\Bri4kaGuide.pdf "bin\Release" /Y /I

::For net48
xcopy .\README.md "bin\Release\net48" /Y /I
xcopy .\UPDATES.md "bin\Release\net48" /Y /I
xcopy .\LICENSE "bin\Release\net48" /Y /I

copy src\RengaBri4kaLoader\RengaBri4kaRelease.rndesc bin\Release\net48\RengaBri4ka.rndesc

::For net8.0-windows
xcopy .\README.md "bin\Release\net8.0-windows" /Y /I
xcopy .\UPDATES.md "bin\Release\net8.0-windows" /Y /I
xcopy .\LICENSE "bin\Release\net8.0-windows" /Y /I
copy src\RengaBri4kaLoader\RengaBri4kaReleaseNet8.rndesc bin\Release\net8.0-windows\RengaBri4ka.rndesc

:: For icons
call icons\RengaBri4ka_Icons.bat

xcopy bin\Release\*.* RengaBri4ka /Y /I /E

::ZIP release
del "RengaBri4ka.zip"
"C:\Program Files\7-Zip\7z" a -tzip "RengaBri4ka.zip" RengaBri4ka
rmdir /s /q RengaBri4ka

pause
::@endlocal
::@exit /B 1
