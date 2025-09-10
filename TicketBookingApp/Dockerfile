# Base image with ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Only expose HTTP (no HTTPS)
EXPOSE 8080

# Set Kestrel to listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["TicketBookingApp/TicketBookingApp.csproj", "TicketBookingApp/"]
RUN dotnet restore "./TicketBookingApp/TicketBookingApp.csproj"

# Copy the entire solution and build
COPY . .
WORKDIR "/src/TicketBookingApp"
RUN dotnet build "./TicketBookingApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TicketBookingApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TicketBookingApp.dll"]