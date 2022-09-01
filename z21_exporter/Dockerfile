#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0.8-bullseye-slim-arm64v8 AS base
WORKDIR /app
EXPOSE 9101

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim-arm64v8 AS build
WORKDIR /src
COPY ["z21_exporter.csproj", "."]
RUN dotnet restore "./z21_exporter.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "z21_exporter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "z21_exporter.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "z21_exporter.dll"]