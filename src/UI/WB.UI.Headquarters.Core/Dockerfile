#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-nanoserver-1903 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-nanoserver-1903 AS build
WORKDIR /src
COPY ["UI/WB.UI.Headquarters.Core/WB.UI.Headquarters.csproj", "UI/WB.UI.Headquarters.Core/"]
COPY ["Infrastructure/WB.Persistence.Headquarters/WB.Persistence.Headquarters.csproj", "Infrastructure/WB.Persistence.Headquarters/"]
COPY ["Core/BoundedContexts/Headquarters/WB.Core.BoundedContexts.Headquarters/WB.Core.BoundedContexts.Headquarters.csproj", "Core/BoundedContexts/Headquarters/WB.Core.BoundedContexts.Headquarters/"]
COPY ["Core/SharedKernels/DataCollection/DataCollection/WB.Core.SharedKernels.DataCollection.csproj", "Core/SharedKernels/DataCollection/DataCollection/"]
COPY ["Core/GenericSubdomains/WB.Core.GenericSubdomains.Portable/WB.Core.GenericSubdomains.Portable.csproj", "Core/GenericSubdomains/WB.Core.GenericSubdomains.Portable/"]
COPY ["Core/Infrastructure/WB.Core.Infrastructure/WB.Core.Infrastructure.csproj", "Core/Infrastructure/WB.Core.Infrastructure/"]
COPY ["Core/SharedKernels/Questionnaire/WB.Core.SharedKernels.Questionnaire/WB.Core.SharedKernels.Questionnaire.csproj", "Core/SharedKernels/Questionnaire/WB.Core.SharedKernels.Questionnaire/"]
COPY ["Core/SharedKernels/DataCollection/DataCollection.Portable/WB.Core.SharedKernels.DataCollection.Portable.csproj", "Core/SharedKernels/DataCollection/DataCollection.Portable/"]
COPY ["Infrastructure/WB.Infrastructure.Native/WB.Infrastructure.Native.csproj", "Infrastructure/WB.Infrastructure.Native/"]
COPY ["Core/SharedKernels/Enumerator/WB.Enumerator.Native/WB.Enumerator.Native.Core.csproj", "Core/SharedKernels/Enumerator/WB.Enumerator.Native/"]
COPY ["UI/Shared/WB.UI.Shared.Web.Core/WB.UI.Shared.Web.Core.csproj", "UI/Shared/WB.UI.Shared.Web.Core/"]
RUN dotnet restore "UI/WB.UI.Headquarters.Core/WB.UI.Headquarters.csproj"
COPY . .
WORKDIR "/src/UI/WB.UI.Headquarters.Core"
RUN dotnet build "WB.UI.Headquarters.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WB.UI.Headquarters.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WB.UI.Headquarters.dll"]