FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 6606/tcp

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Host/FHCore.MVC/FHCore.MVC.csproj Host/FHCore.MVC/
RUN dotnet restore Host/FHCore.MVC/FHCore.MVC.csproj
COPY . .
RUN dotnet build Host/FHCore.MVC/FHCore.MVC.csproj -c Release -o /app

FROM build AS publish
COPY Host/FHCore.MVC/layui /app/layui
RUN dotnet publish Host/FHCore.MVC/FHCore.MVC.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT dotnet FHCore.MVC.dll