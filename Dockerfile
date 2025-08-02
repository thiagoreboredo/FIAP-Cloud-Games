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

# Muda para o usuário root para poder instalar pacotes
USER root
# Atualiza os pacotes e instala as ferramentas necessárias (curl e tar)
RUN apt-get update && apt-get install -y curl tar
# Baixa o tarball do agente Datadog, descompacta, torna o script executável e remove o arquivo baixado
RUN set -ex && \
    curl -Lf --output datadog-dotnet-apm.tar.gz https://github.com/DataDog/dd-trace-dotnet/releases/latest/download/dd-trace-linux-x64.tar.gz && \
    mkdir -p /opt/datadog && \
    tar -xzf datadog-dotnet-apm.tar.gz -C /opt/datadog --strip-components=1 && \
    chmod +x /opt/datadog/create-dotnet-tracer-env.sh && \
    rm datadog-dotnet-apm.tar.gz
# Volta para o usuário padrão da imagem (boa prática de segurança)
USER app

WORKDIR /app
COPY --from=publish /app/publish .

# Define o ponto de entrada que irá iniciar a sua API quando o contêiner rodar.
ENTRYPOINT ["/opt/datadog/create-dotnet-tracer-env.sh", "dotnet", "FIAP-Cloud-Games.dll"]