@echo off

REM PARA GERAR UM PACOTE SO COM O DLL E O PDB
nuget pack FTerminal.Release.nuspec

REM PARA GERAR OS DOIS PACOTES SEPARADOS
REM nuget pack FTerminal.nuspec -Symbols -SymbolPackageFormat snupkg
pause