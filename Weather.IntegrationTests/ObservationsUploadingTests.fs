﻿namespace Weather.IntegraionTests

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
                { From = DateTime.UtcNow.AddDays(-1.0)
                  To = DateTime.UtcNow }
                [StationNumber 33345]

        // TODO: verify results in database
