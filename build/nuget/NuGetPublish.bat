cd /d %~dp0
SET OUTPUT=packages
SET OPTIONS=-OutputDirectory %OUTPUT% -IncludeReferencedProjects -Build -Properties Configuration=Release
del /q %OUTPUT%
mkdir %OUTPUT%
nuget pack ..\..\src\Shipwreck.Net.Http.Formatting\Shipwreck.Net.Http.Formatting.csproj %OPTIONS%
nuget push %OUTPUT%\*.nupkg