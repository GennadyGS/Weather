CREATE TABLE [dbo].[Stations]
(
	[Number] INT NOT NULL PRIMARY KEY, 
    [CountryCode] NCHAR(3) NULL, 
    [Locality] NCHAR(256) NULL, 
    [Latitude] DECIMAL(7, 4) NULL, 
    [Longitude] DECIMAL(7, 4) NULL, 
    [Elevation] SMALLINT NULL, 
    [Description] NCHAR(256) NULL
)
