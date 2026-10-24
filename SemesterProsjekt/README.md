# Kriseportal

Kriseportal er en ASP.NET Core MVC-applikasjon for å registrere behov og tilgjengelige ressurser ved en hendelse. Løsningen bruker MariaDB, Docker og cookie-basert innlogging.

## Funksjonalitet

- Registrering av nye brukere med valgt brukertype
- Innlogging og utlogging
- Roller for `Behovsaktør` og `Ressurseier`
- Registrering av behov og ressurser
- Ressursoversikt med kart
- Redigering og sletting av egne ressurser
- Innstillinger for kontaktopplysninger og passord

## Starte testmiljøet

Krav: Docker Desktop må kjøre.

Kjør kommandoen fra denne mappen:

```bash
docker compose -f compose.test.yml up --build -d
```

Åpne deretter [http://localhost:8081](http://localhost:8081).

Testmiljøet kjører med egen Docker-port og egen MariaDB-volume. Det påvirker ikke en eventuell originalversjon som kjører på port 8080.

Stopp testmiljøet uten å slette data:

```bash
docker compose -f compose.test.yml down
```

## Testkontoer

| Brukernavn | Passord | Rolle |
| --- | --- | --- |
| `testaktor` | `testpassord` | Behovsaktør |
| `testtilbyder` | `testpassord` | Ressurseier |
| `admin` | `testpassord` | Admin |

Nye kontoer kan opprettes fra lenken **Opprett bruker** på innloggingssiden.

## Systemarkitektur

```text
Nettleser
    |
    v
ASP.NET Core MVC
Controllers -> Views -> Dapper-repositories
    |
    v
MariaDB i Docker
```

Applikasjonen følger MVC-mønsteret:

- **Controllers** mottar HTTP-forespørsler og velger view eller neste handling.
- **Views** er Razor-sider som lager HTML-grensesnittet.
- **Models** beskriver dataene som brukes i applikasjonen.
- **Data** inneholder repositories som bruker Dapper for å lese og skrive i MariaDB.

Databasen opprettes fra `Database/init-v2.sql`. Testdata og testkontoer ligger i `Database/test-data-v2.sql`.

## Identitet og sikkerhet

Løsningen bruker ASP.NET Core Identitys `UserManager` sammen med en egen MariaDB-basert user store.

- Passord lagres som PBKDF2-hash med SHA-256 og tilfeldig salt.
- Brukernavn og e-postadresse er unike i databasen.
- Innlogging lagres i en HTTP-only cookie.
- Alle skjema som endrer data bruker antiforgery-token.
- Anonyme brukere sendes til innlogging.
- `Ressurseier` kan opprette, endre eller slette egne ressurser. Admin kan administrere alle ressurser.
- `Admin` kan administrere alle registrerte behov og ressurser.
- En behovsaktør kan foreslå en match for eget behov. Ressurseieren kan godta eller avslå forslaget.
- Match-funksjonen hindrer duplikater og overbooking av mengde i databasen.
- SQL-spørringer bruker parametere gjennom Dapper.
- Oppdatering og sletting av ressurser sjekker både rolle og eier (`UserId`) i databasen.

## Testscenarier og resultater

| Type | Scenario | Resultat per 24.10.2026 |
| --- | --- | --- |
| Byggtest | Bygge ASP.NET Core-prosjektet i Docker | Bestått uten warnings eller errors |
| Enhetstest | Controller-tester for registrering av behov og ressurser, samt eierskap ved ressursredigering og sletting | Bestått lokalt |
| Systemtest | Registrere ny bruker og lagre rolle i MariaDB | Bestått |
| Systemtest | Innlogging med ny konto og tilgang til portal | Bestått |
| Sikkerhetstest | Ressurseier oppretter og sletter egen ressurs | Bestått |
| Sikkerhetstest | En annen ressurseier prøver å redigere ressursen | Avvist med tilgangsnekt-side |
| Sikkerhetstest | Behovsaktør prøver å åpne ressursredigering | Avvist |
| Sikkerhetstest | En behovsaktør prøver å lage match for en annens behov | Avvist |
| Enhetstest | Match opprettes med innlogget bruker og ressurseier godtar | Bestått lokalt |
| Brukervennlighetstest | Test siden på mobil og noter funn | Må gjennomføres før innlevering |

Kjør enhetstestene fra løsningen:

```bash
dotnet test SemesterProsjekt.UnitTests/SemesterProsjekt.UnitTests.csproj
```

Før innlevering må gruppen også dokumentere hvem som testet mobilversjonen, hvilke oppgaver de utførte og hvilke forbedringer dere gjorde etter testen.

## Bruk av KI

Gruppen har brukt ChatGPT/Codex som støtte gjennom utviklingen. KI ble brukt til å:

- Tolke Figma-designet og foreslå CSS- og Razor-struktur.
- Forklare ASP.NET Core Identity, autentisering, roller og password hashing.
- Lage utkast til kode for controllers, view models, repositories og tester.
- Foreslå testscenarier og forbedre dokumentasjon.

Eksempler på prompts som ble brukt:

```text
Implementer hele Figma-designet i ASP.NET Core MVC-prosjektet.
Bruk presentasjonen om identitetshåndtering og lag en login-side som matcher resten.
La oss fikse registrering av ny bruker.
```

Gruppen har gjennomgått, tilpasset og testet koden lokalt. KI har ikke blitt brukt til å pushe, commite eller gjøre endringer direkte i GitHub-repositoryet.

## Videre arbeid før innlevering

- Legg til og dokumenter tester for ressursredigering og sletting i enhetstestprosjektet.
- Gjennomfør mobil- og brukervennlighetstest med reelle testbrukere.
- Legg inn skjermbilder eller et diagram som viser den ferdige arkitekturen og Docker-oppsettet.
- Kryssjekk alle brukerfunksjoner mot oppdragsgivers krav.
