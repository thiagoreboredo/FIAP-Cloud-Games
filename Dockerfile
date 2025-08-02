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

# Estágio 3: Obter o tracer do Datadog
FROM ghcr.io/datadog/dd-trace-dotnet/dd-trace-dotnet:latest AS datadog-tracer

# Estágio 4: Imagem Final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Copia os arquivos do tracer do Datadog do estágio anterior
COPY --from=datadog-tracer /opt/datadog-dotnet /opt/datadog

# Muda para o usuário root apenas para dar permissão de execução
USER root
RUN chmod +x /opt/datadog/create-dotnet-tracer-env.sh

# Volta para o usuário padrão da imagem (boa prática de segurança)
USER app

WORKDIR /app
COPY --from=publish /app/publish .

# Define o ponto de entrada que irá iniciar a sua API quando o contêiner rodar.
ENTRYPOINT ["/opt/datadog/create-dotnet-tracer-env.sh", "dotnet", "FIAP-Cloud-Games.dll"]