FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /App
# Copy everything
COPY . ./

RUN ls
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN ls /App/CryptoAPI
RUN dotnet publish /App/CryptoAPI/CryptoAPI.csproj -c Release -o /App/build


FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /App
COPY --from=build-env /App/build .
ENTRYPOINT ["dotnet", "CryptoAPI.dll"]
