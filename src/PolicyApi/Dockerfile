FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY PolicyApi.slnx ./
COPY src/PolicyApi/PolicyApi.csproj src/PolicyApi/
RUN dotnet restore src/PolicyApi/PolicyApi.csproj
COPY src/PolicyApi/ src/PolicyApi/
RUN dotnet build src/PolicyApi/PolicyApi.csproj --configuration Release --no-restore --output /app/build

FROM build AS publish
RUN dotnet publish src/PolicyApi/PolicyApi.csproj --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PolicyApi.dll"]
