# TechAsset Manager

Sistema corporativo para gerenciamento e controle de inventário de ativos de TI (Hardware e Periféricos). Desenvolvido como projeto de portfólio focando em boas práticas de Engenharia de Software com .NET.

![Badge](https://img.shields.io/badge/Status-Versao%201.0%20%28Estavel%29-blue) ![.NET](https://img.shields.io/badge/.NET-ASP.NET_Core-purple)


## Screenshots

### Dashboard Gerencial
Visão geral com indicadores de status em tempo real.
<img width="1849" height="914" alt="Image" src="https://github.com/user-attachments/assets/7fe8fd6f-6e5e-4718-b8a3-4f17e12fabdc" />

### Listagem de Ativos
Controle de inventário com indicadores visuais de status.
<img width="1850" height="914" alt="Image" src="https://github.com/user-attachments/assets/8a2fac6f-0c7c-4e93-8133-31131ecdba72" />

### Cadastro e Validação
Formulários com validação de dados e feedback visual em Português.
<img width="1832" height="917" alt="Image" src="https://github.com/user-attachments/assets/ee6fb507-cd1d-450a-9583-b9563f7f5edc" />

---

## Funcionalidades

### Versão 1.0 (MVP)
- [x] **CRUD Completo:** Criação, Leitura, Atualização e Exclusão de ativos.
- [x] **Dashboard Interativo:** Página inicial com contadores de equipamentos (Total, Em Uso, Disponíveis, Manutenção).
- [x] **Validação de Dados:** Regras de negócio aplicadas no Back-end com *Data Annotations* e mensagens de erro amigáveis em PT-BR.
- [x] **Identidade Visual Corporativa:** Layout customizado (Azul Royal), responsivo e profissional.
- [x] **Tipagem Forte:** Uso de *Enums* para evitar erros de digitação em Status e Tipos de equipamento.
- [x] **Data Seeding:** População automática do banco de dados para testes iniciais.

---

## Tecnologias Utilizadas

- **Linguagem:** C#
- **Framework:** ASP.NET Core MVC (.NET 10 Preview – projeto de estudo)
- **Banco de Dados:** SQL Server (LocalDB)
- **ORM:** Entity Framework Core (Code-First)
- **Front-end:** Razor Views, Bootstrap 5, CSS Customizado
- **Controle de Versão:** Git & GitHub

---

## Como Rodar o Projeto

1. **Clone o repositório:**

```bash
git clone https://github.com/drexsi0/TechAssetManager.git
```

2. **Configure o Banco de Dados:**
Certifique-se de ter o SQL Server (LocalDB) instalado (padrão no Visual Studio). O Entity Framework irá criar o banco automaticamente na primeira execução graças ao `EnsureCreated`.

3. **Execute a Aplicação:**
Abra a solução no Visual Studio e pressione `F5`.

---

Desenvolvido por **Pedro Henrique**, como parte de estudos avançados em Desenvolvimento Fullstack .NET.