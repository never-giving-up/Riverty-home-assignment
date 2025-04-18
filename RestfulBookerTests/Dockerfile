FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /app
COPY . ./

RUN dotnet restore
RUN dotnet build -c Release --no-restore

ENTRYPOINT ["dotnet", "test", "--logger", "trx", "--no-build", "--results-directory", "/app/test-results", "--verbosity", "normal"]