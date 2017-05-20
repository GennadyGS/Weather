MERGE INTO Stations AS Target 
USING (VALUES 
	(0, NULL, NULL, 'Test station 0'), 
	(1, NULL, NULL, 'Test station 1'), 
	(2, NULL, NULL, 'Test station 2'), 
	(3, NULL, NULL, 'Test station 3'), 
	(4, NULL, NULL, 'Test station 4'), 
	(5, NULL, NULL, 'Test station 5'), 
	(6, NULL, NULL, 'Test station 6'), 
	(7, NULL, NULL, 'Test station 7'), 
	(8, NULL, NULL, 'Test station 8'), 
	(9, NULL, NULL, 'Test station 9'), 
	(33345, 'UKR', 'Kiev', NULL)
) 
AS Source (Number, CountryCode, Locality, Description) 
ON Target.Number = Source.Number
WHEN MATCHED THEN 
UPDATE SET 
	Number = Source.Number,
	CountryCode = Source.CountryCode,
    Locality = Source.Locality,
    Description = Source.Description
WHEN NOT MATCHED BY TARGET THEN 
INSERT (Number, CountryCode, Locality, Description) 
VALUES (Source.Number, Source.CountryCode, Source.Locality, Source.Description)
WHEN NOT MATCHED BY SOURCE THEN 
DELETE;