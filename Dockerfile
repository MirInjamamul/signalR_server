#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Base Image from Microsoft
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

#Enviornment Variable
Env ASPNETCORE_URLS="http://*:5000"
ENV ASPNETCORE_ENVIRONMENT = "Development"

WORKDIR /src

COPY . .

RUN dotnet restore

RUN dotnet build
RUN dotnet dev-certs https --trust
EXPOSE 5000

ENTRYPOINT ["dotnet", "run"]