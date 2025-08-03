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
WORKDIR /app

# Instalar curl e baixar o Datadog Tracer
RUN apt-get update && apt-get install -y curl && \
    mkdir -p /opt/datadog && \
    curl -LO https://github.com/DataDog/dd-trace-dotnet/releases/latest/download/datadog-dotnet-apm_2.53.0_amd64.deb && \
    dpkg -i ./datadog-dotnet-apm_2.53.0_amd64.deb && \
    rm ./datadog-dotnet-apm_2.53.0_amd64.deb && \
    rm -rf /var/lib/apt/lists/*

# Copia os arquivos publicados
COPY --from=publish /app/publish .

# Configurações básicas do Datadog
ENV DD_TRACE_ENABLED=true
ENV DD_SERVICE=fiap-cloud-games-api
ENV DD_VERSION=1.0.0

# Define o ponto de entrada
ENTRYPOINT ["dotnet", "FIAP-Cloud-Games.dll"]