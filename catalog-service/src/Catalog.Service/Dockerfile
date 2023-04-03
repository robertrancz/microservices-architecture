FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN apt-get -y update; apt-get -y install curl
ARG OTEL_VERSION=0.6.0
ADD https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/download/v${OTEL_VERSION}/otel-dotnet-auto-install.sh otel-dotnet-auto-install.sh
RUN apt-get update && apt-get install -y unzip && \
    OTEL_DOTNET_AUTO_HOME="/otel-dotnet-auto" sh otel-dotnet-auto-install.sh

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Catalog.Service/Catalog.Service.csproj", "Catalog.Service/"]
COPY  Catalog.Service/packages/ Catalog.Service/packages/
RUN dotnet restore "Catalog.Service/Catalog.Service.csproj" -s https://api.nuget.org/v3/index.json -s /src/Catalog.Service/packages/ --verbosity n
COPY . .
WORKDIR "/src/Catalog.Service"
RUN dotnet build "Catalog.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Catalog.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.Service.dll"]