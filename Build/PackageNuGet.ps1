param($scriptRoot)

$ErrorActionPreference = "Stop"

Push-Location "$scriptRoot\..\src\Configy"
& dotnet pack "Configy.csproj" -c "Release" -o "$scriptRoot\packages"
Pop-Location