FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["ValidationScoringService.Api/ValidationScoringService.Api.csproj", "ValidationScoringService.Api/"]
RUN dotnet restore "ValidationScoringService.Api/ValidationScoringService.Api.csproj"

COPY . .

WORKDIR "/src/ValidationScoringService.Api"
RUN dotnet publish "ValidationScoringService.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ValidationScoringService.Api.dll"]