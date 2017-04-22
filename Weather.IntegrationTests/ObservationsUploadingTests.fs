﻿namespace Weather.IntegraionTests

open Xunit
open System
open Swensen.Unquote
open Weather.Utils
open Weather.IntegrationTests
open Weather.Persistence
open Weather.Composition

type ObservationsUploadingTests() =
    inherit DbTests()
    interface IClassFixture<LoggingTestFixture>

    [<Fact>]
    member this.``FillNewDataForStations for emply list should not save observations``() =
        let results = 
            ObservationsUploading.fillNewDataForStations
                Settings.ConnectionStrings.Weather 
                Settings.MinTimeSpan 
                { From = DateTime.UtcNow.AddDays(-1.0)
                  To = DateTime.UtcNow }
                []
    
        results =! []
        DbService.getObservations Settings.ConnectionStrings.Weather () =! []

    [<Fact>]
    member this.``FillNewDataForStations for the last day do not throw exception``() =
        let results = 
            ObservationsUploading.fillNewDataForStations
                Settings.ConnectionStrings.Weather 
                Settings.MinTimeSpan
                { From = DateTime.UtcNow.AddDays(-1.0)
                  To = DateTime.UtcNow }
                [33345]

        results =! [Success ()]
        // TODO: verify results in database