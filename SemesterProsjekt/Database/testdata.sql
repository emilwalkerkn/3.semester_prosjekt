INSERT INTO Organizations
(Name, OrganizationNumber, Status, CreatedAt)
VALUES
    ('Test Kommune', NULL, 'Pending', '2024-01-01 09:00:00'),
    ('Test Politiet', NULL, 'Pending', '2024-12-12 09:00:00'),
    ('Test Brannvesenet', NULL, 'Pending', '2024-12-12 09:00:00'),
    ('Test Sykehuset', NULL, 'Pending', '2024-12-12 09:00:00'),
    ('Test Sivilforsvaret', NULL, 'Pending', '2024-01-01 09:00:00'),
    ('Test Heimevernet', NULL, 'Pending', '2024-01-01 09:00:00'),
    ('Test Vann og avløp', NULL, 'Pending', '2025-05-01 09:00:00'),
    ('Test AKT Transport', NULL, 'Pending', '2025-01-01 09:00:00'),
    ('Test Beredskapsetat', NULL, 'Pending', '2024-12-12 09:00:00'),
    ('Test Havnevesen', NULL, 'Pending', '2024-05-20 09:00:00');

-- ====================================================
-- Oppretter portaladministratoren.
-- ====================================================
INSERT INTO AspNetUsers
(Id, FirstName, LastName,
 UserName, NormalizedUserName,
 Email, NormalizedEmail,
 EmailConfirmed, PhoneNumberConfirmed,
 TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
 OrganizationId)
VALUES
    ('1', 'Portal', 'Administrator',
     'admin@kriseportalen.no', 'ADMIN@KRISEPORTALEN.NO',
     'admin@kriseportalen.no', 'ADMIN@KRISEPORTALEN.NO',
     0, 0, 0, 1, 0,
     NULL);

-- Tildeler administratorrollen til brukeren.
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT '1', Id
FROM AspNetRoles
WHERE NormalizedName = 'ADMIN';


-- ====================================================
-- Oppretter én organisasjonadministrator per testorganisasjon. Totalt 10
-- ====================================================
INSERT INTO AspNetUsers
(Id, FirstName, LastName,
 UserName, NormalizedUserName,
 Email, NormalizedEmail,
 EmailConfirmed, PhoneNumberConfirmed,
 TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
 OrganizationId)
VALUES
    ('2', 'Kari', 'Administrator',
     'kari.admin@kommune.no', 'KARI.ADMIN@KOMMUNE.NO',
     'kari.admin@kommune.no', 'KARI.ADMIN@KOMMUNE.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Kommune')),

    ('3', 'Ola', 'Administrator',
     'ola.admin@politi.no', 'OLA.ADMIN@POLITI.NO',
     'ola.admin@politi.no', 'OLA.ADMIN@POLITI.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Politiet')),

    ('4', 'Per', 'Administrator',
     'per.admin@brann.no', 'PER.ADMIN@BRANN.NO',
     'per.admin@brann.no', 'PER.ADMIN@BRANN.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Brannvesenet')),

    ('5', 'Anne', 'Administrator',
     'anne.admin@sykehus.no', 'ANNE.ADMIN@SYKEHUS.NO',
     'anne.admin@sykehus.no', 'ANNE.ADMIN@SYKEHUS.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Sykehuset')),

    ('6', 'Lars', 'Administrator',
     'lars.admin@sivilforsvaret.no', 'LARS.ADMIN@SIVILFORSVARET.NO',
     'lars.admin@sivilforsvaret.no', 'LARS.ADMIN@SIVILFORSVARET.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Sivilforsvaret')),

    ('7', 'Ingrid', 'Administrator',
     'ingrid.admin@hv.no', 'INGRID.ADMIN@HV.NO',
     'ingrid.admin@hv.no', 'INGRID.ADMIN@HV.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Heimevernet')),

    ('8', 'Erik', 'Administrator',
     'erik.admin@vannogavlop.no', 'ERIK.ADMIN@VANNOGAVLOP.NO',
     'erik.admin@vannogavlop.no', 'ERIK.ADMIN@VANNOGAVLOP.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Vann og avløp')),

    ('9', 'Maria', 'Administrator',
     'maria.admin@akt.no', 'MARIA.ADMIN@AKT.NO',
     'maria.admin@akt.no', 'MARIA.ADMIN@AKT.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test AKT Transport')),

    ('10', 'Jon', 'Administrator',
     'jon.admin@beredskapsetat.no', 'JON.ADMIN@BEREDSKAPSETAT.NO',
     'jon.admin@beredskapsetat.no', 'JON.ADMIN@BEREDSKAPSETAT.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Beredskapsetat')),

    ('11', 'Ida', 'Administrator',
     'ida.admin@havnevesen.no', 'IDA.ADMIN@HAVNEVESEN.NO',
     'ida.admin@havnevesen.no', 'IDA.ADMIN@HAVNEVESEN.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Havnevesen'));

-- Tildeler organisasjonsadministratorrollen til brukerne.
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT Id,
       (SELECT Id
        FROM AspNetRoles
        WHERE NormalizedName = 'ORGANIZATIONADMIN')
FROM AspNetUsers
WHERE Id IN ('2', '3', '4', '5', '6',
             '7', '8', '9', '10', '11');


