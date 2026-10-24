-- Gir testkontoen et sted for en sikkert avledet passordhash.
ALTER TABLE AppUsers
    ADD COLUMN IF NOT EXISTS PasswordHash VARCHAR(255) NULL;

INSERT IGNORE INTO AppUsers (
    UserId,
    Username,
    Email,
    FirstName,
    LastName,
    Phone
)
VALUES
    (1, 'testaktor', 'testaktor@example.com', 'Test', 'Aktør', '00000000'),
    (2, 'testtilbyder', 'testtilbyder@example.com', 'Test', 'Tilbyder', '11111111');

INSERT IGNORE INTO PublicActors (
    UserId,
    OrganizationName,
    OrganizationType
)
VALUES
    (1, 'Testkommune', 'Kommune');

INSERT IGNORE INTO ResourceProviders (
    UserId,
    ProviderType,
    OrganizationName
)
VALUES
    (2, 'Frivillig organisasjon', 'Testorganisasjon');

INSERT IGNORE INTO NeedType (
    NeedTypeId,
    Name
)
VALUES
    (1, 'Hjelp etter hendelse');

INSERT IGNORE INTO ResourceType (
    ResourceTypeId,
    Name
)
VALUES
    (1, 'Kjøretøy');

INSERT IGNORE INTO Location (
    LocationId,
    Description,
    Latitude,
    Longitude
)
VALUES
    (1, 'Kristiansand sentrum', '58.1467', '7.9956');

-- Eksempeldata for lokal demonstrasjon av dashboard, kart og oversikter.
INSERT IGNORE INTO ResourceType (ResourceTypeId, Name)
VALUES
    (2, 'Medisinsk utstyr'),
    (3, 'Matforsyning'),
    (4, 'Mannskap');

INSERT IGNORE INTO Location (LocationId, Description, Latitude, Longitude)
VALUES
    (2, 'Vennesla', '58.2690', '8.0048'),
    (3, 'Grimstad', '58.3405', '8.5934');

INSERT IGNORE INTO Needs (
    NeedId, PublicActorId, NeedTypeId, LocationId, Title, Description,
    GeometryType, GeometryData, ContactPoint, NeededFrom, Deadline,
    Quantity, Unit, Status, Priority
)
VALUES
    (101, 1, 3, 1, 'Medisinsk utstyr',
     'Legevakten trenger medisinsk forbruksmateriell til mottak av evakuerte.',
     'marker', '{"type":"Feature","geometry":{"type":"Point","coordinates":[7.9956,58.1467]}}',
     'beredskap@kristiansand.no', '2026-10-24 08:00:00', '2026-10-25 12:00:00',
     120, 'sett', 'New', 'High'),
    (102, 1, 2, 2, 'Transportkapasitet',
     'Det trengs kjøretøy for transport av personell og utstyr mellom Vennesla og Kristiansand.',
     'marker', '{"type":"Feature","geometry":{"type":"Point","coordinates":[8.0048,58.2690]}}',
     'transport@kristiansand.no', '2026-10-24 09:00:00', '2026-10-26 18:00:00',
     4, 'kjøretøy', 'In progress', 'Critical'),
    (103, 1, 1, 3, 'Matforsyninger',
     'Midlertidig mottak trenger mat og drikke til beboere og frivillige.',
     'marker', '{"type":"Feature","geometry":{"type":"Point","coordinates":[8.5934,58.3405]}}',
     'mottak@grimstad.no', '2026-10-24 10:00:00', '2026-10-27 14:00:00',
     350, 'måltider', 'New', 'Normal');

INSERT IGNORE INTO Resources (
    ResourceId, ProviderId, ResourceTypeId, LocationId, Title, Description,
    GeometryType, GeometryData, ContactPoint, AvailableFrom, AvailableTo,
    Quantity, Unit, Status
)
VALUES
    (201, 2, 2, 1, 'Førstehjelpsutstyr',
     'Komplette førstehjelpssett og enklere medisinsk utstyr, klart for utkjøring.',
     'marker', '{"type":"Feature","geometry":{"type":"Point","coordinates":[7.9956,58.1467]}}',
     'vakt@frivillig.no', '2026-10-24 08:00:00', '2026-10-31 18:00:00',
     80, 'sett', 'Available'),
    (202, 2, 1, 2, 'Transportkjøretøy',
     'Varebiler med sjåfør for transport av forsyninger og materiell.',
     'marker', '{"type":"Feature","geometry":{"type":"Point","coordinates":[8.0048,58.2690]}}',
     'transport@frivillig.no', '2026-10-24 08:00:00', '2026-10-28 20:00:00',
     3, 'kjøretøy', 'Available'),
    (203, 2, 3, 3, 'Mat og drikke',
     'Pakkede måltider, vann og tørrvarer til utdeling ved mottak.',
     'marker', '{"type":"Feature","geometry":{"type":"Point","coordinates":[8.5934,58.3405]}}',
     'lager@frivillig.no', '2026-10-24 10:00:00', '2026-10-29 18:00:00',
     500, 'måltider', 'Available');
