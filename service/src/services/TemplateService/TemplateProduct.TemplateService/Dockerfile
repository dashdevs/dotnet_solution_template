#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /
COPY ["src/services/TemplateService/TemplateProduct.TemplateService/TemplateProduct.TemplateService.csproj", "src/services/TemplateService/TemplateProduct.TemplateService/"]
RUN dotnet restore "src/services/TemplateService/TemplateProduct.TemplateService/TemplateProduct.TemplateService.csproj"
COPY . .
WORKDIR "/src/services/TemplateService/TemplateProduct.TemplateService"
RUN dotnet build "TemplateProduct.TemplateService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TemplateProduct.TemplateService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TemplateCompany.TemplateProduct.TemplateService.dll"]
