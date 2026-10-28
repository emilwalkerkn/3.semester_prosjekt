using Microsoft.AspNetCore.Identity;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models.Identity;


// Registrerer MVC-tjenestene som brukes av applikasjonen,
// inkludert Controllers og Views.
var builder = WebApplication.CreateBuilder(args);

/*
builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
*/


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<INeedRepository, NeedRepository>();


var app = builder.Build();


// Konfigurerer HTTP-forespørselsflyten.
// Ved kjøring utenfor utviklingsmiljø brukes en egen feilhåndteringsside
// og HSTS for sikrere HTTPS-tilkoblinger.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    
    // Standardverdien for HSTS er 30 dager.
    // Denne kan endres ved behov for produksjonsmiljø.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.MapStaticAssets();

// Setter opp standard routing slik at URL-er kobles
// til riktig Controller og Action.
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
// Starter ASP.NET Core-applikasjonen og gjør den klar
// til å motta HTTP-forespørsler.
app.Run();