-- ====================================================
-- Oppretter én behovsaktør per testorganisasjon.
-- ====================================================
INSERT INTO AspNetUsers
(Id, FirstName, LastName,
 UserName, NormalizedUserName,
 Email, NormalizedEmail,
 EmailConfirmed, PhoneNumberConfirmed,
 TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
 OrganizationId)
VALUES
    ('12', 'Nora', 'Hansen',
     'nora.hansen@kommune.no', 'NORA.HANSEN@KOMMUNE.NO',
     'nora.hansen@kommune.no', 'NORA.HANSEN@KOMMUNE.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Kommune')),

    ('13', 'Emil', 'Berg',
     'emil.berg@politi.no', 'EMIL.BERG@POLITI.NO',
     'emil.berg@politi.no', 'EMIL.BERG@POLITI.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Politiet')),

    ('14', 'Sofie', 'Dahl',
     'sofie.dahl@brann.no', 'SOFIE.DAHL@BRANN.NO',
     'sofie.dahl@brann.no', 'SOFIE.DAHL@BRANN.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Brannvesenet')),

    ('15', 'Martin', 'Lunde',
     'martin.lunde@sykehus.no', 'MARTIN.LUNDE@SYKEHUS.NO',
     'martin.lunde@sykehus.no', 'MARTIN.LUNDE@SYKEHUS.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Sykehuset')),

    ('16', 'Emma', 'Solberg',
     'emma.solberg@sivilforsvaret.no', 'EMMA.SOLBERG@SIVILFORSVARET.NO',
     'emma.solberg@sivilforsvaret.no', 'EMMA.SOLBERG@SIVILFORSVARET.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Sivilforsvaret')),

    ('17', 'Henrik', 'Nilsen',
     'henrik.nilsen@hv.no', 'HENRIK.NILSEN@HV.NO',
     'henrik.nilsen@hv.no', 'HENRIK.NILSEN@HV.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Heimevernet')),

    ('18', 'Sara', 'Moen',
     'sara.moen@vannogavlop.no', 'SARA.MOEN@VANNOGAVLOP.NO',
     'sara.moen@vannogavlop.no', 'SARA.MOEN@VANNOGAVLOP.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Vann og avløp')),

    ('19', 'Magnus', 'Vik',
     'magnus.vik@akt.no', 'MAGNUS.VIK@AKT.NO',
     'magnus.vik@akt.no', 'MAGNUS.VIK@AKT.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test AKT Transport')),

    ('20', 'Ingrid', 'Bakke',
     'ingrid.bakke@beredskapsetat.no', 'INGRID.BAKKE@BEREDSKAPSETAT.NO',
     'ingrid.bakke@beredskapsetat.no', 'INGRID.BAKKE@BEREDSKAPSETAT.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Beredskapsetat')),

    ('21', 'Oliver', 'Strand',
     'oliver.strand@havnevesen.no', 'OLIVER.STRAND@HAVNEVESEN.NO',
     'oliver.strand@havnevesen.no', 'OLIVER.STRAND@HAVNEVESEN.NO',
     0, 0, 0, 1, 0,
     (SELECT OrganizationId FROM Organizations WHERE Name = 'Test Havnevesen'));

-- Tildeler behovsaktørrollen til brukerne.
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT Id,
       (SELECT Id
        FROM AspNetRoles
        WHERE NormalizedName = 'BEHOVSAKTØR')
FROM AspNetUsers
WHERE Id IN ('12', '13', '14', '15', '16',
             '17', '18', '19', '20', '21');


-- ====================================================
-- Oppretter elleve ressurseiere.
-- ====================================================
INSERT INTO AspNetUsers
(Id, FirstName, LastName,
 UserName, NormalizedUserName,
 Email, NormalizedEmail,
 EmailConfirmed, PhoneNumberConfirmed,
 TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
 OrganizationId)
