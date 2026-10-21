INSERT INTO AppUsers (
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

INSERT INTO PublicActors (
    UserId,
    OrganizationName,
    OrganizationType
)
VALUES
    (1, 'Testkommune', 'Kommune');

INSERT INTO ResourceProviders (
    UserId,
    ProviderType,
    OrganizationName
)
VALUES
    (2, 'Frivillig organisasjon', 'Testorganisasjon');

INSERT INTO NeedType (
    NeedTypeId,
    Name
)
VALUES
    (1, 'Hjelp etter hendelse');

INSERT INTO ResourceType (
    ResourceTypeId,
    Name
)
VALUES
    (1, 'Kjøretøy');

INSERT INTO Location (
    LocationId,
    Description,
    Latitude,
    Longitude
)
VALUES
    (1, 'Kristiansand sentrum', '58.1467', '7.9956');