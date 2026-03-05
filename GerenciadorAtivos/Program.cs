using GerenciadorAtivos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURAÇÃO DE BANCO DE DADOS (HÍBRIDO: LOCAL vs NUVEM) ---
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    // ESTAMOS NA NUVEM -> USAR POSTGRES
    try
    {
        string pgConnectionString;

        // Verifica se a URL está no formato de link (postgres://)
        if (databaseUrl.StartsWith("postgres://") || databaseUrl.StartsWith("postgresql://"))
        {
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');

            // O PULO DO GATO: Se a porta não vier na URL, força a padrão (5432)
            var port = databaseUri.Port > 0 ? databaseUri.Port : 5432;

            pgConnectionString = $"Server={databaseUri.Host};Port={port};Database={databaseUri.AbsolutePath.TrimStart('/')};User Id={userInfo[0]};Password={userInfo[1]};SslMode=Require;Trust Server Certificate=true";
        }
        else
        {
            // Se já vier no formato C# pronto, usa direto
            pgConnectionString = databaseUrl;
        }

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(pgConnectionString));

        Console.WriteLine("--> Usando Banco Postgres (Nuvem)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao configurar Postgres: {ex.Message}");
    }
}
else
{
    // ESTAMOS LOCAL -> USAR SQL SERVER
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

    Console.WriteLine("--> Usando SQL Server (Local)");
}
// -----------------------------------------------------------

// 👇 CORREÇÃO: Restaurando as configurações do Identity (Login e Roles) e MVC
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
// 👆 FIM DA CORREÇÃO

var app = builder.Build();

// --- MIGRAÇÃO AUTOMÁTICA (CRIA O BANCO NA NUVEM) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    // Se tiver mudanças pendentes no banco, aplica agora!
    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
    }
}
// ---------------------------------------------------

// --- INÍCIO DOS SEEDS (Popula o banco inicialmente) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // 1. Seed de Ativos e Históricos Iniciais
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Um erro ocorreu ao popular o banco de dados com ativos.");
    }

    // 2. Seed de Perfis (Roles) e Usuário Admin
    try
    {
        await GerenciadorAtivos.Data.SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Um erro ocorreu ao criar os perfis de usuário.");
    }
}
// ---------------------------------------------------

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 👇 CORREÇÃO: Authentication DEVE vir antes do Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Obrigatório para as telas de Login/Registro funcionarem
app.MapRazorPages();

app.Run();