
# IS-202 Programmeringsprosjekt

 
## Om prosjektet
Dette prosjektet er en ASP.NET Core MVC-applikasjon basert på caset om en ressurs- og behovsportal for Totalforsvaret. Formålet med løsningen er å støtte registreringen og delingen av informasjon om behov og ressurser under krise og større hendelser.
I denne versjonen av applikasjonen er hovedfokuset frontend, registrering av behov og ressurser, dataoverføring i Leaflet/kart.

 
## Teknologier
| Teknologi | Bruk i prosjektet |
|---|---|
| ASP.NET Core MVC | Rammeverk for webapplikasjonen. |
| C# | Programmeringsspråk for backend. |
| Razor Views | Brukes til å lage og vise nettsidene. |
| JavaScript | Brukes til interaksjon med Leaflet-kartet. |
| Bootstrap | Brukes til styling og responsivt design. |
| Leaflet | Brukes til kart og geografiske koordinater. |
| Docker | Brukes til å kjøre applikasjonen i en container. |
| Git | Brukes til versjonskontroll. |
| GitHub | Brukes til lagring av Git-repository og samarbeid i gruppen. |


## Drift
Prosjektet er en ASP.NET Core MVC applikasjon som kan kjøres i Docker. Prosjektet inneholder en Dockerfile og en compose.yml som brukes til å bygge og starte applikasjonen. Vi bruker Git til versjonskontroll og Github til å dele prosjektet og samarbeide i gruppen. Git registrerer endringer i koden gjennom commits. Dette gjør det mulig for oss å følge utviklingen og se hvem som har gjort endringer og hente frem tidligere versjoner ved behov. 
 
**Krav**
For å kjøre prosjektet trenger man Git og Docker installert. Repositoryet kan klones fra GitHub, og applikasjonen startes fra mappen som inneholder compose.yml.
 
**Kommando for å starte applikasjonen:**
´´´bash
docker compose up –build
´´´

Når containeren er startet, er applikasjonen tilgjengelig på http://localhost:8080. 
**Applikasjonen kan stoppes med:**
´´´bash
docker compose down
´´´


## Systemarkitektur
Vi bruker MVC-arkitektur (Model-View-Controller). Dette gjør at vi kan skille mellom håndtering av forespørsler, data og det brukeren ser på nettsiden.

| Del | Bruk i prosjektet |
|---|---|
| Controller | HomeController.cs håndterer GET- og POST-forespørsler for behov og ressurser. |
| ViewModel | NeedViewModel og ResourceViewModel inneholder data fra skjemaene, inkludert koordinater. |
| View | Razor Views brukes til skjemaene og visning av registrert data. |
| Leaflet | Brukes til å vise kart og hente koordinater når brukeren klikker på kartet. |
| Docker | Brukes til å bygge og kjøre applikasjonen i en container. |

 
**Dataflyt**
Dataflyten for registrering av behov er: brukeren fyller ut skjemaet → POST sendes til HomeController → dataen bindes til NeedViewModel → brukeren sendes til NeedResult, hvor informasjonen vises. Ressursregistreringen fungerer på samme måte, men dataene bindes til ResourceViewModel og brukeren sendes til ResourceResult.
**Bruker → Skjema → POST → HomeController → NeedViewModel → NeedResult → Bruker**

Kartet er koblet opp til skjemaet med JavaScript. Når brukeren klikker på kartet, lagres latitude og longitude i ViewModel-en. Disse koordinatene blir deretter sendt videre og vises på resultatsiden.


## Testscenarioer og resultater
Applikasjonen er testet gjennom både enhetstester og manuelle tester. Enhetstestene brukes til å kontrollere deler av controller-funksjonaliteten, og de manuelle testene brukes til å kontrollere funksjonaliteter i applikasjonen som brukeren møter. For hver test dokumenteres det forventede resultatet og det faktiske resultatet.

### Enhetstester
| # | Test | Forventet resultat | Resultat |
|---|---|---|---|
| 1 | TestNeedGet | Need returnerer en ViewResult med NeedViewModel | Bestått |
| 2 | TestNeedPost | Need videresender til NeedResult | Bestått |
| 3 | TestResourceGet | Resource returnerer en ViewResult med ResourceViewModel | Bestått |
| 4 | TestResourcePost | Resource videresender til ResourceResult | Bestått |
| 5 | TestNeedGetError | Testen forventer feilaktig ResourceViewModel i stedet for NeedViewModel | Feilet som forventet |
| 6 | TestResourcePostError | Testen forventer feilaktig videresending til NeedResult i stedet for ResourceResult | Feilet som forventet |

 
### Forklaring av enhetstestene
For å forklare enhetstestingene i mer detalj har vi valgt å ta utgangspunkt i fire tester: 
Test 1 (TestNeedGet)
Test 4 (TestResourcePost) 
Test 5 (TestNeedGetError)
Test 6 (TestResourcePostError)  

Test 1 og 4 er valgt fordi de to testene dekker både GET- og POST-forespørsler, samt funksjonaliteten for  både Need og Resource.
Test 5 og 6 forklares fordi testene med vilje er skrevet med feil ønsket resultat. Dette er for å demonstrere at enhetstestene oppdager avvik. 



#### Test 1  - TestNeedGet
TestNeedGet (‘NeedGet_ReturnsViewWithNeedViewModel’)
```csharp
var controller = new HomeController();
var result = controller.Need();
var viewResult = Assert.IsType<ViewResult>(result);
Assert.IsType<NeedViewModel>(viewResult.Model);
```

