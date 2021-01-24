## CMD

```
dotnet new tool-manifest

dotnet pack src/RestClient -o __publish__
dotnet tool install wk.RestClient --add-source __publish__

dotnet wk-rest --file http/users.rest --header

dotnet run --project src/RestClient/RestClient.csproj --file http/users.rest
dotnet run --project src/RestClient/RestClient.csproj --file http/register.rest
```