VALUES
    ('22', 'Anders', 'Hansen',
     'anders.hansen@eksempel.no', 'ANDERS.HANSEN@EKSEMPEL.NO',
     'anders.hansen@eksempel.no', 'ANDERS.HANSEN@EKSEMPEL.NO',
     0, 0, 0, 1, 0, NULL),

    ('23', 'Line', 'Berg',
     'line.berg@eksempel.no', 'LINE.BERG@EKSEMPEL.NO',
     'line.berg@eksempel.no', 'LINE.BERG@EKSEMPEL.NO',
     0, 0, 0, 1, 0, NULL),

    ('24', 'Thomas', 'Dahl',
     'thomas.dahl@eksempel.no', 'THOMAS.DAHL@EKSEMPEL.NO',
     'thomas.dahl@eksempel.no', 'THOMAS.DAHL@EKSEMPEL.NO',
     0, 0, 0, 1, 0, NULL),

    ('25', 'Julie', 'Lunde',
     'julie.lunde@eksempel.no', 'JULIE.LUNDE@EKSEMPEL.NO',
     'julie.lunde@eksempel.no', 'JULIE.LUNDE@EKSEMPEL.NO',
     0, 0, 0, 1, 0, NULL),

    ('26', 'Morten', 'Solberg',
     'morten.solberg@eksempel.no', 'MORTEN.SOLBERG@EKSEMPEL.NO',
     'morten.solberg@eksempel.no', 'MORTEN.SOLBERG@EKSEMPEL.NO',
     0, 0, 0, 1, 0, NULL),

    ('27', 'Hanne', 'Nilsen',
     'hanne.nilsen@eksempel.no', 'HANNE.NILSEN@EKSEMPEL.NO',
     'hanne.nilsen@eksempel.no', 'HANNE.NILSEN@EKSEMPEL.NO',
     0, 0, 0, 1, 0, NULL),

    ('28', 'Petter', 'Moen',
     'petter.moen@eksempel.no', 'PETTER.MOEN@EKSEMPEL.NO',
     'petter.moen@eksempel.no', 'PETTER.MOEN@EKSEMPEL.NO',
     0, 0, 0, 1, 0, NULL),

    ('29', 'Silje', 'Vik',
     'silje.vik@eksempel.no', 'SILJE.VIK@EKSEMPEL.NO',
     'silje.vik@eksempel.no', 'SILJE.VIK@EKSEMPEL.NO',
     0, 0, 0, 1, 0, NULL),

    ('30', 'Daniel', 'Bakke',
     'daniel.bakke@eksempel.no', 'DANIEL.BAKKE@EKSEMPEL.NO',
     'daniel.bakke@eksempel.no', 'DANIEL.BAKKE@EKSEMPEL.NO',
     0, 0, 0, 1, 0, NULL),

    ('31', 'Elise', 'Strand',
     'elise.strand@eksempel.no', 'ELISE.STRAND@EKSEMPEL.NO',
     'elise.strand@eksempel.no', 'ELISE.STRAND@EKSEMPEL.NO',
     0, 0, 0, 1, 0, NULL),

    ('32', 'Fredrik', 'Holm',
     'fredrik.holm@eksempel.no', 'FREDRIK.HOLM@EKSEMPEL.NO',
     'fredrik.holm@eksempel.no', 'FREDRIK.HOLM@EKSEMPEL.NO',
     0, 0, 0, 1, 0, NULL);

-- Tildeler ressurseierrollen til brukerne.
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT Id,
       (SELECT Id
        FROM AspNetRoles
        WHERE NormalizedName = 'RESSURSEIER')
FROM AspNetUsers
WHERE Id IN ('22', '23', '24', '25', '26', '27',
             '28', '29', '30', '31', '32');



-- ====================================================
-- Oppretter 30 ressurser.
-- ====================================================
    -- Quantity viser gjenværende mengde etter de ti fullførte testmatchene.
    -- Ressurser uten gjenværende mengde er satt til Unavailable.
    
-- Felles kartposisjon for testressursene i Kristiansand.
SET @testlokasjon =
    '{"type":"Feature","properties":{},"geometry":{"type":"Point","coordinates":[7.9956,58.1467]}}';

-- 30 ressurser.
-- AvailableFrom brukes som registreringsdato i oppgaven.
INSERT INTO Resources
(UserId, ResourceTypeId, Title, Description,
 GeometryType, GeometryData,
 AvailableFrom, Quantity, Unit, Status)
