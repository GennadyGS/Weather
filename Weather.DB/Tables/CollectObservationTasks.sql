CREATE TABLE [dbo].[CollectObservationTasks]
(
    [StationNumberMask] VARCHAR(8) NOT NULL PRIMARY KEY, 
    [CollectStartDate] DATETIME NULL, 
    [CollectIntervalHours] SMALLINT NOT NULL DEFAULT 3
)
