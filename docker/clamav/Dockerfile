FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY SecureFilePipeline.ClamAvService/SecureFilePipeline.ClamAvService.csproj ./SecureFilePipeline.ClamAvService/
RUN dotnet restore ./SecureFilePipeline.ClamAvService/SecureFilePipeline.ClamAvService.csproj
COPY . .
RUN dotnet publish ./SecureFilePipeline.ClamAvService/SecureFilePipeline.ClamAvService.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SecureFilePipeline.ClamAvService.dll"]