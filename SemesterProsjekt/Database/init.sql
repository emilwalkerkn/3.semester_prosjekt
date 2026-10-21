



CREATE TABLE IF NOT EXISTS AppUsers (
    UserId INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(80) NOT NULL UNIQUE,
    Email VARCHAR(254) NOT NULL UNIQUE,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Phone VARCHAR(30)
    );

CREATE TABLE IF NOT EXISTS NeedType (
    NeedTypeId INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(80) NOT NULL UNIQUE
    );

CREATE TABLE IF NOT EXISTS ResourceType (
    ResourceTypeId INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(80) NOT NULL UNIQUE
    );

CREATE TABLE IF NOT EXISTS Location (
    LocationId INT AUTO_INCREMENT PRIMARY KEY,
    Description TEXT NOT NULL,
    Latitude VARCHAR(50),
    Longitude VARCHAR(50)
);
CREATE TABLE IF NOT EXISTS PublicActors (
    UserId INT PRIMARY KEY,
    OrganizationName VARCHAR(160) NOT NULL,
    OrganizationType VARCHAR(50) NOT NULL,

    CONSTRAINT FK_PublicActors_AppUsers
    FOREIGN KEY (UserId)
    REFERENCES AppUsers(UserId)
    ON DELETE CASCADE
    );

CREATE TABLE IF NOT EXISTS ResourceProviders (
    UserId INT PRIMARY KEY,
    ProviderType VARCHAR(50) NOT NULL,
    OrganizationName VARCHAR(160),

    CONSTRAINT FK_ResourceProviders_AppUsers
    FOREIGN KEY (UserId)
    REFERENCES AppUsers(UserId)
    ON DELETE CASCADE

    );

CREATE TABLE IF NOT EXISTS Needs (
    NeedId INT AUTO_INCREMENT PRIMARY KEY,
    PublicActorId INT NOT NULL,
    NeedTypeId INT NOT NULL,
    LocationId INT NOT NULL,

    Title VARCHAR(160) NOT NULL,
    Description TEXT NOT NULL,
    GeometryType VARCHAR(50),
    GeometryData TEXT,
    ContactPoint VARCHAR(255),
    
    NeededFrom DATETIME NULL,
    Deadline DATETIME NULL,
    Quantity DECIMAL (10,2) NOT NULL,
    Unit VARCHAR(50) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'New',
    Priority VARCHAR(20) NOT NULL DEFAULT 'Normal',
    
    CONSTRAINT FK_Needs_PublicActors
        FOREIGN KEY (PublicActorId)
        REFERENCES PublicActors(UserId),
    
    CONSTRAINT FK_Needs_NeedType
        FOREIGN KEY (NeedTypeId)
        REFERENCES NeedType(NeedTypeId),
    
    CONSTRAINT FK_Needs_Location
        FOREIGN KEY (LocationId)
        REFERENCES Location(LocationId)
    );

CREATE TABLE IF NOT EXISTS Resources (
    ResourceId INT AUTO_INCREMENT PRIMARY KEY,
    ProviderId INT NOT NULL,
    ResourceTypeId INT NOT NULL,
    LocationId INT NOT NULL,

    Title VARCHAR(160) NOT NULL,
    Description TEXT NOT NULL,
    GeometryType VARCHAR(50),
    GeometryData TEXT,
    ContactPoint VARCHAR(255),
    
    AvailableFrom DATETIME NULL,
    AvailableTo DATETIME NULL,
    Quantity DECIMAL(10,2) NOT NULL,
    Unit VARCHAR(50) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Available',
    
    CONSTRAINT FK_Resources_ResourceProviders
        FOREIGN KEY (ProviderId)
        REFERENCES ResourceProviders(UserId),
    
    CONSTRAINT FK_Resources_ResourceType
        FOREIGN KEY (ResourceTypeId)
        REFERENCES ResourceType(ResourceTypeId),
    
    CONSTRAINT FK_Resources_Location
        FOREIGN KEY (LocationId)
        REFERENCES Location(LocationId)
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





