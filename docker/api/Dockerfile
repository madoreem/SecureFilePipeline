FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY SecureFilePipeline.Api/SecureFilePipeline.Api.csproj ./SecureFilePipeline.Api/
RUN dotnet restore ./SecureFilePipeline.Api/SecureFilePipeline.Api.csproj
COPY . .
RUN dotnet publish ./SecureFilePipeline.Api/SecureFilePipeline.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SecureFilePipeline.Api.dll"]