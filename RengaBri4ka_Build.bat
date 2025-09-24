REM Step 1: Autobuild solution in Release-mode
set VSVER=[17.0^,18.0^)

::Edit path if VS 2022 is installed on other path
call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat" 

rmdir /s /q RengaBri4ka

::Build VinnyLibConverter
devenv RengaBri4ka.sln /Build "Release|Any CPU"
xcopy bin\Release\*.* RengaBri4ka /Y /I /E
xcopy .\README.md "bin\Release" /Y /I
xcopy .\UPDATES.md "bin\Release" /Y /I
xcopy .\LICENSE "bin\Release" /Y /I
xcopy .\docs\Bri4kaGuide.pdf "bin\Release" /Y /I

::ZIP release
"C:\Program Files\7-Zip\7z" a -tzip "RengaBri4ka.zip" RengaBri4ka
rmdir /s /q RengaBri4ka

::@endlocal
::@exit /B 1
