# 🐾 Afetto API

> **"O sistema operacional do cuidado contínuo do pet"**

API RESTful desenvolvida em **ASP.NET Core** para o projeto **Afetto**, sistema de gerenciamento de saúde contínua de pets. Projeto desenvolvido para o **Challenge FIAP 2026** em parceria com a **CLYVO VET**.

---

## 📋 Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Tecnologias](#tecnologias)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Como Instalar e Executar](#como-instalar-e-executar)
- [Rotas da API](#rotas-da-api)
- [Exemplos de Requisição](#exemplos-de-requisição)
- [Equipe](#equipe)

---

## 📖 Sobre o Projeto

O **Afetto** resolve um gap crítico do mercado pet brasileiro: tutores só levam seus animais ao veterinário em situações de urgência, gerando baixa recorrência nas clínicas e cuidado reativo em vez de preventivo.

A API gerencia o cadastro completo de usuários (tutores) com hierarquia geográfica completa (País → Estado → Cidade → Bairro → Logradouro), servindo como base para as funcionalidades de acompanhamento de saúde dos pets.

### Funcionalidades da Sprint 1
- ✅ CRUD completo de Usuários
- ✅ 6 rotas GET parametrizadas
- ✅ Integração com banco Oracle via EF Core
- ✅ Documentação com Swagger
- ✅ Validação de dados e regras de negócio (CPF e e-mail únicos, hash de senha)

---

## 🛠 Tecnologias

| Tecnologia | Versão | Uso |
|---|---|---|
| .NET | 10 | Plataforma base |
| ASP.NET Core | 10 | Framework Web API |
| Entity Framework Core | 9.0.0 | ORM |
| Oracle EF Core | 9.21.140 | Driver Oracle |
| Swashbuckle (Swagger) | 6.9.0 | Documentação da API |
| BCrypt.Net-Next | 4.0.3 | Hash de senhas |

---

## 📁 Estrutura do Projeto

```
Afetto-.Net/
│
├── Controllers/
│   └── UsuarioController.cs      # Endpoints REST do usuário
│
├── Data/
│   └── AppDbContext.cs           # Contexto do EF Core + mapeamento Oracle
│
├── DTOs/
│   └── UsuarioDto.cs             # Request e Response de usuário
│
├── Models/
│   ├── Pais.cs
│   ├── Estado.cs
│   ├── Cidade.cs
│   ├── Bairro.cs
│   ├── Logradouro.cs
│   ├── Usuario.cs
│   └── Pet.cs
│
├── Repositories/
│   ├── IUsuarioRepository.cs     # Interface do repositório
│   └── UsuarioRepository.cs     # Implementação com EF Core
│
├── Services/
│   ├── IUsuarioService.cs        # Interface do serviço
│   └── UsuarioService.cs        # Regras de negócio
│
├── appsettings.json              # Connection string Oracle
└── Program.cs                    # Configuração da aplicação
```

---

## ⚙️ Como Instalar e Executar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) ou VS Code
- Oracle Database (local ou Docker)

### Passo a passo

**1. Clone o repositório**
```bash
git clone https://github.com/Afetto/.Net-Afetto.git
cd Afetto-.Net
```

**2. Configure a connection string**

Abra o `appsettings.json` e ajuste com seus dados do Oracle:
```json
{
  "ConnectionStrings": {
    "OracleConnection": "User id=SEU_USUARIO;Password=SUA_SENHA;Data Source=localhost:1521/XEPDB1"
  }
}
```

**3. Instale os pacotes NuGet**

No Package Manager Console:
```
Install-Package Microsoft.EntityFrameworkCore -Version 9.0.0
Install-Package Microsoft.EntityFrameworkCore.Tools -Version 9.0.0
Install-Package Oracle.EntityFrameworkCore -Version 9.21.140
Install-Package Swashbuckle.AspNetCore -Version 6.9.0
Install-Package BCrypt.Net-Next
```

**4. Execute as Migrations**

No Package Manager Console:
```
Add-Migration InitialCreate
Update-Database
```

**5. Execute o projeto**
```bash
dotnet run
```

Ou pressione **F5** no Visual Studio.

**6. Acesse o Swagger**

Abra no navegador:
```
http://localhost:{PORTA}/
```

---

## 🔀 Rotas da API

### Usuário — `/api/usuarios`

| Método | Rota | Descrição | Status de Retorno |
|---|---|---|---|
| GET | `/api/usuarios` | Lista todos os usuários | 200 |
| GET | `/api/usuarios/{id}` | Busca usuário por ID | 200, 404 |
| GET | `/api/usuarios/email/{email}` | Busca usuário por e-mail | 200, 404 |
| GET | `/api/usuarios/cpf/{cpf}` | Busca usuário por CPF | 200, 404 |
| GET | `/api/usuarios/logradouro/{logradouroId}` | Lista usuários por logradouro | 200, 404 |
| GET | `/api/usuarios/cidade/{cidadeId}` | Lista usuários por cidade | 200, 404 |
| POST | `/api/usuarios` | Cria novo usuário | 201, 400 |
| PUT | `/api/usuarios/{id}` | Atualiza usuário existente | 200, 400, 404 |
| DELETE | `/api/usuarios/{id}` | Remove usuário | 204, 404 |

---

## 📨 Exemplos de Requisição

### POST `/api/usuarios` — Criar usuário

**Request Body:**
```json
{
  "nome": "João Silva",
  "cpf": "12345678901",
  "dataNasc": "1995-06-15T00:00:00",
  "email": "joao.silva@email.com",
  "senha": "senha123",
  "telefone": "11999999999",
  "numero": "42",
  "complemento": "Apto 3",
  "logradouroId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response 201:**
```json
{
  "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "nome": "João Silva",
  "cpf": "12345678901",
  "dataNasc": "1995-06-15T00:00:00",
  "email": "joao.silva@email.com",
  "telefone": "11999999999",
  "numero": "42",
  "complemento": "Apto 3",
  "createdAt": "2026-05-20T03:00:00",
  "logradouroId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "endereco": {
    "logradouro": "Rua das Flores",
    "cep": "01310100",
    "tipo": "Rua",
    "bairro": "Bela Vista",
    "cidade": "São Paulo",
    "estado": "São Paulo",
    "pais": "Brasil"
  }
}
```

### PUT `/api/usuarios/{id}` — Atualizar usuário

**Request Body:**
```json
{
  "nome": "João Silva Atualizado",
  "dataNasc": "1995-06-15T00:00:00",
  "email": "joao.novo@email.com",
  "telefone": "11988888888",
  "numero": "50",
  "complemento": "Casa",
  "logradouroId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### DELETE `/api/usuarios/{id}`

Sem body. Retorna **204 No Content** em caso de sucesso.

---

## 👥 Equipe

Desenvolvido pela equipe **Afetto** — Graduação em Tecnologia, FIAP 2026.

| Nome | GitHub |
|---|---|
| Afetto | [@Afetto](https://github.com/Afetto) |

---

## 📄 Licença

Este projeto foi desenvolvido para fins acadêmicos no **Challenge FIAP 2026** em parceria com a **CLYVO VET**.
