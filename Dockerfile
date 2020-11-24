# Copy paste from
# https://docs.docker.com/engine/examples/dotnetcore/#create-a-dockerfile-for-an-aspnet-core-application

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

COPY . ./
RUN dotnet publish -c Release -o out Server/Server.csproj

FROM microsoft/dotnet:2.1-runtime
WORKDIR /app
COPY --from=build /app/Server/out .
COPY conf.xml .
ENTRYPOINT ["dotnet", "Server.dll"]