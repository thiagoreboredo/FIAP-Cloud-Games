# Estágio 1: Build da Aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia todos os arquivos .csproj e o arquivo .sln para restaurar as dependências.
COPY ["FIAP-Cloud-Games/FIAP-Cloud-Games.sln", "./FIAP-Cloud-Games/"]
COPY ["Application/Application.csproj", "./Application/"]
COPY ["Domain/Domain.csproj", "./Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "./Infrastructure/"]
COPY ["FIAP-Cloud-Games/FIAP-Cloud-Games.csproj", "./FIAP-Cloud-Games/"]
COPY ["FIAP-Cloud-GamesTest/FIAP-Cloud-GamesTest.csproj", "./FIAP-Cloud-GamesTest/"]

# O comando restore usa o .sln para restaurar os pacotes de todos os projetos.
RUN dotnet restore "FIAP-Cloud-Games/FIAP-Cloud-Games.sln"

# Copia todo o resto do código fonte para a imagem
COPY . .

# Aponta o diretório de trabalho para o projeto da API
WORKDIR "/src/FIAP-Cloud-Games"

# Executa o build do projeto da API. O .NET se encarrega de compilar as dependências.
RUN dotnet build "FIAP-Cloud-Games.csproj" -c Release -o /app/build

# Estágio 2: Publicação da Aplicação
FROM build AS publish
RUN dotnet publish "FIAP-Cloud-Games.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 3: Imagem Final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Instalar New Relic .NET Agent
RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
&& echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
&& wget https://download.newrelic.com/548C16BF.gpg \
&& apt-key add 548C16BF.gpg \
&& apt-get update \
&& apt-get install -y 'newrelic-dotnet-agent' \
&& rm -rf /var/lib/apt/lists/*

# Ativando o New Relic .NET Agent
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A}
ENV CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent
ENV CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so

WORKDIR /app
COPY --from=publish /app/publish .

# Define o ponto de entrada
ENTRYPOINT ["dotnet", "FIAP-Cloud-Games.dll"]