# ---- 1) runtime image ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# ---- 2) build image ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# copy your Protos folder exactly where the .csproj expects it
COPY Protos/                    Protos/

# copy only the project file first (to leverage layer caching)
COPY central_server/Api/Api.csproj  central_server/Api/

# restore via the original path inside the container
RUN dotnet restore central_server/Api/Api.csproj

# now bring in the rest of your API code
COPY central_server/Api/.       central_server/Api/

# build using the full path
RUN dotnet build central_server/Api/Api.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/build


FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish central_server/Api/Api.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ---- 4) final image ----
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
