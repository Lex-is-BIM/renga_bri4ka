REM Step 1: Autobuild solution in Release-mode
set VSVER=[17.0^,18.0^)

::Edit path if VS 2022 is installed on other path
call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat" x64

rmdir /s /q RengaBri4ka

::Build VinnyLibConverter
devenv RengaBri4ka.sln /Build "Release|Any CPU"

::For net48
xcopy .\README.md "bin\Release\net48" /Y /I
xcopy .\UPDATES.md "bin\Release\net48" /Y /I
xcopy .\LICENSE "bin\Release\net48" /Y /I
xcopy .\docs\Bri4kaGuide.pdf "bin\Release\net48" /Y /I

::For net8.0-windows
xcopy .\README.md "bin\Release\net8.0-windows" /Y /I
xcopy .\UPDATES.md "bin\Release\net8.0-windows" /Y /I
xcopy .\LICENSE "bin\Release\net8.0-windows" /Y /I
xcopy .\docs\Bri4kaGuide.pdf "bin\Release\net8.0-windows" /Y /I

xcopy bin\Release\*.* RengaBri4ka /Y /I /E



::ZIP release
del "RengaBri4ka.zip"
"C:\Program Files\7-Zip\7z" a -tzip "RengaBri4ka.zip" RengaBri4ka
rmdir /s /q RengaBri4ka

pause
::@endlocal
::@exit /B 1
