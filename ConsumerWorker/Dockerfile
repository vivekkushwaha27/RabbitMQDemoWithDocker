FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base

WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["ConsumerWorker/ConsumerWorker.csproj", "ConsumerWorker/"]
COPY ["Shared/Shared.csproj", "Shared/"]

RUN dotnet restore "ConsumerWorker/ConsumerWorker.csproj"

COPY . .

WORKDIR "/src/ConsumerWorker"

RUN dotnet publish "ConsumerWorker.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


FROM base AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ConsumerWorker.dll"]