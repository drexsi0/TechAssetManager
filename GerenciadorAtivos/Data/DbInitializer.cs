using GerenciadorAtivos.Models;

namespace GerenciadorAtivos.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Garante que o banco foi criado
            context.Database.EnsureCreated();

            // Verifica se já existem ativos. Se tiver, não faz nada.
            if (context.Ativos.Any())
            {
                return;   // O banco já tem dados
            }

            // Se chegou aqui, é porque está vazio. Vamos criar os dados!
            var ativos = new Ativo[]
            {
                new Ativo { Nome = "Dell Latitude 5420", Patrimonio = "NT-001", Tipo = TipoAtivo.Notebook, Marca = "Dell", Modelo = "Latitude 5420", Setor = "1", Status = StatusAtivo.EmUso, ValorCompra = 6200m, DataCompra = DateTime.UtcNow.AddYears(-2) },
                new Ativo { Nome = "Monitor LG Ultrawide", Patrimonio = "MN-055", Tipo = TipoAtivo.Monitor, Marca = "LG", Modelo = "29WK600", Setor = "4", Status = StatusAtivo.Disponivel, ValorCompra = 1450m, DataCompra = DateTime.UtcNow.AddMonths(-14) },
                new Ativo { Nome = "MacBook Pro M3", Patrimonio = "NT-002", Tipo = TipoAtivo.Notebook, Marca = "Apple", Modelo = "Pro 14", Setor = "7", Status = StatusAtivo.EmUso, ValorCompra = 18500m, DataCompra = DateTime.UtcNow.AddMonths(-8) },
                new Ativo { Nome = "Teclado Mecânico Logitech", Patrimonio = "PE-201", Tipo = TipoAtivo.Periferico, Marca = "Logitech", Modelo = "MX Keys", Setor = "5", Status = StatusAtivo.Manutencao, ValorCompra = 650m, DataCompra = DateTime.UtcNow.AddYears(-1) },
                new Ativo { Nome = "Servidor Dell PowerEdge", Patrimonio = "SRV-01", Tipo = TipoAtivo.Servidor, Marca = "Dell", Modelo = "R750", Setor = "2", Status = StatusAtivo.EmUso, ValorCompra = 42000m, DataCompra = DateTime.UtcNow.AddYears(-3) }
            };

            // Adiciona o array acima no banco
            context.Ativos.AddRange(ativos);

            // Salva as alterações
            context.SaveChanges();
        }
    }
}
