FROM mcr.microsoft.com/dotnet/core/sdk:3.1.101 AS builder

# Optimize docker cache, do not make it one layer
RUN apt-get update
RUN apt-get install -y --no-install-recommends imagemagick
###

RUN apt-get install -y --no-install-recommends nsis unzip wine xxd osslsigncode openssl
RUN wget -qO "/tmp/hwi.zip" https://github.com/Groestlcoin/HWI/releases/download/2.0.1/hwi-2.0.1-windows-amd64.zip && \
    unzip "/tmp/hwi.zip" -d "/tmp" && \
    echo "9f7ee46873f34afb19f20e88e27add4a1a1fa30796beded7dbbee9aaad61fb29 /tmp/hwi.exe" | sha256sum -c - && \
    rm "/tmp/hwi.zip" && \
    # Need to setup with rcedit because https://github.com/dotnet/sdk/issues/3943
    # I prebuild the binaries with VS 2019 on commit b807b34a644c86c0b0d89c7f073967e79202731a
    wget -qO "/tmp/rcedit.exe" https://aois.blob.core.windows.net/public/rcedit.exe && \
    echo "4fd0b81071912fb2534770e7ad6688f6ee1dd227f62748b7fc3272c77337d6a6 /tmp/rcedit.exe" | sha256sum -c -

WORKDIR /source
ENV RUNTIME "win-x64"
COPY "Build/common" "Build/common"
ENV EXPORT_VARIABLES "source Build/common/export-variables.sh"
COPY BTCPayServer.Vault/BTCPayServer.Vault.csproj BTCPayServer.Vault/BTCPayServer.Vault.csproj
COPY BTCPayServer.Hwi/BTCPayServer.Hwi.csproj BTCPayServer.Hwi/BTCPayServer.Hwi.csproj

SHELL ["/bin/bash", "-c"]
RUN $EXPORT_VARIABLES && dotnet_restore
COPY BTCPayServer.Hwi BTCPayServer.Hwi
COPY BTCPayServer.Vault BTCPayServer.Vault

RUN $EXPORT_VARIABLES && \
    dotnet_publish && mv /tmp/hwi.exe "$PUBLISH_FOLDER/"

COPY BTCPayServerVault.png BTCPayServerVault.png
COPY "Build/${RUNTIME}" "Build/${RUNTIME}"
RUN $EXPORT_VARIABLES && \
    mkdir -p "/tmp/BTCPayServerVault.ico.tmp" && \
    for size in 256x256 48x48 32x32 16x16; do \
        convert -background none -resize "!$size" "BTCPayServerVault.png" "PNG32:/tmp/BTCPayServerVault.ico.tmp/BTCPayServerVault-$size.png"; \
    done && \
    convert /tmp/BTCPayServerVault.ico.tmp/*.png /tmp/BTCPayServerVault.ico && \
    executable="$PUBLISH_FOLDER/$EXECUTABLE.exe" && \
    # Workaround https://github.com/dotnet/sdk/issues/3990
    e_lfanew_loc=$(xxd -p -l1 -s $((16#3C)) "$executable") && \
    e_lfanew_loc="$((16#$e_lfanew_loc))" && \
    echo "e_lfanew=$(printf "%08x" 0x$e_lfanew_loc)" && \
    subsystem_loc=$(($e_lfanew_loc + 92)) && \
    echo "subsystem_loc location is 0x$(printf "%08x" $subsystem_loc)" && \
    printf "%06x: 0200" $subsystem_loc | xxd -r - "$executable" && \
    # End of workaround
    wine /tmp/rcedit.exe "$executable" \
    --set-icon "/tmp/BTCPayServerVault.ico" \
    --set-version-string "LegalCopyright" "$LICENSE" \
    --set-version-string "CompanyName" "$COMPANY" \
    --set-version-string "FileDescription" "$DESCRIPTION" \
    --set-version-string "ProductName" "$TITLE" \
    --set-file-version "$VERSION" \
    --set-product-version "$VERSION" && \
    makensis \
    "-DICON=/tmp/BTCPayServerVault.ico" \
    "-DICONNAME=BTCPayServerVault.ico" \
    "-DPRODUCT_VERSION=$VERSION" \
    "-DPRODUCT_NAME=$TITLE" \
    "-DPRODUCT_PUBLISHER=$COMPANY" \
    "-DPRODUCT_DESCRIPTION=$DESCRIPTION" \
    "-DDIST=$DIST" \
    "-DEXECUTABLE=$EXECUTABLE" \
    "-DPUBLISH_FOLDER=$PUBLISH_FOLDER" \
    "-DRESOURCES=${RESOURCES}" \
    "$RESOURCES/vault.nsis"

ENTRYPOINT [ "/bin/bash", "-c", "$EXPORT_VARIABLES && cp $DIST/* /opt/dist/" ]
