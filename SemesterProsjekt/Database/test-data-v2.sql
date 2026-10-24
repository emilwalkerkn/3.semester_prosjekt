INSERT INTO Roles (RoleId, Name, Description) VALUES
    (1, 'Behovsaktør', 'Registrerer og følger opp behov'),
    (2, 'Ressurseier', 'Registrerer og oppdaterer ressurser'),
    (3, 'Admin', 'Administrerer alle behov og ressurser');

INSERT INTO Users (UserId, Username, Email, FirstName, LastName, Phone, PasswordHash) VALUES
    (1, 'testaktor', 'testaktor@example.com', 'Test', 'Aktør', '00000000', '100000.XVcKuplPrRs569ODzR/JpQ==.W/2DsMDThcuLtItABZoDBld8Fm3pAv90fUgzC4Q7rCU='),
    (2, 'testtilbyder', 'testtilbyder@example.com', 'Test', 'Tilbyder', '11111111', '100000.XVcKuplPrRs569ODzR/JpQ==.W/2DsMDThcuLtItABZoDBld8Fm3pAv90fUgzC4Q7rCU='),
    (3, 'admin', 'admin@kriseportal.local', 'Test', 'Administrator', '22222222', '100000.XVcKuplPrRs569ODzR/JpQ==.W/2DsMDThcuLtItABZoDBld8Fm3pAv90fUgzC4Q7rCU=');

INSERT INTO UserRole (UserId, RoleId) VALUES (1, 1), (2, 2), (3, 3);

INSERT INTO NeedType (NeedTypeId, Name) VALUES
    (1, 'Mat og drikke'), (2, 'Transport'), (3, 'Medisinsk utstyr'), (4, 'Mannskap'), (5, 'Overnatting');

INSERT INTO ResourceType (ResourceTypeId, Name) VALUES
    (1, 'Kjøretøy'), (2, 'Medisinsk utstyr'), (3, 'Matforsyning'), (4, 'Mannskap');

INSERT INTO Needs (NeedId, UserId, NeedTypeId, Title, Description, GeometryType, GeometryData, ContactPoint, NeededFrom, Deadline, Quantity, Unit, Status, Priority) VALUES
    (101, 1, 3, 'Medisinsk utstyr', 'Legevakten trenger medisinsk forbruksmateriell til mottak av evakuerte.', 'marker', '{"type":"Feature","geometry":{"type":"Point","coordinates":[7.9956,58.1467]}}', 'beredskap@kristiansand.no', '2026-10-24 08:00:00', '2026-10-25 12:00:00', 120, 'sett', 'New', 'High'),
    (102, 1, 2, 'Transportkapasitet', 'Det trengs kjøretøy for transport av personell og utstyr.', 'marker', '{"type":"Feature","geometry":{"type":"Point","coordinates":[8.0048,58.2690]}}', 'transport@kristiansand.no', '2026-10-24 09:00:00', '2026-10-26 18:00:00', 4, 'kjøretøy', 'In progress', 'Critical'),
    (103, 1, 1, 'Matforsyninger', 'Midlertidig mottak trenger mat og drikke til beboere og frivillige.', 'marker', '{"type":"Feature","geometry":{"type":"Point","coordinates":[8.5934,58.3405]}}', 'mottak@grimstad.no', '2026-10-24 10:00:00', '2026-10-27 14:00:00', 350, 'måltider', 'New', 'Normal');

INSERT INTO Resources (ResourceId, UserId, ResourceTypeId, Title, Description, GeometryType, GeometryData, ContactPoint, AvailableFrom, AvailableTo, Quantity, Unit, Status) VALUES
    (201, 2, 2, 'Førstehjelpsutstyr', 'Komplette førstehjelpssett og enklere medisinsk utstyr, klart for utkjøring.', 'marker', '{"type":"Feature","geometry":{"type":"Point","coordinates":[7.9956,58.1467]}}', 'vakt@frivillig.no', '2026-10-24 08:00:00', '2026-10-31 18:00:00', 80, 'sett', 'Available'),
    (202, 2, 1, 'Transportkjøretøy', 'Varebiler med sjåfør for transport av forsyninger og materiell.', 'marker', '{"type":"Feature","geometry":{"type":"Point","coordinates":[8.0048,58.2690]}}', 'transport@frivillig.no', '2026-10-24 08:00:00', '2026-10-28 20:00:00', 3, 'kjøretøy', 'Available'),
    (203, 2, 3, 'Mat og drikke', 'Pakkede måltider, vann og tørrvarer til utdeling ved mottak.', 'marker', '{"type":"Feature","geometry":{"type":"Point","coordinates":[8.5934,58.3405]}}', 'lager@frivillig.no', '2026-10-24 10:00:00', '2026-10-29 18:00:00', 500, 'måltider', 'Available');

-- Et åpent eksempel viser match-flyten når testdatabasen opprettes på nytt.
INSERT INTO `Match` (NeedId, ResourceId, AssignedQuantity, Status) VALUES
    (101, 201, 40, 'Proposed');