VALUES
    -- Ressurseier 22: 6 ressurser.
    ('22', 3, 'Test: Drikkevann på flasker',
     'Flaskevann til utdeling ved evakuering.',
     'marker', @testlokasjon,
     '2024-02-10 09:00:00', 0, 'flasker', 'Unavailable'),

    ('22', 3, 'Test: Hermetikkpakker',
     'Matpakker med hermetikk som kan lagres uten kjøling.',
     'marker', @testlokasjon,
     '2025-01-12 09:00:00', 100, 'pakker', 'Available'),

    ('22', 3, 'Test: Tørrmat',
     'Tørrmat til personer ved et evakueringssenter.',
     'marker', @testlokasjon,
     '2025-03-15 09:00:00', 150, 'porsjoner', 'Available'),

    ('22', 3, 'Test: Middagsporsjoner',
     'Ferdige middagsporsjoner som kan varmes opp.',
     'marker', @testlokasjon,
     '2025-06-10 09:00:00', 80, 'porsjoner', 'Available'),

    ('22', 3, 'Test: Energibarer',
     'Energibarer til innsatspersonell.',
     'marker', @testlokasjon,
     '2025-08-20 09:00:00', 300, 'stk', 'Available'),

    ('22', 3, 'Test: Vannbeholdere',
     'Fylte vannbeholdere med 20 liter drikkevann hver.',
     'marker', @testlokasjon,
     '2025-11-05 09:00:00', 10, 'beholdere', 'Available'),

    -- Ressurseier 23: 6 ressurser.
    ('23', 3, 'Test: Matpakker til evakuerte',
     'Ferdigsmurte matpakker til utdeling.',
     'marker', @testlokasjon,
     '2024-04-12 09:00:00', 120, 'matpakker', 'Available'),

    ('23', 1, 'Test: Minibuss',
     'Minibuss med plass til åtte passasjerer.',
     'marker', @testlokasjon,
     '2024-09-18 09:00:00', 0, 'kjøretøy', 'Unavailable'),

    ('23', 1, 'Test: Varebil',
     'Varebil til transport av forsyninger.',
     'marker', @testlokasjon,
     '2025-02-10 09:00:00', 0, 'kjøretøy', 'Unavailable'),

    ('23', 1, 'Test: Firehjulstrekker',
     'Firehjulstrekker til transport på krevende veier.',
     'marker', @testlokasjon,
     '2025-05-15 09:00:00', 1, 'kjøretøy', 'Available'),

    ('23', 1, 'Test: Lastebil',
     'Lastebil til transport av større mengder utstyr.',
     'marker', @testlokasjon,
     '2025-09-10 09:00:00', 1, 'kjøretøy', 'Available'),

    ('23', 1, 'Test: Personbil',
     'Personbil til transport av personell.',
     'marker', @testlokasjon,
     '2026-01-15 09:00:00', 0, 'kjøretøy', 'Unavailable'),

    -- Ressurseier 24: 6 ressurser.
    ('24', 1, 'Test: Traktor',
     'Traktor til rydding og transport.',
     'marker', @testlokasjon,
     '2024-06-20 09:00:00', 1, 'kjøretøy', 'Available'),

    ('24', 4, 'Test: Frivillige til utdeling',
     'Frivillige som kan dele ut mat og drikke.',
     'marker', @testlokasjon,
     '2024-11-10 09:00:00', 6, 'personer', 'Available'),

    ('24', 4, 'Test: Frivillige til evakuering',
     'Frivillige som kan bistå ved evakuering.',
     'marker', @testlokasjon,
     '2025-01-20 09:00:00', 0, 'personer', 'Unavailable'),

    ('24', 4, 'Test: Sjåfører',
     'Sjåfører som kan bidra med transport.',
     'marker', @testlokasjon,
     '2025-04-12 09:00:00', 1, 'personer', 'Available'),

    ('24', 4, 'Test: Mannskap til rydding',
     'Mannskap som kan rydde mindre hindringer.',
     'marker', @testlokasjon,
     '2025-10-18 09:00:00', 5, 'personer', 'Available'),

    ('24', 4, 'Test: Førstehjelpspersonell',
     'Personell med førstehjelpsopplæring.',
     'marker', @testlokasjon,
     '2026-02-12 09:00:00', 2, 'personer', 'Available'),

    -- Ressurseier 25: 5 ressurser.
    ('25', 2, 'Test: Førstehjelpsutstyr',
     'Førstehjelpsvesker med bandasjer og sårutstyr.',
     'marker', @testlokasjon,
     '2024-12-20 09:00:00', 5, 'vesker', 'Available'),

    ('25', 2, 'Test: Hjertestartere',
     'Bærbare hjertestartere til utlån.',
     'marker', @testlokasjon,
     '2025-03-25 09:00:00', 1, 'stk', 'Available'),

    ('25', 5, 'Test: Overnattingsplasser i forsamlingshus',
     'Soveplasser med tilgang til toalett.',
     'marker', @testlokasjon,
     '2025-07-10 09:00:00', 30, 'plasser', 'Available'),

    ('25', 5, 'Test: Overnattingsplasser i gjestehus',
     'Sengeplasser til midlertidig innkvartering.',
     'marker', @testlokasjon,
     '2025-12-15 09:00:00', 12, 'plasser', 'Available'),

    ('25', 6, 'Test: Strømaggregat',
     'Mobilt strømaggregat til midlertidig strømforsyning.',
     'marker', @testlokasjon,
     '2026-03-10 09:00:00', 0, 'stk', 'Unavailable'),

    -- Ressurseier 26: 2 ressurser.
    ('26', 6, 'Test: Arbeidslamper',
     'Bærbare arbeidslamper til innsatsområder.',
     'marker', @testlokasjon,
     '2026-04-15 09:00:00', 8, 'stk', 'Available'),

    ('26', 7, 'Test: Varme tepper',
     'Tepper til evakuerte og innsatspersonell.',
     'marker', @testlokasjon,
     '2026-05-12 09:00:00', 50, 'stk', 'Available'),

    -- Ressurseier 27: 1 ressurs.
    ('27', 7, 'Test: Vinterbekledning',
     'Pakker med jakke, lue og hansker.',
     'marker', @testlokasjon,
     '2026-06-10 09:00:00', 20, 'pakker', 'Available'),

    -- Ressurseier 28: 1 ressurs.
    ('28', 8, 'Test: Sambandsradioer',
     'Bærbare radioer for lokal kommunikasjon.',
     'marker', @testlokasjon,
     '2026-07-15 09:00:00', 10, 'stk', 'Available'),

    -- Ressurseier 29: 1 ressurs.
    ('29', 9, 'Test: Hygienepakker',
     'Pakker med såpe, tannbørste og andre hygieneartikler.',
     'marker', @testlokasjon,
     '2026-08-12 09:00:00', 100, 'pakker', 'Available'),

    -- Ressurseier 30: 1 ressurs.
    ('30', 10, 'Test: Verktøysett',
     'Sett med håndverktøy til mindre reparasjoner.',
     'marker', @testlokasjon,
     '2026-09-10 09:00:00', 5, 'sett', 'Available'),

    -- Ressurseier 31: 1 ressurs.
    ('31', 99, 'Test: Oppbevaringskasser',
     'Kasser til sortering og oppbevaring av forsyninger.',
     'marker', @testlokasjon,
     '2026-10-15 09:00:00', 25, 'stk', 'Available');

