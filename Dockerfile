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

# Instala dependências necessárias e o tracer da Datadog
RUN apt-get update && apt-get install -y curl \
    && curl -LO https://github.com/DataDog/dd-trace-dotnet/releases/download/v2.50.0/datadog-dotnet-apm_2.50.0_amd64.deb \
    && dpkg -i datadog-dotnet-apm_2.50.0_amd64.deb \
    && rm datadog-dotnet-apm_2.50.0_amd64.deb \
    && apt-get clean

# Define variáveis de ambiente para o Datadog
ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8} \
    CORECLR_PROFILER_PATH=/opt/datadog/Datadog.Trace.ClrProfiler.Native.so \
    DD_DOTNET_TRACER_HOME=/opt/datadog \
    DD_SERVICE=fiap-cloud-games \
    DD_ENV=production \
    DD_LOGS_INJECTION=true \
    DD_RUNTIME_METRICS_ENABLED=true \
    DD_TRACE_DEBUG=true \
    DD_AGENT_HOST=datadog-agent \
    DD_API_KEY=your-api-key-here

# Copia os arquivos publicados
COPY --from=publish /app/publish .

# Define o ponto de entrada
ENTRYPOINT ["dotnet", "FIAP-Cloud-Games.dll"]