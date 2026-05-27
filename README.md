# TechAsset Manager

Sistema web para gerenciamento de inventário de ativos de TI, desenvolvido em ASP.NET Core MVC como projeto principal de portfólio. O foco é demonstrar autenticação, autorização por perfis, rastreabilidade, Entity Framework Core, PostgreSQL, Docker e uma UI administrativa simples com Razor Views.

![Status](https://img.shields.io/badge/status-portfolio%20ready-blue) ![.NET](https://img.shields.io/badge/.NET-10%20Preview-purple) ![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512bd4)

## Problema

Empresas pequenas e médias frequentemente controlam notebooks, monitores, periféricos e servidores em planilhas descentralizadas. Isso dificulta saber onde cada item está, quem é o responsável, qual é o status operacional e qual é o valor patrimonial aproximado.

## Solução

O TechAsset Manager centraliza o cadastro de ativos, permite acompanhar status e setor, atribuir responsáveis, consultar histórico de movimentações e exportar relatórios em Excel. O projeto foi construído como uma aplicação MVC server-side para manter simplicidade operacional e boa aderência ao ecossistema .NET.

## Funcionalidades

- Dashboard com indicadores de inventário, valor investido, valor atual estimado e gráficos por setor, status e tipo.
- CRUD de ativos com validação via Data Annotations.
- Atribuição de ativos a usuários cadastrados.
- Auditoria de criação, edição, atribuição e exclusão lógica.
- Soft delete para preservar histórico.
- Busca, filtros combinados e paginação server-side.
- Exportação de relatório em Excel com ClosedXML.
- ASP.NET Core Identity com confirmação de e-mail, 2FA e lockout em falhas de login.
- RBAC com perfis `Admin`, `Manager` e `User`.
- Tela administrativa para alteração de perfis de usuários.
- Dockerfile e GitHub Actions para build, testes e auditoria de pacotes.

## Stack

- C# e ASP.NET Core MVC/Razor Pages
- ASP.NET Core Identity
- Entity Framework Core com migrations
- PostgreSQL via Npgsql
- Bootstrap 5, Razor Views e Chart.js
- ClosedXML para exportação Excel
- xUnit para testes automatizados
- Docker e GitHub Actions

## Segurança

O projeto evita credenciais versionadas. Configure dados sensíveis por User Secrets no ambiente local ou variáveis de ambiente no deploy.

Variáveis esperadas:

```bash
DATABASE_URL=postgres://usuario:senha/host/banco
Smtp__User=seu-email-smtp
Smtp__Pass=sua-senha-ou-app-password
SEED_ADMIN_EMAIL=admin@exemplo.com
SEED_ADMIN_PASSWORD=uma-senha-forte
DEMO_MODE=false
```

Notas importantes:

- `DEMO_MODE=true` auto-confirma o e-mail no cadastro para facilitar demonstrações públicas. Use apenas em ambiente de demo.
- O seed de administrador só roda se `SEED_ADMIN_EMAIL` e `SEED_ADMIN_PASSWORD` estiverem configurados.
- Depois de qualquer segredo exposto, rotacione a credencial no provedor externo antes de publicar o repositório.

## Como rodar localmente

1. Clone o repositório.
2. Configure a connection string e SMTP via User Secrets ou variáveis de ambiente.
3. Restaure e compile:

```bash
dotnet restore GerenciadorAtivosSolution.slnx
dotnet build GerenciadorAtivosSolution.slnx
```

4. Execute a aplicação:

```bash
dotnet run --project GerenciadorAtivos/GerenciadorAtivos.csproj
```

5. Acesse a URL exibida pelo ASP.NET Core.

## Testes e auditoria

```bash
dotnet test GerenciadorAtivosSolution.slnx
dotnet list GerenciadorAtivos/GerenciadorAtivos.csproj package --vulnerable --include-transitive
```

## Decisões técnicas

- MVC server-side em vez de SPA para reduzir complexidade e destacar fundamentos de backend .NET.
- EF Core Code First para versionar evolução do schema.
- Identity padrão customizado em vez de autenticação própria.
- Soft delete para preservar rastreabilidade.
- Roles simples e explícitas para facilitar demonstração de autorização.

## Próximas melhorias

- Criar uma API REST complementar para integração externa.
- Adicionar testes de controller/autorização com WebApplicationFactory.
- Adicionar logs estruturados e health checks.
- Criar uma demo em vídeo curto ou GIF mostrando login, dashboard, CRUD, atribuição, auditoria e exportação.
- Evoluir a tela de usuários para bloquear/desbloquear contas e resetar 2FA.

## Autor

Desenvolvido por Pedro Henrique como projeto de portfólio em desenvolvimento fullstack .NET.
