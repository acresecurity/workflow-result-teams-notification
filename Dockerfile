FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
ENV PROJECT_NAME="Worker"
WORKDIR /app
COPY $PROJECT_NAME/ ./$PROJECT_NAME
RUN dotnet publish $PROJECT_NAME -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build-env /app/$PROJECT_NAME/out .
ENV PROJECT_NAME="Worker"

# Code file to execute when the docker container starts up (`entrypoint.sh`)
ENTRYPOINT ["dotnet", "Worker.dll"]