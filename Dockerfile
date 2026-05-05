FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SnackFlowMES.csproj ./
RUN dotnet restore ./SnackFlowMES.csproj

COPY . ./
RUN dotnet publish ./SnackFlowMES.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "SnackFlowMES.dll"]