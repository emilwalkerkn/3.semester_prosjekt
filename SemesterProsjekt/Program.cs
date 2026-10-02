using SemesterProsjekt.Data;



var builder = WebApplication.CreateBuilder(args);

// Registrerer MVC-tjenestene som brukes av applikasjonen,
// inkludert Controllers og Views.
builder.Services.AddControllersWithViews();

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