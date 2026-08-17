FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WolfLeash/WolfLeash.csproj", "WolfLeash/"]
RUN dotnet restore "WolfLeash/WolfLeash.csproj"
COPY . .
WORKDIR "/src/WolfLeash"
RUN dotnet build "./WolfLeash.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WolfLeash.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final

USER root

ARG GOSU_VERSION=1.14

RUN <<_INSTALL_PACKAGES
set -e

echo "**** Update apt database ****"
apt-get update

echo "**** Install certificates ****"
apt-get install -y --reinstall --no-install-recommends \
    ca-certificates

echo "**** Install base packages ****"
apt-get install -y --no-install-recommends \
    fuse \
    libnss3 \
    wget \
    curl \
    jq \
    socat \
    netcat-openbsd

echo "**** Install gosu ****"
wget --progress=dot:giga \
    -O /usr/bin/gosu \
    "https://github.com/tianon/gosu/releases/download/${GOSU_VERSION}/gosu-amd64"
chmod +x /usr/bin/gosu

echo "**** Verify gosu works ****"
gosu nobody true && echo "Working!"

_INSTALL_PACKAGES

#USER $APP_UID

WORKDIR /app
COPY --from=publish /app/publish .
COPY --chmod=755 WolfLeash/DockerOverlay /
ENTRYPOINT ["/entrypoint.sh"]
