using GerenciadorAtivos.Models;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorAtivos.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Ativo> Ativos { get; set; }
    }
}