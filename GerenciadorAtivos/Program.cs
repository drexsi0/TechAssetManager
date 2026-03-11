using GerenciadorAtivos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Permite que o PostgreSQL aceite datas locais (como o SQL Server fazia)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// --- CONFIGURAÇÃO DE BANCO DE DADOS (POSTGRES SEMPRE) ---
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

// BLINDAGEM: Verifica se não é nulo antes de tentar ler
if (!string.IsNullOrEmpty(connectionString) &&
    (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://")))
{
    var databaseUri = new Uri(connectionString);
    var userInfo = databaseUri.UserInfo.Split(':');
    var port = databaseUri.Port > 0 ? databaseUri.Port : 5432;

    connectionString = $"Server={databaseUri.Host};Port={port};Database={databaseUri.AbsolutePath.TrimStart('/')};User Id={userInfo[0]};Password={userInfo[1]};SslMode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
// --------------------------------------------------------

// 👇 CORREÇÃO: Restaurando as configurações do Identity (Login e Roles) e MVC
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            // Usa o IP do usuário como chave de bloqueio
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100, // Máximo de requisições
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1) // Tempo da janela
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

app.UseRateLimiter();

// --- FORÇA O SISTEMA A USAR O PADRÃO BRASILEIRO (R$, Datas, etc) ---
var defaultCulture = new CultureInfo("pt-BR");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};
app.UseRequestLocalization(localizationOptions);
// -------------------------------------------------------------------

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