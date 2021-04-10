#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
COPY ./src/ ./src/
RUN dotnet restore "/src/BirdsiteLive/BirdsiteLive.csproj"
RUN dotnet restore "/src/BSLManager/BSLManager.csproj"
RUN dotnet build "/src/BirdsiteLive/BirdsiteLive.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "/src/BirdsiteLive/BirdsiteLive.csproj" -c Release -o /app/publish
RUN dotnet publish "/src/BSLManager/BSLManager.csproj" -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BirdsiteLive.dll"]