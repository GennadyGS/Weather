namespace Weather.IntegraionTests

open System
open Xunit
open Swensen.Unquote
open Weather.Model
open Weather.Persistence
open Weather.IntegrationTests
open Weather.Utils

type DbServiceTests() =
    inherit DbTests()
    
    let connectionstring = Settings.ConnectionStrings.Weather
    let currentTime = System.DateTime.UtcNow
    let getSampleObservation stationNumber (observationDateTime : DateTime) = 
        { Header = 
            { ObservationTime = 
                { Date = observationDateTime.Date
                  Hour = byte(observationDateTime.Hour) }
              StationNumber = stationNumber }
          Temperature = -1.3m }

    let sortObservations observations = 
        observations |> (List.sortBy (fun o -> o.Header))

    let testSaveObservations observations = 
        DbService.insertObservationList connectionstring observations |> ignore
        
        let result = DbService.getObservations connectionstring
        
        let expectedResult = observations |> sortObservations |> Success
        expectedResult =! (result |> Result.map sortObservations)

    let roundDateTimeToHours (dateTime : DateTime) = 
        dateTime.Date.AddHours(float dateTime.Hour)

    [<Fact>]
    let ``SaveObservations should save empty list of observation correctly``() = 
        testSaveObservations []

    [<Fact>]
    let ``SaveObservations should save single observation correctly``() = 
        testSaveObservations [ getSampleObservation (StationNumber 0) currentTime ]

    [<Fact>]
    let ``GetLastObservationTimeListForStations should return empty list when there are no observations``() = 
        let now = System.DateTime.UtcNow;
        let requestedStationNumbers = [StationNumber 0; StationNumber 10; StationNumber 100]
        let interval = 
            { From = currentTime.AddDays(-1.0)
              To = currentTime }
        
        let result = DbService.getLastObservationTimeListForStations connectionstring interval requestedStationNumbers

        let expectedResult = 
            requestedStationNumbers 
            |> List.map (fun stNumber -> (stNumber, None))
            |> Success
        expectedResult =! result

    [<Fact>]
    let ``GetLastObservationTimeListForStations should return correct last observation time list when there are two observations``() = 
        let savedStationNumberList = [StationNumber 1; StationNumber 2]
        let requestedStationNumberList = StationNumber 0 :: savedStationNumberList
        let interval = 
            { From = currentTime.AddDays(-1.0)
              To = currentTime }
        let observationTime = currentTime.AddHours(-1.0)
        let observationList = 
            savedStationNumberList 
            |> List.map (fun stNumber -> getSampleObservation stNumber observationTime)
        
        DbService.insertObservationList connectionstring observationList |> ignore
        
        let result = DbService.getLastObservationTimeListForStations connectionstring interval requestedStationNumberList

        let expectedResult = 
            requestedStationNumberList
            |> List.map (
                function 
                | stNumber when List.contains stNumber savedStationNumberList -> 
                    (stNumber, Some (roundDateTimeToHours observationTime))
                | stNumber -> (stNumber, None))
            |> Success
        expectedResult =! (result |> Result.map (List.sortBy fst))