**Hva skjer:**
Testen oppretter en HomeController og kaller på GET-metoden Need(). Need() lager en ny og tom NeedViewModel og returnerer View(model).
Den første sjekken kontrollerer at resultatet er et ViewResult. Slik kontrollerer vi at metoden faktisk returnerer et view.
Den andre sjekken kontrollerer at modellen som sendes til viewet er av typen NeedViewModel. Testen blir da bestått siden dette stemmer med hva Need() returnerer.

**Hensikt med testen:**
Testen kontrollerer at GET-metoden  Need() returnerer et view med riktig ViewModel. Hvis metoden hadde returnert feil type resultat eller feil type modell, ville testen ikke vært bestått og vi kunne se den konkrete feilen og fikse det.



#### Test 4 - TestRescourcePost
TestResourcePost (‘ResourcePost_RedirectsToResourceResult’)
```csharp
car controller = new HomeController();
var model = new ResourceViewModel
{Type = "Brannbil",
Description = "Brannbil med mannskap",
Location = "Kristiansand",
Latitude = "58.1467",
Longitude = "7.9956"};
var result = controller.Resource(model);
var redirectResult = Assert.IsType<RedirectToActionResult>(result);
Assert.Equal("ResourceResult", redirectResult.ActionName);
```

**Hva skjer:**
Testen oppretter en HomeController og en ResourceViewModel med testdata. ResourceViewModel inneholder type, beskrivelse, sted, latitude og longitude. Modellen representerer data som kan bli sendt inn fra ressursskjemaet.
Så kalles den første POST-metoden Resource(model), hvor modellen sendes inn som parameter.
Den første sjekken kontrollerer at resultatet er et RedirectToActionResult. Dette betyr at controlleren skal sende brukeren videre til en action etter at skjemaet er sendt.
Den andre sjekken kontrollerer at brukeren faktisk sendes videre til ResourceResult. Testen består fordi Resource() sender brukeren videre til ResourceResult.

**Hensikt med testen:**
Testen kontrollerer at POST-metoden for ressursregistrering sender brukeren videre til riktig resultatside. Hvis Resource() hadde sendt brukeren til feil action ville testen feilet og vi kunne ha sett feilen og fikset det.


#### Test 5 og 6 - TestNeedGetError og TestResourcePostError
TestNeedGetError (`NeedGet_ShouldFail`)
```csharp
var controller = new HomeController();
var result = controller.Need();

var viewResult = Assert.IsType<ViewResult>(result);   // Består
Assert.IsType<ResourceViewModel>(viewResult.Model);   // Feiler
```

**Hva skjer:**
Testen oppretter en HomeController og kaller på GET-metoden Need(). Need lager så en ny, tom NeedViewModel og returnerer View(model).
Den første sjekken (at resultatet er en ViewResult) består fordi metoden faktisk returnerer et view. 
Den andre sjekken forventer derimot at modellen er av typen ResourceViewModel. Dette er bevisst feil, siden Need() returnerer en NeedViewModel. Testen feiler derfor på denne sjekken.
Assert.IsType<T>() kontrollerer at objektet er nøyaktig av den angitte typen. NeedViewModel og ResourceViewModel har de samme egenskapene, men de er to separate klasser.

**Hensikt med testen:**
Testen viser at enhetstesten oppdager det dersom feil type ViewModel blir returnert. Hvis Need() bruker feil modell vil testen feile og gjøre oss oppmerksomme på det. 





### Manuelle tester
I tillegg til enhetstestene har vi gjennomført manuelle tester av applikasjonen. Dette gjorde vi for å kontrollere funksjonalitet som brukeren møter i nettleseren. Testene ble gjennomført ved å bruke applikasjonen som en vanlig bruker og sammenligne det forventede resultatet med det faktiske resultatet.

| # | Test | Forventet resultat | Resultat |
|---|---|---|---|
| 1 | Åpne startsiden | Startsiden vises og brukeren kan velge behov og ressurs | Bestått |
| 2 | Registrere behov | Utfylt behov vises på resultatsiden | Bestått |
| 3 | Velge posisjon på kart | Markør vises og latitude/longitude fylles inn | Bestått |
| 4 | Registrere ressurs | Utfylt ressurs og koordinater vises på resultatsiden | Bestått |
| 5 | Mobilvisning 400 px bredde | Navigasjon, skjema, resultatsider og kart tilpasser seg skjermbredden uten feil | Bestått |







## Dokumentasjon i koden
Vi bruker korte kommentarer på sentrale klasser og metoder for å forklare hva koden gjør. Fokuset ligger spesielt på controller-metodene og ViewModel-klassene. Dokumentasjonen i koden er kort og forklarer hensikten med sentrale deler av løsningen, og forklarer ikke hver enkelt linje.

## Bruk av KI i prosjektet
I dette prosjektet har vi hovedsakelig brukt KI-verktøyet ChatGPT som et hjelpemiddel i utviklingen. KI har i hovedsak blitt brukt til korreksjon av syntaksfeil, rydde opp i og forkorte unødvendig kode, og gi forslag til hvordan koden kan bli bedre strukturert. I tillegg har vi brukt KI for å få bedre forståelse av enkelte konsepter underveis. Løsninger og faglige vurderinger i prosjektet er våre egne.

### Eksempler på prompts
- "Kan dette sammenlignes med OOP i C#? Om det går, forklar i mens du sammenligner"
- "Forkalr hvordan Model, View og Controller fungerer i MVC, og hvordan de henger sammen"
- "Kan du forklare GitHub-workflows? Steg for steg slik vi jobber mest mulig effektivt og ungår konflikt"
- "Kan du vise hvordan jeg kan konvertere tabellene fra google docs dokumentet slik det ser riktgi ut i README-filen?"
- "Hva er feil her? Hvorfor er det grå farge i teksten på Rider?"
- "Skal enhetstestene ligge i samme fil som applikasjonen? Eller i samme prosjekt-mappe som applikasjonen?"

