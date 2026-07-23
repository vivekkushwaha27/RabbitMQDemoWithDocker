FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app

EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["ProducerApi/ProducerApi.csproj", "ProducerApi/"]
COPY ["Shared/Shared.csproj", "Shared/"]

RUN dotnet restore "ProducerApi/ProducerApi.csproj"

COPY . .

WORKDIR "/src/ProducerApi"

RUN dotnet publish "ProducerApi.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


FROM base AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ProducerApi.dll"]