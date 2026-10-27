# 🐾 Afetto API

> **"O sistema operacional do cuidado contínuo do pet"**

API RESTful em **ASP.NET Core** do **Afetto**, plataforma de saúde contínua de pets desenvolvida para o **Challenge FIAP 2026** (parceiro: CLYVO VET). Entrega da disciplina **Advanced Business Development with .NET**, Sprints 1 a 4.

---

## 📋 Índice

1. [Visão geral](#-visão-geral)
2. [Arquitetura](#-arquitetura)
3. [Tecnologias](#-tecnologias)
4. [Como instalar e executar](#-como-instalar-e-executar)
5. [Configuração](#-configuração)
6. [Autenticação (JWT)](#-autenticação-jwt)
7. [Endpoints](#-endpoints)
8. [Paginação, ordenação, filtros e HATEOAS](#-paginação-ordenação-filtros-e-hateoas)
9. [Tratamento de erros](#-tratamento-de-erros)
10. [Persistência: Oracle e MongoDB](#-persistência-oracle-e-mongodb)
11. [Observabilidade e Health Checks](#-observabilidade-e-health-checks)
12. [Testes](#-testes)
13. [Documentação Swagger](#-documentação-swagger)
14. [Equipe](#-equipe)

---

## 📖 Visão geral

Tutores só levam seus pets ao veterinário em situações de urgência, o que gera baixa recorrência e histórico de saúde fragmentado. O Afetto resolve isso com um cadastro contínuo do tutor e do pet, com endereço completo (País → Estado → Cidade → Bairro → Logradouro).

### Evolução por sprint

| Sprint | Entrega |
|---|---|
| 1 | CRUD de usuários, hierarquia geográfica, Oracle + EF Core, Swagger, hash de senha (BCrypt) |
| 3 | Health Checks, logs estruturados (Serilog), OpenTelemetry, testes unitários e de integração |
| 4 | Clean Architecture, JWT, paginação/ordenação/filtros, HATEOAS, tratamento global de erros, MongoDB (auditoria), README e diagrama |

---

## 🏛 Arquitetura

O projeto segue **Clean Architecture** organizada em camadas (pastas/namespaces) dentro do projeto `Afetto-.Net`. A regra de dependência é: **API → Application → Domain**, e **Infrastructure** implementa as interfaces definidas em Domain/Application. Essa regra é verificada automaticamente pelos testes de arquitetura.

```mermaid
flowchart LR
    Client([Cliente / Swagger]) --> API

    subgraph API["API (apresentação)"]
        direction TB
        Controllers["Controllers<br/>Usuario · Auth · Auditoria"]
        MW["ExceptionHandlingMiddleware<br/>(ProblemDetails)"]
        HAT["Hateoas"]
        EXT["Extensions<br/>(composition root)"]
    end

    subgraph APP["Application"]
        direction TB
        Svc["UsuarioService · AuthService<br/>AuditedUsuarioService (decorator)"]
        DTO["DTOs"]
        AI["Interfaces<br/>IUsuarioService · ITokenService<br/>ICurrentUserService"]
    end

    subgraph DOM["Domain"]
        direction TB
        Ent["Entities<br/>Usuario · Pet · Pais…Logradouro · AuditLog"]
        DI["Interfaces<br/>IUsuarioRepository · IAuditLogRepository"]
    end

    subgraph INF["Infrastructure"]
        direction TB
        EF["Persistence<br/>AppDbContext + Migrations"]
        Repo["Repositories<br/>UsuarioRepository"]
        Mongo["Mongo<br/>MongoAuditLogRepository"]
        Sec["Security<br/>TokenService (JWT)"]
        HC["HealthChecks"]
    end

    API --> APP
    APP --> DOM
    INF -. implementa .-> DOM
    INF -. implementa .-> APP
    EF --> Oracle[(Oracle)]
    Mongo --> MongoDB[(MongoDB)]
```

### Estrutura de pastas

```
Afetto-.Net/
├── Domain/
│   ├── Entities/            # Usuario, Pet, Pais, Estado, Cidade, Bairro, Logradouro, AuditLog
│   ├── Interfaces/          # IUsuarioRepository, IAuditLogRepository
│   └── Filters/             # UsuarioFiltro, AuditLogFiltro
├── Application/
│   ├── DTOs/                # Requests/Responses, paginação, links HATEOAS
│   ├── Interfaces/          # IUsuarioService, IAuthService, ITokenService, ICurrentUserService
│   └── Services/            # UsuarioService, AuthService, AuditedUsuarioService
├── Infrastructure/
│   ├── Persistence/         # AppDbContext (Oracle) + Migrations
│   ├── Repositories/        # UsuarioRepository (EF Core)
│   ├── Mongo/               # MongoAuditLogRepository, MongoSettings, mapeamento BSON
│   ├── Security/            # TokenService, JwtSettings
│   └── HealthChecks/        # Api, Oracle e MongoDB
├── Controllers/             # UsuarioController, AuthController, AuditoriaController
├── Middleware/              # ExceptionHandlingMiddleware
├── Hateoas/                 # UsuarioLinkBuilder
├── Extensions/              # Registro de dependências por camada
├── Security/                # HttpCurrentUserService
└── Program.cs

Afetto-.Net.Tests.Unit/          # Serviços, decorator de auditoria e testes de arquitetura
Afetto-.Net.Tests.Integration/   # Endpoints com WebApplicationFactory + banco em memória
docker-compose.yml               # MongoDB local
```

### Princípios aplicados

- **Repository Pattern**: `IUsuarioRepository` e `IAuditLogRepository` (Domain) com implementações concretas em Infrastructure (EF Core/Oracle e MongoDB).
- **Decorator**: `AuditedUsuarioService` adiciona auditoria ao `UsuarioService` sem alterá-lo (Open/Closed). Falha no MongoDB não derruba a operação principal.
- **Injeção de dependência** por camada, em `Extensions/ServiceCollectionExtensions.cs`.
- **DTOs** para entrada e saída: a entidade (e o hash da senha) nunca é exposta.
- **Tratamento global de exceções** em um único middleware, sem `try/catch` espalhado nos controllers.

---

## 🛠 Tecnologias

| Tecnologia | Versão | Uso |
|---|---|---|
| .NET / ASP.NET Core | 9 | API (projeto de testes unitários usa `net10.0`) |
| Entity Framework Core + Oracle.EntityFrameworkCore | 9.0.0 / 9.23.60 | ORM e driver Oracle |
| MongoDB.Driver | 3.13.0 | Persistência NoSQL (auditoria) |
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.0 | Autenticação JWT |
| Swashbuckle.AspNetCore | 6.9.0 | Swagger / OpenAPI |
| BCrypt.Net-Next | 4.0.3 | Hash de senhas |
| Serilog | 8.0.3 | Logs estruturados (console + arquivo) |
| OpenTelemetry | 1.9.0 | Traces e métricas |
| xUnit, Moq, FluentAssertions, Mvc.Testing, EF InMemory | 2.9 / 4.20 / 6.12 / 9.0 / 9.0 | Testes |

---

## ⚙️ Como instalar e executar

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download) e [.NET 10 SDK](https://dotnet.microsoft.com/download) (o projeto de testes unitários usa `net10.0`)
- Visual Studio 2022+ (ou VS Code)
- Oracle Database acessível (FIAP ou local)
- MongoDB (opcional para rodar a API; recomendado via Docker)

### Passo a passo

```bash
# 1. Clonar
git clone https://github.com/Afetto/.Net-Afetto.git
cd .Net-Afetto

# 2. (Opcional) Subir o MongoDB local
docker compose up -d

# 3. Restaurar e compilar
dotnet build

# 4. Executar a API
dotnet run --project Afetto-.Net
```

No Visual Studio: abra `Afetto-.Net.slnx` e pressione **F5**. O Swagger abre na raiz: `http://localhost:5150/` (ou `https://localhost:7295/`).

### Migrations (Oracle)

No **Package Manager Console** (projeto padrão `Afetto-.Net`):

```
Update-Database
```

A migration `InitialCreate` já está no repositório. É necessário que o Oracle esteja acessível pela connection string configurada.

---

## 🔧 Configuração

Arquivo `Afetto-.Net/appsettings.json`:

| Chave | Descrição | Exemplo |
|---|---|---|
| `ConnectionStrings:OracleConnection` | Conexão Oracle | `User id=SEU_RM;Password=SUA_SENHA;Data Source=host:1521/SERVICO` |
| `Jwt:Issuer` / `Jwt:Audience` | Emissor e audiência do token | `Afetto.API` / `Afetto.Clients` |
| `Jwt:Key` | Chave de assinatura (mín. 32 caracteres) | trocar em produção |
| `Jwt:ExpiresInMinutes` | Validade do token | `60` |
| `MongoDb:ConnectionString` | Conexão MongoDB | `mongodb://localhost:27017` |
| `MongoDb:DatabaseName` / `AuditCollection` | Banco e coleção | `afetto` / `audit_logs` |
| `MongoDb:TimeoutSeconds` | Tempo máximo para achar o servidor | `3` |

> 🔐 **Segredos não devem ir para o Git.** Use *user-secrets* ou variáveis de ambiente, que sobrescrevem o `appsettings.json`:
>
> ```bash
> dotnet user-secrets set "ConnectionStrings:OracleConnection" "..." --project Afetto-.Net
> # ou, por variável de ambiente (note o __ no lugar de :)
> MongoDb__ConnectionString="mongodb+srv://usuario:senha@cluster.mongodb.net"
> Jwt__Key="uma-chave-longa-e-secreta-com-mais-de-32-caracteres"
> ```

---

## 🔑 Autenticação (JWT)

| Rota | Acesso |
|---|---|
| `POST /api/auth/login` | Público |
| `POST /api/usuarios` (cadastro) | Público |
| Demais rotas de `/api/usuarios` e `/api/auditoria` | Exigem `Authorization: Bearer <token>` |
| `/health`, `/health/live`, `/health/ready` | Públicas |

**Fluxo:** cadastre um usuário → faça login → copie o `token` → no Swagger clique em **Authorize** e cole o token (sem a palavra `Bearer`).

```http
POST /api/auth/login
{ "email": "joao.silva@email.com", "senha": "senha123" }
```

```json
{
  "token": "eyJhbGciOi...",
  "tipoToken": "Bearer",
  "expiraEm": "2026-10-27T20:30:00Z",
  "usuarioId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "nome": "João Silva",
  "email": "joao.silva@email.com"
}
```

Credenciais inválidas retornam `401` (mesma mensagem para e-mail inexistente e senha errada).

---

## 🔀 Endpoints

### Autenticação — `/api/auth`

| Método | Rota | Descrição | Retornos |
|---|---|---|---|
| POST | `/api/auth/login` | Autentica e devolve o JWT | 200, 400, 401 |

### Usuários — `/api/usuarios`

| Método | Rota | Descrição | Auth | Retornos |
|---|---|---|---|---|
| GET | `/api/usuarios` | Lista **paginada** com ordenação e filtros | ✅ | 200, 400, 401 |
| GET | `/api/usuarios/{id}` | Busca por ID | ✅ | 200, 404 |
| GET | `/api/usuarios/email/{email}` | Busca por e-mail | ✅ | 200, 404 |
| GET | `/api/usuarios/cpf/{cpf}` | Busca por CPF | ✅ | 200, 404 |
| GET | `/api/usuarios/logradouro/{logradouroId}` | Usuários de um logradouro | ✅ | 200, 404 |
| GET | `/api/usuarios/cidade/{cidadeId}` | Usuários de uma cidade | ✅ | 200, 404 |
| POST | `/api/usuarios` | Cria usuário | ❌ público | 201, 400 |
| PUT | `/api/usuarios/{id}` | Atualiza usuário | ✅ | 200, 400, 404 |
| DELETE | `/api/usuarios/{id}` | Remove usuário | ✅ | 204, 404 |

### Auditoria (MongoDB) — `/api/auditoria`

| Método | Rota | Descrição | Auth | Retornos |
|---|---|---|---|---|
| GET | `/api/auditoria` | Logs de criação/atualização/remoção, mais recentes primeiro | ✅ | 200, 400, 401 |

Filtros: `entidadeId` (GUID), `acao` (`criado` \| `atualizado` \| `removido`), `page`, `pageSize`.

### Saúde — públicas

| Rota | Descrição |
|---|---|
| `GET /health` | Relatório completo (API, Oracle e MongoDB) |
| `GET /health/live` | Liveness (API respondendo) |
| `GET /health/ready` | Readiness (bancos de dados) |

---

## 📄 Paginação, ordenação, filtros e HATEOAS

`GET /api/usuarios` aceita:

| Parâmetro | Padrão | Regra |
|---|---|---|
| `page` | 1 | ≥ 1 |
| `pageSize` | 10 | 1 a 50 |
| `sortBy` | `nome` | `nome`, `email`, `dataNasc`, `createdAt` |
| `sortDir` | `asc` | `asc` ou `desc` |
| `nome`, `email` | — | contém (sem diferenciar maiúsculas) |
| `cidadeId`, `logradouroId` | — | igualdade |

```
GET /api/usuarios?page=2&pageSize=10&sortBy=email&sortDir=desc&nome=maria
```

```json
{
  "items": [
    {
      "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
      "nome": "Maria Souza",
      "email": "maria@email.com",
      "links": [
        { "rel": "self",   "method": "GET",    "href": "https://localhost:7295/api/usuarios/9b1deb4d-..." },
        { "rel": "update", "method": "PUT",    "href": "https://localhost:7295/api/usuarios/9b1deb4d-..." },
        { "rel": "delete", "method": "DELETE", "href": "https://localhost:7295/api/usuarios/9b1deb4d-..." }
      ]
    }
  ],
  "page": 2,
  "pageSize": 10,
  "totalItems": 25,
  "totalPages": 3,
  "links": [
    { "rel": "self",  "method": "GET", "href": ".../api/usuarios?page=2&pageSize=10&sortBy=email&sortDir=desc&nome=maria" },
    { "rel": "first", "method": "GET", "href": ".../api/usuarios?page=1&..." },
    { "rel": "last",  "method": "GET", "href": ".../api/usuarios?page=3&..." },
    { "rel": "prev",  "method": "GET", "href": ".../api/usuarios?page=1&..." },
    { "rel": "next",  "method": "GET", "href": ".../api/usuarios?page=3&..." }
  ]
}
```

Os links `prev`/`next` aparecem somente quando existem. Os filtros são preservados nos links de navegação.

---

## 🚨 Tratamento de erros

Um único middleware (`ExceptionHandlingMiddleware`) converte exceções em respostas **ProblemDetails** (RFC 7807) padronizadas:

| Exceção | Status |
|---|---|
| Regra de negócio (`InvalidOperationException`, ex.: e-mail/CPF duplicado) | 400 |
| `ArgumentException` | 400 |
| `UnauthorizedAccessException` (login inválido) | 401 |
| `KeyNotFoundException` | 404 |
| `DbUpdateException` | 409 |
| Qualquer outra | 500 (mensagem genérica fora de Development) |

```json
{
  "status": 400,
  "title": "Requisição inválida",
  "detail": "E-mail 'joao.silva@email.com' já está em uso.",
  "instance": "/api/usuarios",
  "traceId": "0HN7..."
}
```

Erros de validação de entrada (DataAnnotations) retornam `400` automaticamente.

---

## 🗄 Persistência: Oracle e MongoDB

- **Oracle + EF Core** guarda os dados transacionais (usuários, pets e endereço), com migrations versionadas em `Infrastructure/Persistence/Migrations`.
- **MongoDB** guarda a **trilha de auditoria** (`afetto.audit_logs`): quem criou, atualizou ou removeu cada usuário e quando. É NoSQL por natureza: documentos append-only, sem joins, e leitura paginada por data.
- A auditoria é um **decorator** do serviço de usuários. Se o MongoDB estiver indisponível, a operação principal continua e o erro vira um aviso no log; o health check do Mongo passa a `Degraded`.

---

## 📊 Observabilidade e Health Checks

- **Serilog**: logs estruturados no console e em arquivo rotativo (`logs/afetto-AAAAMMDD.log`), com log de requisições HTTP.
- **OpenTelemetry**: traces (ASP.NET Core e EF Core) e métricas de runtime exportados no console.
- **Health Checks**: `api` (live), `oracle` (ready, `Unhealthy` se cair) e `mongodb` (ready, `Degraded` se cair, pois a auditoria não é crítica).

---

## 🧪 Testes

```bash
dotnet test
```

| Projeto | Cobertura |
|---|---|
| `Afetto-.Net.Tests.Unit` (33 testes) | Regras de `UsuarioService`, paginação e ordenação, `AuthService`, decorator de auditoria e **testes de arquitetura** (regra de dependência entre camadas) |
| `Afetto-.Net.Tests.Integration` (40 testes + 2 opcionais) | Endpoints via `WebApplicationFactory` com banco em memória: CRUD, JWT (401 sem token), paginação, filtros, HATEOAS, auditoria, health checks |

- Padrão **AAA** (Arrange, Act, Assert) e nomes no formato `Metodo_Cenario_Resultado`.
- Os testes de integração substituem o Oracle por EF InMemory e o MongoDB por um repositório em memória, então rodam sem infraestrutura externa.
- **2 testes opcionais** validam o repositório contra um MongoDB real. Para executá-los:

  ```bash
  docker compose up -d
  MONGO_TEST_CONNECTION=mongodb://localhost:27017 dotnet test        # Linux/macOS
  $env:MONGO_TEST_CONNECTION="mongodb://localhost:27017"; dotnet test # PowerShell
  ```

---

## 📘 Documentação Swagger

- Interface interativa: `http://localhost:5150/` (ambiente Development), com botão **Authorize** para o JWT.
- Especificação OpenAPI exportada: [`docs/swagger.json`](docs/swagger.json) (gerada de `/swagger/v1/swagger.json`).
- Todos os endpoints possuem descrição (`/// <summary>`) e códigos de resposta documentados (`[ProducesResponseType]`).

---

## 👥 Equipe

| Nome | RM |
|---|---|
| Gustavo Souto de Melo | 558595 |
| Iago Liziero Pereira | 564063 |
| Ícaro José dos Santos | 562403 |
| Leonardo Barbosa Santos | 558230 |

**FIAP — Challenge 2026 · Advanced Business Development with .NET**
