CREATE TABLE IF NOT EXISTS Resources (
                                         Id INT AUTO_INCREMENT PRIMARY KEY,
                                         Type VARCHAR(100) NOT NULL,
    Description TEXT NOT NULL,
    Location VARCHAR(255) NOT NULL,
    Latitude VARCHAR(50),
    Longitude VARCHAR(50),
    GeometryType VARCHAR(50),
    GeometryData TEXT,
    ContactPoint VARCHAR(255),
    AvailableFrom DATETIME NULL,
    AvailableTo DATETIME NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Available'
    );

CREATE TABLE IF NOT EXISTS Needs (
                                     Id INT AUTO_INCREMENT PRIMARY KEY,
                                     Type VARCHAR(100) NOT NULL,
    Description TEXT NOT NULL,
    Location VARCHAR(255) NOT NULL,
    Latitude VARCHAR(50),
    Longitude VARCHAR(50),
    GeometryType VARCHAR(50),
    GeometryData TEXT,
    Status VARCHAR(50) NOT NULL DEFAULT 'New',
    Priority VARCHAR(20) NOT NULL DEFAULT 'Normal',
    ContactPoint VARCHAR(255),
    NeededFrom DATETIME NULL,
    Deadline DATETIME NULL
    );


CREATE TABLE IF NOT EXISTS NeedType (
                                        need_type_id SMALLINT AUTO_INCREMENT PRIMARY KEY,
                                        name VARCHAR(80) NOT NULL UNIQUE
    );

CREATE TABLE IF NOT EXISTS ResourceType (
                                            resource_type_id SMALLINT AUTO_INCREMENT PRIMARY KEY,
                                            name VARCHAR(80) NOT NULL UNIQUE
    );

CREATE TABLE IF NOT EXISTS `Match` (
                                       match_id INT AUTO_INCREMENT PRIMARY KEY,
                                       need_id INT NOT NULL,
                                       resource_id INT NOT NULL,
                                       assigned_quantity DECIMAL(10,2) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Proposed',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_match_need
    FOREIGN KEY (need_id) REFERENCES Needs(Id),

    CONSTRAINT fk_match_resource
    FOREIGN KEY (resource_id) REFERENCES Resources(Id),

    UNIQUE (need_id, resource_id)
    );
