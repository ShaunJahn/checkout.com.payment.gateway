# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Switch to root user to install packages
USER root

# Install curl
RUN apt-get update && apt-get install -y curl

USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8443


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/PaymentGateway.Api/PaymentGateway.Api.csproj", "src/PaymentGateway.Api/"]
COPY ["src/PaymentGateway.Common/PaymentGateway.Common.csproj", "src/PaymentGateway.Common/"]
COPY ["src/PaymentGateway.Api.Application/PaymentGateway.Api.Application.csproj", "src/PaymentGateway.Api.Application/"]
COPY ["src/PaymentGateway.Infrastructure/PaymentGateway.Infrastructure.csproj", "src/PaymentGateway.Infrastructure/"]
COPY ["src/PaymentGateway.Contracts/PaymentGateway.Contracts.csproj", "src/PaymentGateway.Contracts/"]
COPY ["src/PaymentGateway.PaymentService/PaymentGateway.PaymentService.csproj", "src/PaymentGateway.PaymentService/"]
COPY ["src/PaymentGateway.Bank.Simulator/PaymentGateway.Bank.Simulator.csproj", "src/PaymentGateway.Bank.Simulator/"]
RUN dotnet restore "./src/PaymentGateway.Api/PaymentGateway.Api.csproj"
COPY . .
WORKDIR "/src/src/PaymentGateway.Api"
RUN dotnet build "./PaymentGateway.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PaymentGateway.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentGateway.Api.dll"]