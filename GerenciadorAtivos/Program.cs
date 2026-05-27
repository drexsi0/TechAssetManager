using GerenciadorAtivos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Permite que o PostgreSQL aceite datas locais (como o SQL Server fazia)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// --- CONFIGURAÃ‡ÃƒO DE BANCO DE DADOS (POSTGRES SEMPRE) ---
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

// BLINDAGEM: Verifica se nÃ£o Ã© nulo antes de tentar ler
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

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddControllersWithViews();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            // Usa o IP do usuÃ¡rio como chave de bloqueio
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100, // MÃ¡ximo de requisiÃ§Ãµes
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1) // Tempo da janela
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

app.UseRateLimiter();

// --- FORÃ‡A O SISTEMA A USAR O PADRÃƒO BRASILEIRO (R$, Datas, etc) ---
var defaultCulture = new CultureInfo("pt-BR");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};
app.UseRequestLocalization(localizationOptions);
// -------------------------------------------------------------------

// --- MIGRAÃ‡ÃƒO AUTOMÃTICA (CRIA O BANCO NA NUVEM) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    // Se tiver mudanÃ§as pendentes no banco, aplica agora!
    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
    }

    await EnsureAssetResponsavelSchemaAsync(context);
}
// ---------------------------------------------------

// --- INÃCIO DOS SEEDS (Popula o banco inicialmente) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // 1. Seed de Ativos e HistÃ³ricos Iniciais
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

    // 2. Seed de Perfis (Roles) e UsuÃ¡rio Admin
    try
    {
        await GerenciadorAtivos.Data.SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Um erro ocorreu ao criar os perfis de usuÃ¡rio.");
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

// ðŸ‘‡ CORREÃ‡ÃƒO: Authentication DEVE vir antes do Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// ObrigatÃ³rio para as telas de Login/Registro funcionarem
app.MapRazorPages();

app.Run();

static async Task EnsureAssetResponsavelSchemaAsync(ApplicationDbContext context)
{
    await context.Database.ExecuteSqlRawAsync("""
        ALTER TABLE "Ativos"
        ADD COLUMN IF NOT EXISTS "ResponsavelId" text;
        """);

    await context.Database.ExecuteSqlRawAsync("""
        CREATE INDEX IF NOT EXISTS "IX_Ativos_ResponsavelId"
        ON "Ativos" ("ResponsavelId");
        """);

    await context.Database.ExecuteSqlRawAsync("""
        DO $$
        BEGIN
            IF NOT EXISTS (
                SELECT 1
                FROM pg_constraint
                WHERE conname = 'FK_Ativos_AspNetUsers_ResponsavelId'
            ) THEN
                ALTER TABLE "Ativos"
                ADD CONSTRAINT "FK_Ativos_AspNetUsers_ResponsavelId"
                FOREIGN KEY ("ResponsavelId")
                REFERENCES "AspNetUsers" ("Id")
                ON DELETE SET NULL;
            END IF;
        END $$;
        """);
}
