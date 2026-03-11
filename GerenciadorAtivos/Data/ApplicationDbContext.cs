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
            // Varre todas as classes 'Ativo' que estão na fila para serem alteradas
            foreach (var entry in ChangeTracker.Entries<Ativo>())
            {
                // Se alguém clicou em excluir...
                if (entry.State == EntityState.Deleted)
                {
                    // ...nós mentimos para o banco! Trocamos para 'Modificado' e marcamos como deletado.
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                }
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