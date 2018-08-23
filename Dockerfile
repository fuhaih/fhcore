FROM microsoft/dotnet:2.1-aspnetcore-runtime

WORKDIR /app

COPY ./publish /fhcore

EXPOSE 6606/tcp

ENTRYPOINT dotnet FHCore.MVC.dll