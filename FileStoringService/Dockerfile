FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FileStoringService.csproj", "./"]
RUN dotnet restore "FileStoringService.csproj"
COPY . .
RUN dotnet build "FileStoringService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileStoringService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/uploads
ENTRYPOINT ["dotnet", "FileStoringService.dll"]