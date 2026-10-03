FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Olympus.Api/Olympus.Api.csproj", "Olympus.Api/"]
RUN dotnet restore "Olympus.Api/Olympus.Api.csproj"
COPY . .
WORKDIR "/src/Olympus.Api"
RUN dotnet publish "Olympus.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Olympus.Api.dll"]