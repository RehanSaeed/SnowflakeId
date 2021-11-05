#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Source/ID.API/ID.API.csproj", "Source/ID.API/"]
RUN dotnet restore "Source/ID.API/ID.API.csproj"
COPY . .
WORKDIR "/src/Source/ID.API"
RUN dotnet build "ID.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ID.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ID.API.dll"]