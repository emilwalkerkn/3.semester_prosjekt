using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using Microsoft.AspNetCore.DataProtection;
using SemesterProsjekt.Services;


// Oppretter en builder som brukes til å konfigurere
// tjenestene og innstillingene i applikasjonen.

var builder = WebApplication.CreateBuilder(args);


// Henter tilkoblingsstrengen fra konfigurasjonen.
// Programmet avbrytes dersom tilkoblingsstrengen ikke finnes.

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' ble ikke funnet.");


// Registrerer ApplicationDbContext og kobler EF Core
// til MariaDB-databasen som applikasjonen bruker.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MariaDbServerVersion(new Version(10, 11, 0))));


// Konfigurerer ASP.NET Core Identity for brukere og roller.
// Her bestemmes kravene til passord, e-post og kontolåsing.

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = false;

        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// Angir hvilke sider brukeren sendes til når brukeren
// ikke er innlogget eller ikke har riktig tilgang.

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});


// Legger til støtte for MVC-controllere, views og Razor Pages.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


// Registrerer databasetilkoblingen og repositoryene.
// Dapper brukes til å hente og lagre behov,
// ressurser og matcher i databasen.

builder.Services.AddSingleton<
    IDbConnectionFactory,
    MySqlConnectionFactory>();

builder.Services.AddScoped<
    IResourceRepository,
    ResourceRepository>();

builder.Services.AddScoped<
    INeedRepository,
    NeedRepository>();

builder.Services.AddScoped<
    IMatchRepository,
    MatchRepository>();


// I produksjonsmiljøet lagres nøklene for databeskyttelse i en egen mappe, slik at de bevares ved omstart.

builder.Services.AddScoped<InvitationEmailSender>();

if (!builder.Environment.IsDevelopment())
{
    builder.Services
        .AddDataProtection()
        .PersistKeysToFileSystem(
            new DirectoryInfo("/keys"))
        .SetApplicationName("SemesterProsjekt"); 
}

builder.Services.AddScoped<InvitationEmailSender>();



// Bygger applikasjonen med tjenestene
// og innstillingene som er registrert.

var app = builder.Build();


// Oppretter et eget tjenesteområde og henter databasen.
// Ventende EF Core-migrasjoner kjøres ved oppstart.
await DomainSchemaMigrator.MigrateAsync(app.Services);

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await database.Database.MigrateAsync();
}


// Oppretter nødvendige roller, oppdaterer domeneskjemaet
// og legger inn lokale eksempeldata dersom det er konfigurert.

await IdentitySeeder.SeedRolesAsync(app.Services);
if (app.Configuration.GetValue<bool>(
        "ApprovalAdminTestSeeder:Enabled"))
{
    if (!app.Environment.IsDevelopment())
        throw new InvalidOperationException(
            "Testseederen kan bare kjøre i Development.");
}

await CategorySeeder.SeedAsync(app.Services);
await LocalDemoSeeder.SeedAsync(
    app.Services,
    app.Configuration);


// I produksjonsmiljøet sendes uventede feil til feilsiden.
// HSTS ber nettleseren bruke HTTPS ved senere besøk.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


// Sender HTTP-forespørsler videre til HTTPS
// og finner riktig rute for hver forespørsel.

app.UseHttpsRedirection();
app.UseRouting();


// Kontrollerer hvem brukeren er og hva brukeren
// har tilgang til før forespørselen behandles.

app.UseAuthentication();
app.UseAuthorization();


// Gjør statiske filer som CSS, JavaScript og bilder tilgjengelige.

app.MapStaticAssets();


// Setter opp standardruten i applikasjonen.
// Innloggingssiden åpnes dersom ingen annen rute er oppgitt.

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();


// Gjør Razor Pages tilgjengelige i applikasjonen.

app.MapRazorPages();


// Starter applikasjonen og begynner å ta imot forespørsler.

app.Run();
