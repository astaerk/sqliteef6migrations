Branches:
- master		-> exactly the original code from https://github.com/bubibubi/sqliteef6migrations
- feature/extensibility	-> the feature branch where the extensibility features are developed.
- mergerequest		-> merge all changes from feature/extensibility to this branch but changes that should not be in the merge request should be merged with strategy "ours"
- release/*		-> branch for each release. make a squash commit from mergerequest to this branch with a meaningful commit message and create a merge request in the original project for this branch




NuGet:
- https://www.nuget.org/packages/SQLite.EF6.Migrations-Extensible/
- replacement tokens in nuspec: https://docs.microsoft.com/de-de/nuget/reference/nuspec#replacement-tokens
- install with powershell: Install-Package NuGet.CommandLine
- command in directory of csproj: nuget pack System.Data.SQLite.EF6.Migrations.csproj -Prop Configuration=Release