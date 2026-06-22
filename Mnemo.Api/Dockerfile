# Use .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY *.csproj .
# Restore dependencies (caching dependencies)
RUN dotnet restore

# Copy all source code
COPY . .
# Publish release build
RUN dotnet publish -c Release -o /app

# Use runtime-only image for binares
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published binaries from build stage
COPY --from=build /app .

# Configure Kestrel to listen on port 8080 inside container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Start backend
ENTRYPOINT ["dotnet", "Mnemo.dll"]