FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80 443

ARG BUILD_DEVELOPMENT
ARG VERSION
ARG VERSION_SUFFIX
# use docker ternary in base flow
ENV BUILDCONFIG=${BUILD_DEVELOPMENT:+Debug}
ENV BUILDCONFIG=${BUILDCONFIG:-Release}
ENV ASPNETCORE_ENVIRONMENT=${BUILD_DEVELOPMENT:+Development}
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Production}
ENV VERSION=${VERSION:-1.0.0}
ENV VERSION_SUFFIX=${VERSION_SUFFIX:+"-$VERSION_SUFFIX"}

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /build

ARG BUILD_DEVELOPMENT
ARG VERSION
ARG VERSION_SUFFIX

# use docker ternary in build flow
ENV BUILDCONFIG=${BUILD_DEVELOPMENT:+Debug}
ENV BUILDCONFIG=${BUILDCONFIG:-Release}
ENV ASPNETCORE_ENVIRONMENT=${BUILD_DEVELOPMENT:+Development}
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Production}
ENV VERSION=${VERSION:-1.0.0}
ENV VERSION_SUFFIX=${VERSION_SUFFIX:+"-$VERSION_SUFFIX"}

# Source Files
COPY ["src/", "src/"]

RUN dotnet restore "src/SF.PhotoPixels.API/SF.PhotoPixels.API.csproj"
RUN dotnet build /p:AssemblyVersion=${VERSION} /p:Version=${VERSION}${VERSION_SUFFIX} "src/SF.PhotoPixels.API/SF.PhotoPixels.API.csproj" --no-restore -c ${BUILDCONFIG}

FROM build AS publish
WORKDIR /build
RUN dotnet publish /p:AssemblyVersion=${VERSION} /p:Version=${VERSION}${VERSION_SUFFIX} --no-build "src/SF.PhotoPixels.API/SF.PhotoPixels.API.csproj" -c ${BUILDCONFIG} -o /app/publish

FROM base AS final
WORKDIR /app

RUN if [ "${BUILDCONFIG}" = "Debug" ] ; then \
        apt-get update && \
        curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /app/vsdbg; \
    fi

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SF.PhotoPixels.API.dll"]