-- Oppretter fagtabellene for behov, ressurser og matcher.
-- Brukere og roller opprettes separat av ASP.NET Core Identity
-- gjennom Entity Framework Core-migreringer.
--
-- Docker kjører filen ved første initialisering av en tom database.
-- CREATE TABLE IF NOT EXISTS oppretter manglende tabeller,
-- men endrer ikke strukturen til eksisterende tabeller.


-- Lagrer kategoriene som brukes ved registrering av behov.
-- Primærnøkkelen genereres automatisk, og typenavnet må være unikt.
CREATE TABLE IF NOT EXISTS NeedType (
                                        NeedTypeId INT AUTO_INCREMENT PRIMARY KEY,
                                        Name VARCHAR(80) NOT NULL UNIQUE
    );


-- Lagrer kategoriene som brukes ved registrering av ressurser.
CREATE TABLE IF NOT EXISTS ResourceType (
                                            ResourceTypeId INT AUTO_INCREMENT PRIMARY KEY,
                                            Name VARCHAR(80) NOT NULL UNIQUE
    );


-- Lagrer behov, kontaktinformasjon, kartgeometri og oppfølgingsstatus.
-- UserId inneholder Identity-ID-en til brukeren som registrerte behovet.
-- Denne filen definerer ikke en fremmednøkkel fra UserId til AspNetUsers.
CREATE TABLE IF NOT EXISTS Needs (
                                     NeedId INT AUTO_INCREMENT PRIMARY KEY,
                                     UserId VARCHAR(255) NOT NULL,
    NeedTypeId INT NOT NULL,

    Title VARCHAR(160) NOT NULL,
    Description TEXT NOT NULL,

    -- GeometryType angir punkt eller linje.
    -- GeometryData inneholder plasseringen som GeoJSON.
    GeometryType VARCHAR(50) NOT NULL,
    GeometryData TEXT NOT NULL,

    ContactPoint VARCHAR(255),

    -- Valgfrie tidspunkt for når behovet oppstår og må være dekket.
    NeededFrom DATETIME,
    Deadline DATETIME,

    -- Mengde og enhet er valgfrie.
    -- DECIMAL(10,2) tillater ti sifre totalt, hvorav to etter desimaltegnet.
    Quantity DECIMAL(10,2),
    Unit VARCHAR(50),

    -- Standardverdiene brukes dersom verdiene utelates ved innsetting.
    Status VARCHAR(50) NOT NULL DEFAULT 'New',
    Priority VARCHAR(20) NOT NULL DEFAULT 'Normal',

    -- Fremmednøkkelen sikrer at behovet viser til en eksisterende behovstype.
    CONSTRAINT FK_Needs_NeedType
    FOREIGN KEY (NeedTypeId) REFERENCES NeedType(NeedTypeId)
    );


-- Lagrer ressurser, eier, kartgeometri og tilgjengelighet.
-- UserId inneholder ressurseierens Identity-ID.
-- Denne filen definerer ikke en fremmednøkkel fra UserId til AspNetUsers.
CREATE TABLE IF NOT EXISTS Resources (
                                         ResourceId INT AUTO_INCREMENT PRIMARY KEY,
                                         UserId VARCHAR(255) NOT NULL,
    ResourceTypeId INT NOT NULL,

    Title VARCHAR(160) NOT NULL,
    Description TEXT NOT NULL,

    -- Samme geometriformat brukes for både behov og ressurser.
    GeometryType VARCHAR(50) NOT NULL,
    GeometryData TEXT NOT NULL,

    ContactPoint VARCHAR(255),

    -- Valgfrie tidspunkt som beskriver ressursens tilgjengelighet.
    AvailableFrom DATETIME,
    AvailableTo DATETIME,

    Quantity DECIMAL(10,2),
    Unit VARCHAR(50),
    Status VARCHAR(50) NOT NULL DEFAULT 'Available',

    -- Fremmednøkkelen sikrer at ressursen har en eksisterende ressurstype.
    CONSTRAINT FK_Resources_ResourceType
    FOREIGN KEY (ResourceTypeId) REFERENCES ResourceType(ResourceTypeId)
    );


