FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Включаем root-доступ только для установки
USER 0

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    curl \
    ca-certificates \
    gnupg && \
    rm -rf /var/lib/apt/lists/*

# 2. Добавляем репозиторий NodeSource для Node.js 22.x
RUN curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg && \
    echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_22.x nodistro main" > /etc/apt/sources.list.d/nodesource.list

# 3. Устанавливаем Node.js
RUN apt-get update && \
    apt-get install -y nodejs && \
    npm install -g npm@latest && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app/OsmToGeoJson

# Устанавливаем npm-пакеты в текущую папку
RUN npm init -y && \
    npm install mapshaper osmtogeojson

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WebAPI/WebAPI.csproj", "WebAPI/"]
RUN dotnet restore "WebAPI/WebAPI.csproj"
COPY . .
WORKDIR "/src/WebAPI"
RUN dotnet build "WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final

USER 0
# Копируем Node.js и npm-пакеты из node-base
COPY --from=base /usr/bin/node /usr/bin/node
COPY --from=base /usr/lib/node_modules /usr/lib/node_modules
COPY --from=base /app/OsmToGeoJson/node_modules /app/OsmToGeoJson/node_modules
RUN ln -s /usr/lib/node_modules/npm/bin/npm-cli.js /usr/bin/npm && \
    ln -s /usr/lib/node_modules/osmtogeojson/bin/osmtogeojson /usr/bin/osmtogeojson && \
    ln -s /usr/lib/node_modules/mapshaper/bin/mapshaper /usr/bin/mapshaper \
    
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "WebAPI.dll"]
