FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SharedKernel/SharedKernel/SharedKernel.csproj", "src/SharedKernel/SharedKernel/"]
COPY ["src/Identity/Identity.Api/Identity.Api.csproj", "src/Identity/Identity.Api/"]
COPY ["src/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj", "src/Identity/Identity.Infrastructure/"]
COPY ["src/Identity/Identity.Application/Identity.Application.csproj", "src/Identity/Identity.Application/"]
COPY ["src/Identity/Identity.Domain/Identity.Domain.csproj", "src/Identity/Identity.Domain/"]
RUN dotnet restore "./src/Identity/Identity.Api/Identity.Api.csproj"
COPY . .
WORKDIR "/src/src/Identity/Identity.Api"
RUN dotnet build "./Identity.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Identity.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.Api.dll"]