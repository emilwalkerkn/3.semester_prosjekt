CREATE TABLE IF NOT EXISTS Resources (
                                         Id INT AUTO_INCREMENT PRIMARY KEY,
                                         Type VARCHAR(100) NOT NULL,
    Description TEXT NOT NULL,
    Location VARCHAR(255) NOT NULL,
    Latitude VARCHAR(50),
    Longitude VARCHAR(50),
    GeometryType VARCHAR(50),
    GeometryData TEXT
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
    Status VARCHAR(50) NOT NULL DEFAULT 'New'
    );

