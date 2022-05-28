#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["URLShortenerService/URLShortenerService.csproj", "URLShortenerService/"]
RUN dotnet restore "URLShortenerService/URLShortenerService.csproj"
COPY . .
WORKDIR "/src/URLShortenerService"
RUN dotnet build "URLShortenerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "URLShortenerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "URLShortenerService.dll"]