using SemesterProsjekt.Data;
using SemesterProsjekt.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using SemesterProsjekt.Models;



var builder = WebApplication.CreateBuilder(args);

// Registrerer MVC-tjenestene som brukes av applikasjonen,
// inkludert Controllers og Views.
builder.Services.AddControllersWithViews();

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();

builder.Services.AddScoped<IResourceRepository, ResourceRepository>();

builder.Services.AddScoped<INeedRepository, NeedRepository>();

builder.Services.AddScoped<IAccountRepository, AccountRepository>();

builder.Services.AddSingleton<PasswordService>();

builder.Services.AddScoped<IPasswordHasher<PortalUser>, PortalPasswordHasher>();

// Testversjonen kan kjøre parallelt med originalen på localhost:8080.
// Navnene kan settes i compose.test.yml slik at de to appene ikke deler cookies.
var authenticationCookieName = builder.Configuration["Authentication:CookieName"];
var antiforgeryCookieName = builder.Configuration["Antiforgery:CookieName"];

if (!string.IsNullOrWhiteSpace(antiforgeryCookieName))
{
    builder.Services.AddAntiforgery(options => options.Cookie.Name = antiforgeryCookieName);
}

builder.Services.AddIdentityCore<PortalUser>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddUserStore<PortalUserStore>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";

        if (!string.IsNullOrWhiteSpace(authenticationCookieName))
        {
            options.Cookie.Name = authenticationCookieName;
        }
    });

builder.Services.AddAuthorization();

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

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

// Setter opp standard routing slik at URL-er kobles
// til riktig Controller og Action.
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


// Starter ASP.NET Core-applikasjonen og gjør den klar
// til å motta HTTP-forespørsler.
app.Run();
