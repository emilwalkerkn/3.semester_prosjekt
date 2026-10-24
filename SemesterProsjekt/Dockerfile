FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SemesterProsjekt.csproj", "./"]
RUN dotnet restore "SemesterProsjekt.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "./SemesterProsjekt.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SemesterProsjekt.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Docker kopierer dette kataloginnholdet til et nytt named volume. Da kan
# app-brukeren lagre ASP.NET Core-sikkerhetsnøkler også etter en omstart.
USER root
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys && chown -R $APP_UID:$APP_UID /home/app/.aspnet
USER $APP_UID
ENTRYPOINT ["dotnet", "SemesterProsjekt.dll"]