-- Ressurseier 32 har ikke registrert noen ressurser.


-- ====================================================
-- Opppretter 20 behov
-- ====================================================
    -- Quantity viser gjenværende behov etter de fullførte testmatchene.

-- Felles kartposisjon for testbehovene i Kristiansand.
SET @testlokasjon =
    '{"type":"Feature","properties":{},"geometry":{"type":"Point","coordinates":[7.9956,58.1467]}}';

--  20 behov.
INSERT INTO Needs
(UserId, NeedTypeId, Title, Description,
 GeometryType, GeometryData,
 NeededFrom, Deadline, Quantity, Unit,
 Status, Priority)
VALUES
    -- Test Kommune.
    ('2', 1, 'Test: Drikkevann til evakueringssenter',
     'Behov for drikkevann til personer som er evakuert.',
     'marker', @testlokasjon,
     '2025-02-01 09:00:00', '2025-02-01 18:00:00',
     0, 'flasker', 'Resolved', 'Critical'),

    ('12', 5, 'Test: Midlertidige overnattingsplasser',
     'Behov for soveplasser ved evakuering.',
     'marker', @testlokasjon,
     '2026-03-15 16:00:00', '2026-03-15 22:00:00',
     20, 'plasser', 'In progress', 'High'),

    -- Test Politiet.
    ('3', 2, 'Test: Varebil til utstyrstransport',
     'Behov for en varebil til transport av beredskapsutstyr.',
     'marker', @testlokasjon,
     '2025-06-01 09:00:00', '2025-06-01 15:00:00',
     0, 'kjøretøy', 'Resolved', 'Normal'),

    ('13', 8, 'Test: Radioer til lokal koordinering',
     'Behov for bærbare radioer under en planlagt øvelse.',
     'marker', @testlokasjon,
     '2026-08-01 09:00:00', '2026-08-03 09:00:00',
     6, 'stk', 'New', 'Low'),

    -- Test Brannvesenet.
    ('4', 4, 'Test: Mannskap til evakuering',
     'Behov for personell som kan bistå evakuerte.',
     'marker', @testlokasjon,
     '2025-05-10 10:00:00', '2025-05-10 12:00:00',
     0, 'personer', 'Resolved', 'Critical'),

    ('14', 10, 'Test: Verktøy til rydding',
     'Behov for håndverktøy til rydding etter uvær.',
     'marker', @testlokasjon,
     '2026-09-15 09:00:00', '2026-09-15 18:00:00',
     3, 'sett', 'In progress', 'High'),

    -- Test Sykehuset.
    ('5', 3, 'Test: Førstehjelpsvesker',
     'Behov for ekstra førstehjelpsutstyr ved et mottakssted.',
     'marker', @testlokasjon,
     '2025-03-01 09:00:00', '2025-03-02 09:00:00',
     0, 'vesker', 'Resolved', 'Normal'),

    ('15', 7, 'Test: Tepper til beredskapslager',
     'Behov for tepper til et planlagt beredskapslager.',
     'marker', @testlokasjon,
     '2026-06-01 09:00:00', '2026-06-10 09:00:00',
     30, 'stk', 'New', 'Low'),

    -- Test Sivilforsvaret.
    ('6', 6, 'Test: Strøm til mottakssted',
     'Behov for et aggregat etter strømbrudd.',
     'marker', @testlokasjon,
     '2026-03-20 08:00:00', '2026-03-20 12:00:00',
     0, 'stk', 'Resolved', 'Critical'),

    ('16', 4, 'Test: Frivillige til utdeling av forsyninger',
     'Behov for frivillige som kan dele ut mat og vann.',
     'marker', @testlokasjon,
     '2026-04-15 09:00:00', '2026-04-15 15:00:00',
     5, 'personer', 'In progress', 'High'),

    -- Test Heimevernet.
    ('7', 2, 'Test: Transport av personell',
     'Behov for en minibuss til transport av personell.',
     'marker', @testlokasjon,
     '2025-04-01 09:00:00', '2025-04-02 09:00:00',
     0, 'kjøretøy', 'Resolved', 'Normal'),

    ('17', 1, 'Test: Mat til beredskapsøvelse',
     'Behov for matpakker til en planlagt øvelse.',
     'marker', @testlokasjon,
     '2026-10-20 09:00:00', '2026-10-22 09:00:00',
     50, 'matpakker', 'New', 'Low'),

    -- Test Vann og avløp.
    ('8', 1, 'Test: Reserveforsyning av drikkevann',
     'Behov for drikkevann ved avbrudd i vannforsyningen.',
     'marker', @testlokasjon,
     '2025-12-01 08:00:00', '2025-12-01 12:00:00',
     0, 'beholdere', 'Resolved', 'Critical'),

    ('18', 6, 'Test: Belysning ved vannstasjon',
     'Behov for arbeidslamper ved en midlertidig vannstasjon.',
     'marker', @testlokasjon,
     '2026-05-15 18:00:00', '2026-05-15 22:00:00',
     4, 'stk', 'In progress', 'High'),

    -- Test AKT Transport.
    ('9', 2, 'Test: Ekstra personbil',
     'Behov for en personbil til transport av personell.',
     'marker', @testlokasjon,
     '2026-01-20 09:00:00', '2026-01-21 09:00:00',
     0, 'kjøretøy', 'Resolved', 'Normal'),

    ('19', 99, 'Test: Oppbevaringskasser ved transportknutepunkt',
     'Behov for kasser til organisering av reserveutstyr.',
     'marker', @testlokasjon,
     '2026-11-01 09:00:00', '2026-11-05 09:00:00',
     10, 'stk', 'New', 'Low'),

    -- Test Beredskapsetat.
    ('10', 3, 'Test: Hjertestarter til mottak',
     'Behov for en hjertestarter ved et midlertidig mottak.',
     'marker', @testlokasjon,
     '2025-07-01 09:00:00', '2025-07-01 12:00:00',
     0, 'stk', 'Resolved', 'Critical'),

    ('20', 9, 'Test: Hygienepakker til evakuerte',
     'Behov for hygieneartikler ved et evakueringssenter.',
     'marker', @testlokasjon,
     '2026-09-20 09:00:00', '2026-09-20 18:00:00',
     60, 'pakker', 'New', 'High'),

    -- Test Havnevesen.
    ('11', 4, 'Test: Sjåfører til forsyningstransport',
     'Behov for sjåfører til transport av forsyninger fra havnen.',
     'marker', @testlokasjon,
     '2025-11-01 09:00:00', '2025-11-02 09:00:00',
     0, 'personer', 'Resolved', 'Normal'),

    ('21', 7, 'Test: Vinterbekledning til beredskapslager',
     'Behov for pakker med jakke, lue og hansker.',
     'marker', @testlokasjon,
     '2026-11-10 09:00:00', '2026-11-20 09:00:00',
     15, 'pakker', 'New', 'Low');


