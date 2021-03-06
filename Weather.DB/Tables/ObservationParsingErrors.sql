﻿CREATE TABLE [dbo].[ObservationParsingErrors]
(
    [Date] DATE NOT NULL , 
    [Hour] TINYINT NOT NULL, 
    [StationNumber] INT NOT NULL, 
    [RequestTime] DATETIME NOT NULL, 
    [ErrorText] NVARCHAR(MAX) NOT NULL, 
    CONSTRAINT [PK_ObservationParsingErrors] PRIMARY KEY ([Date], [Hour], [StationNumber])
)
