# Use the official .NET Core SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["src/SND.SMP.ItemTrackingRetriever/SND.SMP.ItemTrackingRetriever.csproj", "src/SND.SMP.ItemTrackingRetriever/"]
COPY ["src/SND.SMP.Shared/SND.SMP.Shared.csproj", "src/SND.SMP.Shared/"]
COPY ["src/SND.SMP.Core/SND.SMP.Core.csproj", "src/SND.SMP.Core/"]
COPY ["src/SND.SMP.EntityFrameworkCore/SND.SMP.EntityFrameworkCore.csproj", "src/SND.SMP.EntityFrameworkCore/"]
WORKDIR "/src/src/SND.SMP.ItemTrackingRetriever"
RUN dotnet restore 

WORKDIR /src
COPY ["src/SND.SMP.ItemTrackingRetriever", "src/SND.SMP.ItemTrackingRetriever"]
COPY ["src/SND.SMP.Shared", "src/SND.SMP.Shared"]
COPY ["src/SND.SMP.Core", "src/SND.SMP.Core"]
COPY ["src/SND.SMP.EntityFrameworkCore", "src/SND.SMP.EntityFrameworkCore"]
WORKDIR "/src/src/SND.SMP.ItemTrackingRetriever"
RUN dotnet publish -c Release -o /publish --no-restore

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /publish .

# Set the entry point for the container
ENTRYPOINT ["dotnet", "SND.SMP.ItemTrackingRetriever.dll"]
