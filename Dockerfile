FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything and restore/publish the ApiCore project
COPY . .
RUN dotnet restore ApiCore/ApiCore.csproj
RUN dotnet publish ApiCore/ApiCore.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Allow Railway to set the port via the PORT env var
ENV ASPNETCORE_URLS=http://+:${PORT:-80}
EXPOSE 80

ENTRYPOINT ["dotnet", "ApiCore.dll"]
