FOR /R ".\src\services" %%i IN (*.csproj) DO dotnet sln add "%%i"