# ReadyToHelp

Aplicação para reportar ocorrências (várias categorias) e permitir que utilizadores confirmem/atualizem o estado das mesmas (estilo Waze: confirmações/feedback). Este repositório contém o backend (ASP.NET Core + Entity Framework). O projeto terá também cliente móvel em Kotlin e frontend web em Angular.

## Tecnologias
- Backend: .NET 8+ (ASP.NET Core) + Entity Framework Core
- Web: Angular
- Mobile: Kotlin (Android)
- BD: SQL Server / PostgreSQL (configurável)

## Pré-requisitos
- Git
- .NET SDK (8+)
- dotnet-ef
- SQL Server / PostgreSQL ou Docker
- Node.js + npm + Angular CLI 
- Android Studio 

## Connect with GIT
```
cd existing_repo
git remote add origin https://gitlab.com/grupo_07-lds-2526/lds_25_26.git
git branch -M main
git push -uf origin main
```
## Clone
```bash
git clone https://gitlab.com/grupo_07-lds-2526/lds_25_26.git
cd readytohelp
```

## Backend — passos rápidos (Windows)
1. Entrar na pasta do backend:
```bash
cd backend
```
2. Restaurar dependências e preparar ferramentas:
```bash
dotnet restore
dotnet tool install --global dotnet-ef   # se necessário
```
3. Configurar a string de conexão
- Editar appsettings.Development.json
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=readytohelp;Username=postgres;Password=postgres"
}
```
4. Criar e aplicar migrations:
```bash
dotnet ef migrations add InitialCreate --project ReadyToHelp.Api --startup-project ReadyToHelp.Api
dotnet ef database update --project ReadyToHelp.Api --startup-project ReadyToHelp.Api
```
5. Executar API:
```bash
dotnet run --project ReadyToHelp.Api
```
- Swagger: http://localhost:5000/swagger (ou porta indicada no output)

6. Testes:
```bash
dotnet test

