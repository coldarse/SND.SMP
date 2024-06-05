FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["src/SND.SMP.Web.Host/SND.SMP.Web.Host.csproj", "src/SND.SMP.Web.Host/"]
COPY ["src/SND.SMP.Web.Core/SND.SMP.Web.Core.csproj", "src/SND.SMP.Web.Core/"]
COPY ["src/SND.SMP.Application/SND.SMP.Application.csproj", "src/SND.SMP.Application/"]
COPY ["src/SND.SMP.Core/SND.SMP.Core.csproj", "src/SND.SMP.Core/"]
COPY ["src/SND.SMP.EntityFrameworkCore/SND.SMP.EntityFrameworkCore.csproj", "src/SND.SMP.EntityFrameworkCore/"]
COPY ["src/SND.SMP.Shared/SND.SMP.Shared.csproj", "src/SND.SMP.Shared/"]
WORKDIR "/src/src/SND.SMP.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/SND.SMP.Web.Host", "src/SND.SMP.Web.Host"]
COPY ["src/SND.SMP.Web.Core", "src/SND.SMP.Web.Core"]
COPY ["src/SND.SMP.Application", "src/SND.SMP.Application"]
COPY ["src/SND.SMP.Core", "src/SND.SMP.Core"]
COPY ["src/SND.SMP.EntityFrameworkCore", "src/SND.SMP.EntityFrameworkCore"]
COPY ["src/SND.SMP.Shared", "src/SND.SMP.Shared"]
WORKDIR "/src/src/SND.SMP.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "SND.SMP.Web.Host.dll"]
