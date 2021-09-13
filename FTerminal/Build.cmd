@echo off
MSBuild FTerminal.csproj /t:Rebuild /p:Configuration=Debug
MSBuild FTerminal.csproj /t:Rebuild /p:Configuration=Release