-- ====================================================
-- Oppretter 18 matcher:
-- 10 fullførte, 4 godkjente, 2 foreslåtte og 2 avslåtte.
-- ====================================================

INSERT INTO `Match`
(NeedId, ResourceId, ProposedByUserId,
 AssignedQuantity, AssignedUnit,
 Status, MatchedAt, DeliveryAt, DeliveredAt, ResolvedAt,
 RequestNeedTitle, RequestResourceTitle, Notes)
SELECT
    n.NeedId,
    r.ResourceId,
    n.UserId,
    t.Mengde,
    n.Unit,
    t.Matchstatus,
    t.Foreslaatt,
    n.NeededFrom,
    CASE
        WHEN t.Matchstatus = 'Fullført' THEN t.Avsluttet
        ELSE NULL
        END,
    t.Avsluttet,
    n.Title,
    r.Title,
    'Testmatch for IS-201'
FROM (
         -- Ti fullførte matcher.
         SELECT
             'Test: Drikkevann til evakueringssenter' AS Behov,
             'Test: Drikkevann på flasker' AS Ressurs,
             200 AS Mengde,
             'Fullført' AS Matchstatus,
             '2025-01-31 10:00:00' AS Foreslaatt,
             '2025-02-01 12:00:00' AS Avsluttet

         UNION ALL
         SELECT
             'Test: Varebil til utstyrstransport',
             'Test: Varebil',
             1, 'Fullført',
             '2025-05-31 10:00:00', '2025-06-01 11:00:00'

         UNION ALL
         SELECT
             'Test: Mannskap til evakuering',
             'Test: Frivillige til evakuering',
             4, 'Fullført',
             '2025-05-10 09:00:00', '2025-05-10 11:00:00'

         UNION ALL
         SELECT
             'Test: Førstehjelpsvesker',
             'Test: Førstehjelpsutstyr',
             5, 'Fullført',
             '2025-02-28 10:00:00', '2025-03-01 12:00:00'

         UNION ALL
         SELECT
             'Test: Strøm til mottakssted',
             'Test: Strømaggregat',
             1, 'Fullført',
             '2026-03-19 10:00:00', '2026-03-20 10:00:00'

         UNION ALL
         SELECT
             'Test: Transport av personell',
             'Test: Minibuss',
             1, 'Fullført',
             '2025-03-31 10:00:00', '2025-04-01 12:00:00'

         UNION ALL
         SELECT
             'Test: Reserveforsyning av drikkevann',
             'Test: Vannbeholdere',
             10, 'Fullført',
             '2025-11-30 10:00:00', '2025-12-01 10:00:00'

         UNION ALL
         SELECT
             'Test: Ekstra personbil',
             'Test: Personbil',
             1, 'Fullført',
             '2026-01-19 10:00:00', '2026-01-20 12:00:00'

         UNION ALL
         SELECT
             'Test: Hjertestarter til mottak',
             'Test: Hjertestartere',
             1, 'Fullført',
             '2025-06-30 10:00:00', '2025-07-01 10:00:00'

         UNION ALL
         SELECT
             'Test: Sjåfører til forsyningstransport',
             'Test: Sjåfører',
             2, 'Fullført',
             '2025-10-31 10:00:00', '2025-11-01 12:00:00'

         -- Fire godkjente matcher.
         UNION ALL
         SELECT
             'Test: Midlertidige overnattingsplasser',
             'Test: Overnattingsplasser i forsamlingshus',
             20, 'Godkjent',
             '2026-03-14 10:00:00', NULL

         UNION ALL
         SELECT
             'Test: Verktøy til rydding',
             'Test: Verktøysett',
             3, 'Godkjent',
             '2026-09-14 10:00:00', NULL

         UNION ALL
         SELECT
             'Test: Frivillige til utdeling av forsyninger',
             'Test: Frivillige til utdeling',
             5, 'Godkjent',
             '2026-04-14 10:00:00', NULL

         UNION ALL
         SELECT
             'Test: Belysning ved vannstasjon',
             'Test: Arbeidslamper',
             4, 'Godkjent',
             '2026-05-14 10:00:00', NULL

         -- To foreslåtte matcher.
         UNION ALL
         SELECT
             'Test: Radioer til lokal koordinering',
             'Test: Sambandsradioer',
             6, 'Foreslått',
             '2026-07-25 10:00:00', NULL

         UNION ALL
         SELECT
             'Test: Tepper til beredskapslager',
             'Test: Varme tepper',
             30, 'Foreslått',
             '2026-05-25 10:00:00', NULL

         -- To afslåtte matcher.
         UNION ALL
         SELECT
             'Test: Mat til beredskapsøvelse',
             'Test: Matpakker til evakuerte',
             50, 'Avslått',
             '2026-10-15 10:00:00', '2026-10-16 10:00:00'

         UNION ALL
         SELECT
             'Test: Vinterbekledning til beredskapslager',
             'Test: Vinterbekledning',
             15, 'Avslått',
             '2026-11-01 10:00:00', '2026-11-02 10:00:00'
     ) AS t
         JOIN Needs AS n
             ON n.Title = t.Behov
            AND n.UserId IN (
                '2', '3', '4', '5', '6', '7', '8', '9', '10', '11',
                '12', '13', '14', '15', '16', '17', '18', '19', '20', '21'
            )
         JOIN Resources AS r
             ON r.Title = t.Ressurs
            AND r.UserId IN (
                '22', '23', '24', '25', '26', '27',
                '28', '29', '30', '31', '32'
            );

