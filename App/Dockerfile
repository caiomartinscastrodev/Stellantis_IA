# =========================
# 1. Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copia todo o projeto
COPY . .

# Restaura as dependências
RUN dotnet restore

# Compila e publica
RUN dotnet publish -c Release -o /app/publish


# =========================
# 2. Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Copia a aplicação publicada
COPY --from=build /app/publish .

# Copia os PDFs
COPY --from=build /src/documents ./documents

# API escutará na porta 8080
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "App.dll"]