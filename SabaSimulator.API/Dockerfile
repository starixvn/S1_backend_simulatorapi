#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.


FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["SabaSimulator.API/SabaSimulator.API.csproj", "SabaSimulator.API/"]
RUN dotnet restore "./SabaSimulator.API/./SabaSimulator.API.csproj"
COPY . .
WORKDIR "/src/SabaSimulator.API"
RUN dotnet build "./SabaSimulator.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SabaSimulator.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM hub.lumigame.com/base/dotnet/core/aspnet:8.0-alpine-24h1.1 AS base
WORKDIR /app
COPY --from=publish /app/publish .


ENTRYPOINT ["dotnet", "SabaSimulator.API.dll"]
EXPOSE 5000