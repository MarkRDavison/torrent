FROM mcr.microsoft.com/dotnet/sdk:5.0 as BUILD
WORKDIR /app 

COPY / /app/
RUN dotnet restore
RUN dotnet test
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=BUILD /app/out .

ENTRYPOINT ["dotnet", "Zeno.Torrent.API.dll"]