﻿namespace Weather.IntegraionTests

open Xunit
open System
open Swensen.Unquote
open Weather.IntegrationTests
open Weather.Persistence

type CompositionRootTests() =
    inherit DbTests()

    [<Fact>]
    let ``FillNewData for emply list should not save observations``() =
        Weather.Composition.CompositionRoot.fillNewData
            Settings.ConnectionStrings.Weather 
            Settings.MinTimeSpan
            []
            { From = DateTime.UtcNow.AddDays(-1.0)
              To = DateTime.UtcNow }
    
        DbService.getObservations Settings.ConnectionStrings.Weather =! []

    [<Fact>]
    let ``FillNewData for the last day do not throw exception``() =
        Weather.Composition.CompositionRoot.fillNewData
            Settings.ConnectionStrings.Weather 
            Settings.MinTimeSpan
            [33345]
            { From = DateTime.UtcNow.AddDays(-1.0)
              To = DateTime.UtcNow }

        // TODO: Assert results
