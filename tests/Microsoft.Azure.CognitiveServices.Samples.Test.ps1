$projects = (Get-ChildItem '..\samples' -Filter '*.csproj' -Recurse)

foreach ($project in $projects)
{
	Write-Host "$project" -ForegroundColor Yellow
	dotnet publish -f netstandard1.4 $project.FullName
	Write-Host
}


Write-Host "Microsoft.Azure.CognitiveServices.Samples.Test.csproj" -ForegroundColor Yellow
dotnet test '.\\Microsoft.Azure.CognitiveServices.Samples.Test\\Microsoft.Azure.CognitiveServices.Samples.Test.csproj'