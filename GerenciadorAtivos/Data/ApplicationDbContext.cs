using GerenciadorAtivos.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorAtivos.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Ativo> Ativos { get; set; }
        public DbSet<Historico> Historicos { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var historicosParaAdicionar = new List<Historico>();

            foreach (var entry in ChangeTracker.Entries())
            {
                // 1. A REGRA DO SOFT DELETE (Mantida)
                if (entry.Entity is Ativo ativoExcluido && entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    ativoExcluido.IsDeleted = true;
                }

                // 2. A REGRA DA AUDITORIA GRANULAR (O Novo!)
                // Se a entidade for um Ativo, estiver sendo modificada, e não for a exclusão acima...
                if (entry.Entity is Ativo ativoModificado && entry.State == EntityState.Modified && !ativoModificado.IsDeleted)
                {
                    var detalhesAlteracao = new List<string>();

                    // Varre todas as colunas daquele ativo para ver o que mudou
                    foreach (var prop in entry.Properties)
                    {
                        if (prop.IsModified && prop.Metadata.Name != "IsDeleted")
                        {
                            var valorAntigo = prop.OriginalValue?.ToString() ?? "Vazio";
                            var valorNovo = prop.CurrentValue?.ToString() ?? "Vazio";

                            detalhesAlteracao.Add($"{prop.Metadata.Name} alterado de '{valorAntigo}' para '{valorNovo}'");
                        }
                    }

                    // Se encontrou alguma alteração real, cria o registro de histórico
                    if (detalhesAlteracao.Any())
                    {
                        var stringDetalhes = string.Join(" | ", detalhesAlteracao);

                        historicosParaAdicionar.Add(new Historico
                        {
                            // ATENÇÃO: Ajuste esses nomes (AtivoId, Acao, Detalhes) se a sua classe Historico for um pouco diferente
                            AtivoId = ativoModificado.Id,
                            TipoAcao = "Edição Granular",
                            Descricao = stringDetalhes,
                            DataAcao = DateTime.UtcNow
                            // Nota: O campo 'Usuario' geralmente é preenchido lá no Controller onde você tem acesso ao Identity. 
                            // Se quiser salvar aqui no DbContext, podemos adicionar depois!
                        });
                    }
                }
            }

            // Salva os históricos novos no banco antes de finalizar a transação
            if (historicosParaAdicionar.Any())
            {
                AddRange(historicosParaAdicionar);
            }

            return base.SaveChangesAsync(cancellationToken);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Muito importante manter isso por causa do Identity!

            // Filtro Global: Toda vez que o sistema buscar os ativos, ignore os que estão "deletados"
            builder.Entity<Ativo>().HasQueryFilter(a => !a.IsDeleted);
        }
    }
}