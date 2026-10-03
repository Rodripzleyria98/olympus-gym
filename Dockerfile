FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["backend/Olympus.Api/Olympus.Api.csproj", "backend/Olympus.Api/"]
RUN dotnet restore "backend/Olympus.Api/Olympus.Api.csproj"

COPY backend/ backend/
WORKDIR "/src/backend/Olympus.Api"
RUN dotnet publish "Olympus.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Olympus.Api.dll"]