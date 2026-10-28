

CREATE TABLE IF NOT EXISTS Users (
                                     UserId INT AUTO_INCREMENT PRIMARY KEY,
                                     Username VARCHAR(80) NOT NULL UNIQUE,
    Email VARCHAR(254) NOT NULL UNIQUE,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Phone VARCHAR(30),
    PasswordHash VARCHAR(255) NOT NULL
    );

CREATE TABLE IF NOT EXISTS Roles (
                                     RoleId INT AUTO_INCREMENT PRIMARY KEY,
                                     Name VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(255)
    );

CREATE TABLE IF NOT EXISTS UserRole (
                                        UserId INT NOT NULL,
                                        RoleId INT NOT NULL,
                                        PRIMARY KEY (UserId, RoleId),

    CONSTRAINT FK_UserRole_User
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
    ON DELETE CASCADE,

    CONSTRAINT FK_UserRole_Role
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
    ON DELETE CASCADE
    );



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
                                     UserId INT NOT NULL,
                                     NeedTypeId INT NOT NULL,

                                     Title VARCHAR(160) NOT NULL,
    Description TEXT NOT NULL,
    GeometryType VARCHAR(50),
    GeometryData TEXT,
    ContactPoint VARCHAR(255),
    NeededFrom DATETIME,
    Deadline DATETIME,
    Quantity DECIMAL(10,2) NOT NULL,
    Unit VARCHAR(50) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'New',
    Priority VARCHAR(20) NOT NULL DEFAULT 'Normal',

    CONSTRAINT FK_Needs_User
    FOREIGN KEY (UserId) REFERENCES Users(UserId),

    CONSTRAINT FK_Needs_NeedType
    FOREIGN KEY (NeedTypeId) REFERENCES NeedType(NeedTypeId)
    );

CREATE TABLE IF NOT EXISTS Resources (
                                         ResourceId INT AUTO_INCREMENT PRIMARY KEY,
                                         UserId INT NOT NULL,
                                         ResourceTypeId INT NOT NULL,

                                         Title VARCHAR(160) NOT NULL,
    Description TEXT NOT NULL,
    GeometryType VARCHAR(50),
    GeometryData TEXT,
    ContactPoint VARCHAR(255),
    AvailableFrom DATETIME,
    AvailableTo DATETIME,
    Quantity DECIMAL(10,2) NOT NULL,
    Unit VARCHAR(50) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Available',

    CONSTRAINT FK_Resources_User
    FOREIGN KEY (UserId) REFERENCES Users(UserId),

    CONSTRAINT FK_Resources_ResourceType
    FOREIGN KEY (ResourceTypeId) REFERENCES ResourceType(ResourceTypeId)
    );


CREATE TABLE IF NOT EXISTS `Match` (
    MatchId INT AUTO_INCREMENT PRIMARY KEY,
    NeedId INT NOT NULL,
    ResourceId INT NOT NULL,
    AssignedQuantity DECIMAL(10,2) NOT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Proposed',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT FK_MatchNeed
        FOREIGN KEY (NeedId) 
        REFERENCES Needs(NeedId),

    CONSTRAINT FK_MatchResource
        FOREIGN KEY (ResourceId) 
        REFERENCES Resources(ResourceId),

    CONSTRAINT UQ_Match_Need_Resource
    UNIQUE (NeedId, ResourceId)
    );


--Testdata--
-- Startverdier for behovstyper
INSERT IGNORE INTO Roles (RoleId, Name, Description) VALUES
    (1, 'Behovsaktør', 'Registrerer og følger opp behov'),
    (2, 'Ressurseier', 'Registrerer og oppdaterer ressurser'),
    (3, 'Admin', 'Administrerer behov, ressurser og matcher');

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




