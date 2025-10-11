FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY SecureFilePipeline.MetadataService/SecureFilePipeline.MetadataService.csproj ./SecureFilePipeline.MetadataService/
RUN dotnet restore ./SecureFilePipeline.MetadataService/SecureFilePipeline.MetadataService.csproj
COPY . .
RUN dotnet publish ./SecureFilePipeline.MetadataService/SecureFilePipeline.MetadataService.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SecureFilePipeline.MetadataService.dll"]