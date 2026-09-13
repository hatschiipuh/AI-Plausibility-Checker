FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["BusinessService.Api/BusinessService.Api.csproj", "BusinessService.Api/"]
COPY ["BusinessService.Domain/BusinessService.Domain.csproj", "BusinessService.Domain/"]
COPY ["BusinessService.Infrastructure/BusinessService.Infrastructure.csproj", "BusinessService.Infrastructure/"]

RUN dotnet restore "BusinessService.Api/BusinessService.Api.csproj"

COPY . .

WORKDIR "/src/BusinessService.Api"
RUN dotnet publish "BusinessService.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BusinessService.Api.dll"]