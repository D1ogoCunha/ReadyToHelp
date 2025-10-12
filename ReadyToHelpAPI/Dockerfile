FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ReadyToHelpAPI.csproj", "./"]
RUN dotnet restore "ReadyToHelpAPI.csproj"

COPY . ./
RUN dotnet publish "ReadyToHelpAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "ReadyToHelpAPI.dll"]
