FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
COPY Worker ./Worker
RUN dotnet publish Worker -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
COPY --from=build-env /app/out /

# Code file to execute when the docker container starts up (`entrypoint.sh`)
ENTRYPOINT ["dotnet", "/Worker.dll"]