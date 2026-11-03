-- Fagtabeller. Brukere og roller opprettes av ASP.NET Core Identity.

CREATE TABLE IF NOT EXISTS NeedType (
    NeedTypeId INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(80) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS ResourceType (
    ResourceTypeId INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(80) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS Needs (
    NeedId INT AUTO_INCREMENT PRIMARY KEY,
    UserId VARCHAR(255) NOT NULL,
    NeedTypeId INT NOT NULL,
    Title VARCHAR(160) NOT NULL,
    Description TEXT NOT NULL,
    GeometryType VARCHAR(50) NOT NULL,
    GeometryData TEXT NOT NULL,
    ContactPoint VARCHAR(255),
    NeededFrom DATETIME,
    Deadline DATETIME,
    Quantity DECIMAL(10,2),
    Unit VARCHAR(50),
    Status VARCHAR(50) NOT NULL DEFAULT 'New',
    Priority VARCHAR(20) NOT NULL DEFAULT 'Normal',
    CONSTRAINT FK_Needs_NeedType
        FOREIGN KEY (NeedTypeId) REFERENCES NeedType(NeedTypeId)
);

CREATE TABLE IF NOT EXISTS Resources (
    ResourceId INT AUTO_INCREMENT PRIMARY KEY,
    UserId VARCHAR(255) NOT NULL,
    ResourceTypeId INT NOT NULL,
    Title VARCHAR(160) NOT NULL,
    Description TEXT NOT NULL,
    GeometryType VARCHAR(50) NOT NULL,
    GeometryData TEXT NOT NULL,
    ContactPoint VARCHAR(255),
    AvailableFrom DATETIME,
    AvailableTo DATETIME,
    Quantity DECIMAL(10,2),
    Unit VARCHAR(50),
    Status VARCHAR(50) NOT NULL DEFAULT 'Available',
    CONSTRAINT FK_Resources_ResourceType
        FOREIGN KEY (ResourceTypeId) REFERENCES ResourceType(ResourceTypeId)
);

CREATE TABLE IF NOT EXISTS `Match` (
    MatchId INT AUTO_INCREMENT PRIMARY KEY,
    NeedId INT NOT NULL,
    ResourceId INT NOT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Foreslått',
    MatchedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ResolvedAt DATETIME,
    Notes VARCHAR(500),
    CONSTRAINT FK_Match_Need FOREIGN KEY (NeedId) REFERENCES Needs(NeedId) ON DELETE CASCADE,
    CONSTRAINT FK_Match_Resource FOREIGN KEY (ResourceId) REFERENCES Resources(ResourceId) ON DELETE CASCADE,
    CONSTRAINT UQ_Match_Need_Resource UNIQUE (NeedId, ResourceId)
);

INSERT IGNORE INTO NeedType (NeedTypeId, Name) VALUES
    (1, 'Mat og drikke'),
    (2, 'Transport'),
    (3, 'Medisinsk utstyr'),
    (4, 'Mannskap'),
    (5, 'Overnatting');

INSERT IGNORE INTO ResourceType (ResourceTypeId, Name) VALUES
    (1, 'Kjøretøy'),
    (2, 'Medisinsk utstyr'),
    (3, 'Matforsyning'),
    (4, 'Mannskap');
