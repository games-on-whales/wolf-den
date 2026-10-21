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

RUN <<_INSTALL_RUST_AND_REQ
set -e
apt-get install -y build-essential git npm clang cmake
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh -s -- -y
rm -rf /var/lib/apt/lists/*
_INSTALL_RUST_AND_REQ

ENV PATH="/root/.cargo/bin:${PATH}"

RUN <<_INSTALL_MOONLIGHT_WEB
set -e

git clone https://github.com/MrCreativ3001/moonlight-web-stream.git
cd moonlight-web-stream
git checkout v2.10.0
cargo build --release
cargo install wasm-bindgen-cli
npm install
npm run build
_INSTALL_MOONLIGHT_WEB

RUN <<_CLEANUP
set -e
cd moonlight-web-stream
mkdir moonlight-web
mv target/release/web-server moonlight-web/web-server
mv target/release/streamer moonlight-web/streamer
mv dist moonlight-web/static
mv moonlight-web ../moonlight-web
cd ..
rm -rf moonlight-web-stream
_CLEANUP

#USER $APP_UID

WORKDIR /app
COPY --from=publish /app/publish .
COPY --chmod=755 WolfLeash/DockerOverlay /
ENTRYPOINT ["/entrypoint.sh"]
