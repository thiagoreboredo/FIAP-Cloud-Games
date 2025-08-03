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

# ---- INÍCIO: Instalação do Agente New Relic (Método Oficial) ----
# Instala as dependências para adicionar um novo repositório (curl, gnupg)
RUN apt-get update && apt-get install -y --no-install-recommends curl gnupg && \
    # Adiciona a chave de segurança GPG do New Relic
    curl -sS https://download.newrelic.com/548C16BF.gpg | gpg --dearmor -o /usr/share/keyrings/newrelic-archive-keyring.gpg
# Adiciona o repositório APT do New Relic na lista de fontes do sistema
# A imagem base do .NET 8 usa Debian Bookworm
RUN echo "deb [signed-by=/usr/share/keyrings/newrelic-archive-keyring.gpg] https://apt.newrelic.com/debian/ stable main" \
    | tee /etc/apt/sources.list.d/newrelic-dotnet-agent.list
# Atualiza a lista de pacotes e finalmente instala o agente .NET
RUN apt-get update && apt-get install -y newrelic-dotnet-agent
# Configura as variáveis de ambiente para ativar o profiler do New Relic
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A}
ENV CORE_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so
ENV NEW_RELIC_APP_NAME="FIAP-Cloud-Games-Azure"
# ---- FIM: Instalação do Agente New Relic ----

WORKDIR /app
COPY --from=publish /app/publish .

# Define o ponto de entrada
ENTRYPOINT ["dotnet", "FIAP-Cloud-Games.dll"]