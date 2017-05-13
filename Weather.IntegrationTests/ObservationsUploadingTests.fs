namespace Weather.IntegraionTests

open Xunit
open System
open Swensen.Unquote
open Weather.Utils
open Weather.Utils.Database
open Weather.IntegrationTests
open Weather.Persistence
open Weather.Composition
open Weather.Model

type ObservationsUploadingTests() =
    inherit DbTests()
    interface IClassFixture<LoggingTestFixture>

    [<Fact>]
    member this.``FillNewDataForStations for emply list should not save observations``() =
        ObservationsUploading.fillNewDataForStations
            Settings.MinTimeSpan 
            Settings.ConnectionStrings.Weather 
            (Settings.OgimetBaseUrl.ToString())
            { From = DateTime.UtcNow.AddDays(-1.0)
              To = DateTime.UtcNow }
            []
    
        Database.readDataContext
            DbService.getObservations Settings.ConnectionStrings.Weather () =! Success []

    [<Fact>]
    member this.``FillNewDataForStations for the last day returns success result``() =
        ObservationsUploading.fillNewDataForStations
            Settings.MinTimeSpan
            Settings.ConnectionStrings.Weather 
            (Settings.OgimetBaseUrl.ToString())
            { From = DateTime.UtcNow.AddDays(-1.0)
              To = DateTime.UtcNow }
            [StationNumber 33345]

        let result = 
            Database.readDataContext
                DbService.getObservations Settings.ConnectionStrings.Weather ()
        // TODO: More detailed verification
        match result with
            | Success observations -> List.length observations >=! 7
            | Failure _ -> failwith "Result should be success"
