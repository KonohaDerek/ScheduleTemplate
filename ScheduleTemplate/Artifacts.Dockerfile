#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
ARG ENVIRONMENT="Development"
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
RUN apt-get update -yq  && apt-get install -yq libgdiplus build-essential curl iputils-ping
WORKDIR /app
EXPOSE 80
EXPOSE 443


FROM base AS final
ENV ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}
ENV TZ=Asia/Taipei
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "ScheduleTemplate.dll"]
HEALTHCHECK --interval=10s --timeout=3s --start-period=5s CMD curl -f http://localhost/health || exit
