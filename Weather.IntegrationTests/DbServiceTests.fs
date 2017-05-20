namespace Weather.IntegraionTests

open System
open Xunit
open Swensen.Unquote
open Weather.Model
open Weather.Persistence
open Weather.IntegrationTests
open Weather.Utils
open Weather.Utils.Database
open Weather.Utils.DateTime

type DbServiceTests() =
    inherit DbTests()
    
    let connectionstring = Settings.ConnectionStrings.Weather

    let currentTime = DateTime.UtcNow

    let getSampleObservation stationNumber (observationDateTime : DateTime) = 
        let roundedObservationTime = roundToHours observationDateTime
        { Header = 
            { ObservationTime = 
                { Date = roundedObservationTime.Date
                  Hour = byte(roundedObservationTime.Hour) }
              StationNumber = stationNumber 
              RequestTime = roundToSeconds currentTime }
          Temperature = -1.3m }

    let sortObservations observations = 
        observations |> (List.sortBy (fun o -> o.Header))

    let testSaveObservations observations = 
        Database.writeDataContextForList 
            DbService.insertObservation connectionstring observations |> ignore
        
        let result = 
            Database.readDataContext 
                DbService.getObservations connectionstring ()
        
        let expectedResult = observations |> sortObservations |> Success
        expectedResult =! (result |> Result.map sortObservations)

    [<Fact>]
    let ``SaveObservations should save empty list of observation correctly``() = 
        testSaveObservations []

    [<Fact>]
    let ``SaveObservations should save single observation correctly``() = 
        testSaveObservations [ getSampleObservation (StationNumber 0) currentTime ]

    [<Fact>]
    let ``GetLastObservationTimeListForStations should return empty list when there are no observations``() = 
        let requestedStationNumbers = [StationNumber 0; StationNumber 10; StationNumber 100]
        let interval = 
            { From = currentTime.AddDays(-1.0)
              To = currentTime }
        
        let result = 
            Database.readDataContext
                DbService.getLastObservationTimeListForStations connectionstring (interval, requestedStationNumbers)

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
        
        Database.writeDataContextForList 
            DbService.insertObservation connectionstring observationList |> ignore
        
        let result = 
            Database.readDataContext 
                DbService.getLastObservationTimeListForStations connectionstring (interval, requestedStationNumberList)

        let expectedResult = 
            requestedStationNumberList
            |> List.map (
                function 
                | stNumber when List.contains stNumber savedStationNumberList -> 
                    (stNumber, Some (roundToHours observationTime))
                | stNumber -> (stNumber, None))
            |> Success
        expectedResult =! (result |> Result.map (List.sortBy fst))

    [<Fact>]
    let ``Test``() = 
        Database.writeDataContext
            DbService.insertObservation connectionstring (getSampleObservation (StationNumber 0) currentTime) |> ignore
        let results = 
            Database.readDataContext 
                DbService.getObservationsAndStations connectionstring ()

        match results with
            | Success list -> list.Length >! 0
            | Failure failure -> failwith failure
        