-- Kobler ett behov til én ressurs per match.
-- Et behov kan ha flere matcher, og en ressurs kan inngå i flere matcher.
-- Tabellnavnet skrives med backticks fordi MATCH også brukes i SQL-syntaks.
CREATE TABLE IF NOT EXISTS `Match` (
                                       MatchId INT AUTO_INCREMENT PRIMARY KEY,
                                       NeedId INT NOT NULL,
                                       ResourceId INT NOT NULL,
    -- Identifiserer forslagsstilleren slik at riktig motpart kan svare.
    -- NULL beholdes som mulighet for eldre matcher uten kjent opphavsperson.
    ProposedByUserId VARCHAR(255),
    -- Reserveres ved godkjenning og leveres ved fullføring.
    AssignedQuantity DECIMAL(10,2),
    AssignedUnit VARCHAR(50),
    DeliveryAt DATETIME,
    DeliveredAt DATETIME,
    DeliveryLocation VARCHAR(255),
    DeliveryContact VARCHAR(255),
    RequestNeedTitle VARCHAR(160),
    RequestResourceTitle VARCHAR(160),

    -- Nye matcher får status Foreslått dersom status ikke oppgis.
                                       Status VARCHAR(20) NOT NULL DEFAULT 'Foreslått',

    -- Registreringstidspunktet får en standardverdi fra databaseserveren
    -- dersom applikasjonen ikke sender inn et tidspunkt.
    MatchedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Avslutningstidspunkt og merknader er valgfrie.
    ResolvedAt DATETIME,
    Notes VARCHAR(500),

    -- Matchen må vise til et eksisterende behov.
    -- Hvis behovet slettes, slettes også tilhørende matcher.
    CONSTRAINT FK_Match_Need
    FOREIGN KEY (NeedId) REFERENCES Needs(NeedId)
    ON DELETE CASCADE,

    -- Matchen må vise til en eksisterende ressurs.
    -- Hvis ressursen slettes, slettes også tilhørende matcher.
    CONSTRAINT FK_Match_Resource
    FOREIGN KEY (ResourceId) REFERENCES Resources(ResourceId)
    ON DELETE CASCADE,

    -- Aktive duplikater kontrolleres under radlås i MatchRepository.
    INDEX IX_Match_Need_Resource (NeedId, ResourceId)
    );


-- Legger inn faste behovstyper som brukes i registreringsskjemaet.
-- INSERT IGNORE hopper over rader som kolliderer med eksisterende
-- primærnøkler eller unike typenavn. Eksisterende rader oppdateres ikke.
INSERT IGNORE INTO NeedType (NeedTypeId, Name) VALUES
    (1, 'Mat og drikke'),
    (2, 'Kjøretøy'),
    (3, 'Medisinsk utstyr'),
    (4, 'Mannskap'),
    (5, 'Overnatting'),
    (6, 'Strøm og belysning'),
    (7, 'Varme og bekledning'),
    (8, 'Kommunikasjon'),
    (9, 'Hygiene og sanitær'),
    (10, 'Verktøy og maskiner'),
    (99, 'Annet');


-- Legger inn faste ressurstyper som brukes i registreringsskjemaet.
INSERT IGNORE INTO ResourceType (ResourceTypeId, Name) VALUES
    (1, 'Kjøretøy'),
    (2, 'Medisinsk utstyr'),
    (3, 'Mat og drikke'),
    (4, 'Mannskap'),
    (5, 'Overnatting'),
    (6, 'Strøm og belysning'),
    (7, 'Varme og bekledning'),
    (8, 'Kommunikasjon'),
    (9, 'Hygiene og sanitær'),
    (10, 'Verktøy og maskiner'),
    (99, 'Annet');
