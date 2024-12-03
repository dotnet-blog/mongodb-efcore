# source build
# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY ./src .
RUN dotnet restore ./Samples.MongoDb.EFCore.Api/Samples.MongoDb.EFCore.Api.csproj --disable-parallel
RUN dotnet publish ./Samples.MongoDb.EFCore.Api/Samples.MongoDb.EFCore.Api.csproj -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 5000
ENTRYPOINT ["dotnet", "Samples.MongoDb.EFCore.Api.dll"]