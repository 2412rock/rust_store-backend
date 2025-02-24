# Use the official .NET 7 SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy the .csproj and restore any dependencies (via NuGet)
COPY *.csproj ./
RUN dotnet restore

# Copy the remaining source code and build the application
COPY . ./

RUN dotnet publish -c Release -o out

# Use a smaller runtime image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

WORKDIR /app
#COPY backendcertificate.pfx /app/backendcertificate.pfx
# Copy the built application from the build image
COPY --from=build /app/out ./

# Expose port 80 for the application
EXPOSE 4500

# Define the command to run the application when the container starts
ENTRYPOINT ["dotnet", "FFhub-backend.dll"]