-- ====================================================
-- Oppretter én claim per testbruker: 32 rader.
-- ====================================================

INSERT INTO AspNetUserClaims
(UserId, ClaimType, ClaimValue)
SELECT
    Id,
    'testdatasett',
    'IS-201'
FROM AspNetUsers
WHERE Id IN (
             '1', '2', '3', '4', '5', '6', '7', '8',
             '9', '10', '11', '12', '13', '14', '15', '16',
             '17', '18', '19', '20', '21', '22', '23', '24',
             '25', '26', '27', '28', '29', '30', '31', '32'
    );

-- ====================================================
-- Oppretter ti testclaims fordelt på de fire rollene.
-- ====================================================

INSERT INTO AspNetRoleClaims
(RoleId, ClaimType, ClaimValue)
SELECT
    r.Id,
    'testtillatelse',
    t.Tillatelse
FROM (
         SELECT 'ADMIN' AS Rollenavn,
                'bruker.administrer' AS Tillatelse

         UNION ALL
         SELECT 'ADMIN', 'organisasjon.administrer'

         UNION ALL
         SELECT 'ADMIN', 'match.administrer'

         UNION ALL
         SELECT 'ORGANIZATIONADMIN', 'organisasjon.egne_brukere'

         UNION ALL
         SELECT 'ORGANIZATIONADMIN', 'behov.opprett'

         UNION ALL
         SELECT 'ORGANIZATIONADMIN', 'behov.endre_egne'

         UNION ALL
         SELECT 'BEHOVSAKTØR', 'behov.opprett'

         UNION ALL
         SELECT 'BEHOVSAKTØR', 'behov.endre_egne'

         UNION ALL
         SELECT 'RESSURSEIER', 'ressurs.opprett'

         UNION ALL
         SELECT 'RESSURSEIER', 'ressurs.endre_egne'
     ) AS t
         JOIN AspNetRoles AS r
              ON r.NormalizedName = t.Rollenavn;

-- ====================================================
-- Oppretter ti testoppføringer for ekstern innlogging.
-- ====================================================

INSERT INTO AspNetUserLogins
(LoginProvider, ProviderKey, ProviderDisplayName, UserId)
VALUES
    ('Microsoft', 'test-ms-02', 'Microsoft', '2'),
    ('Microsoft', 'test-ms-03', 'Microsoft', '3'),
    ('Microsoft', 'test-ms-04', 'Microsoft', '4'),
    ('Microsoft', 'test-ms-05', 'Microsoft', '5'),
    ('Microsoft', 'test-ms-06', 'Microsoft', '6'),
    ('Microsoft', 'test-ms-07', 'Microsoft', '7'),
    ('Microsoft', 'test-ms-08', 'Microsoft', '8'),
    ('Microsoft', 'test-ms-09', 'Microsoft', '9'),
    ('Google', 'test-google-22', 'Google', '22'),
    ('Google', 'test-google-23', 'Google', '23');

-- ====================================================
-- Oppretter ti testoppføringer for tokens.
-- ====================================================

