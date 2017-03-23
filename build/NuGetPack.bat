cd /d %~dp0
SET OUTPUT=packages
SET OPTIONS=-OutputDirectory %OUTPUT% -IncludeReferencedProjects -Build -Properties Configuration=Release
del /q %OUTPUT%
mkdir %OUTPUT%
nuget pack ..\src\Shipwreck.Querying\Shipwreck.Querying.csproj %OPTIONS%
nuget pack ..\src\Shipwreck.Querying.TypeScript\Shipwreck.Querying.TypeScript.nuspec %OPTIONS%