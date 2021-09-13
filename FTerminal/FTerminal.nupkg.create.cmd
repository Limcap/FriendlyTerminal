@echo off
REM nuget pack FTerminal.nuspec
nuget pack FTerminal.nuspec -Symbols -SymbolPackageFormat snupkg
pause