INSERT INTO AspNetUserTokens
(UserId, LoginProvider, Name, Value)
VALUES
    ('2', 'TestProvider', 'TestToken', 'test-token-02'),
    ('3', 'TestProvider', 'TestToken', 'test-token-03'),
    ('4', 'TestProvider', 'TestToken', 'test-token-04'),
    ('5', 'TestProvider', 'TestToken', 'test-token-05'),
    ('6', 'TestProvider', 'TestToken', 'test-token-06'),
    ('7', 'TestProvider', 'TestToken', 'test-token-07'),
    ('8', 'TestProvider', 'TestToken', 'test-token-08'),
    ('9', 'TestProvider', 'TestToken', 'test-token-09'),
    ('10', 'TestProvider', 'TestToken', 'test-token-10'),
    ('11', 'TestProvider', 'TestToken', 'test-token-11');

-- ====================================================
-- Oppretter ti testinvitasjoner.
-- ====================================================

INSERT INTO OrganizationInvitations
(OrganizationId, Email, TokenHash, InvitedByUserId,
 CreatedAt, ExpiresAt, AcceptedAt, RevokedAt,
 IsOrganizationAdminInvitation)
VALUES
    -- Akseptert invitasjon: Test Kommune.
    ((SELECT OrganizationId
      FROM Organizations WHERE Name = 'Test Kommune'),
     'nora.hansen@kommune.no',
     SHA2('is201-testinvitasjon-01', 256),
     '2',
     '2026-01-05 09:00:00', '2026-01-12 09:00:00',
     '2026-01-06 10:00:00', NULL, 0),

    -- Akseptert invitasjon: Test Politiet.
    ((SELECT OrganizationId
      FROM Organizations WHERE Name = 'Test Politiet'),
     'emil.berg@politi.no',
     SHA2('is201-testinvitasjon-02', 256),
     '3',
     '2026-01-05 09:00:00', '2026-01-12 09:00:00',
     '2026-01-07 11:00:00', NULL, 0),

    -- Tilbakekalt invitasjon: Test Brannvesenet.
    ((SELECT OrganizationId
      FROM Organizations WHERE Name = 'Test Brannvesenet'),
     'test.invitasjon@brann.no',
     SHA2('is201-testinvitasjon-03', 256),
     '4',
     '2026-02-01 09:00:00', '2026-02-08 09:00:00',
     NULL, '2026-02-02 12:00:00', 0),

    -- Tilbakekalt invitasjon: Test Sykehuset.
    ((SELECT OrganizationId
      FROM Organizations WHERE Name = 'Test Sykehuset'),
     'test.invitasjon@sykehus.no',
     SHA2('is201-testinvitasjon-04', 256),
     '5',
     '2026-02-01 09:00:00', '2026-02-08 09:00:00',
     NULL, '2026-02-03 12:00:00', 0),

    -- Utløpt invitasjon: Test Sivilforsvaret.
    ((SELECT OrganizationId
      FROM Organizations WHERE Name = 'Test Sivilforsvaret'),
     'test.invitasjon@sivilforsvaret.no',
     SHA2('is201-testinvitasjon-05', 256),
     '6',
     '2026-03-01 09:00:00', '2026-03-08 09:00:00',
     NULL, NULL, 0),

    -- Utløpt invitasjon: Test Heimevernet.
    ((SELECT OrganizationId
      FROM Organizations WHERE Name = 'Test Heimevernet'),
     'test.invitasjon@hv.no',
     SHA2('is201-testinvitasjon-06', 256),
     '7',
     '2026-03-01 09:00:00', '2026-03-08 09:00:00',
     NULL, NULL, 0),

    -- Utløpt invitasjon: Test Vann og avløp.
    ((SELECT OrganizationId
      FROM Organizations WHERE Name = 'Test Vann og avløp'),
     'test.invitasjon@vannogavlop.no',
     SHA2('is201-testinvitasjon-07', 256),
     '8',
     '2026-03-01 09:00:00', '2026-03-08 09:00:00',
     NULL, NULL, 0),

    -- Åpen invitasjon: Test AKT Transport.
    ((SELECT OrganizationId
      FROM Organizations WHERE Name = 'Test AKT Transport'),
     'test.invitasjon@akt.no',
     SHA2('is201-testinvitasjon-08', 256),
     '9',
     '2026-11-19 09:00:00', '2026-12-01 09:00:00',
     NULL, NULL, 0),

    -- Åpen invitasjon: Test Beredskapsetat.
    ((SELECT OrganizationId
      FROM Organizations WHERE Name = 'Test Beredskapsetat'),
     'test.invitasjon@beredskapsetat.no',
     SHA2('is201-testinvitasjon-09', 256),
     '10',
     '2026-11-19 09:00:00', '2026-12-01 09:00:00',
     NULL, NULL, 0),

    -- Åpen invitasjon: Test Havnevesen.
    ((SELECT OrganizationId
      FROM Organizations WHERE Name = 'Test Havnevesen'),
     'test.invitasjon@havnevesen.no',
     SHA2('is201-testinvitasjon-10', 256),
     '11',
     '2026-11-19 09:00:00', '2026-12-01 09:00:00',
     NULL, NULL, 0);


-- | Tabell | Planlagt antall rader |
-- |---|---:|
-- | `AspNetUserClaims` | 32 |
-- | `AspNetRoleClaims` | 10 |
-- | `AspNetUserLogins` | 10 |
-- | `AspNetUserTokens` | 10 |
-- | `OrganizationInvitations` | 10 |