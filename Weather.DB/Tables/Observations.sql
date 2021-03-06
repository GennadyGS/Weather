﻿CREATE TABLE [dbo].[Observations]
(
    [Date] DATE NOT NULL , 
    [Hour] TINYINT NOT NULL, 
    [StationNumber] INT NOT NULL, 
    [RequestTime] DATETIME NOT NULL, 
    [Temperature] DECIMAL(4, 2) NOT NULL, 
    CONSTRAINT [PK_Observations] PRIMARY KEY ([Date], [Hour], [StationNumber]), 
    CONSTRAINT [FK_Observations_To_Stations] FOREIGN KEY ([StationNumber]) REFERENCES [Stations]([Number])
)
