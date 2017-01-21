CREATE TABLE [dbo].[Observations]
(
    [Date] DATE NOT NULL , 
    [Hour] TINYINT NOT NULL, 
    [StationNumber] VARCHAR(12) NOT NULL, 
    [Temperature] DECIMAL(4, 2) NOT NULL, 
    CONSTRAINT [PK_Observations] PRIMARY KEY ([Date], [Hour], [StationNumber])
)
