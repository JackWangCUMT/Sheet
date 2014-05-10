@echo off
set PATH=%WINDIR%\Microsoft.NET\Framework\v4.0.30319;%PATH%

MSBuild Sheet.sln /m /t:Build /p:Configuration=Release;TargetFrameworkVersion=v4.5.1

pause
