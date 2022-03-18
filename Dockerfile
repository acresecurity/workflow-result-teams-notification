FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
COPY Worker ./Worker
RUN dotnet publish Worker -c Release -o out
COPY entrypoint.sh ./out/entrypoint.sh

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
WORKDIR /app
COPY --from=build-env /app/out .
RUN ls /
COPY . /
RUN ls /

# Code file to execute when the docker container starts up (`entrypoint.sh`)
ENTRYPOINT ["/bin/bash", "/entrypoint.sh"]