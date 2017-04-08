namespace Weather.IntegraionTests

open Xunit
open System
open Swensen.Unquote
open Weather.Utils
open Weather.IntegrationTests
open Weather.Persistence

type CompositionRootTests() =
    inherit DbTests()

    [<Fact>]
    let ``FillNewDataForStations for emply list should not save observations``() =
        Weather.Composition.CompositionRoot.fillNewDataForStations
            Settings.ConnectionStrings.Weather 
            Settings.MinTimeSpan
            { From = DateTime.UtcNow.AddDays(-1.0)
              To = DateTime.UtcNow }
            []
    
        DbService.getObservations Settings.ConnectionStrings.Weather () =! []

    [<Fact>]
    let ``FillNewDataForStations for the last day do not throw exception``() =
        Weather.Composition.CompositionRoot.fillNewDataForStations
            Settings.ConnectionStrings.Weather 
            Settings.MinTimeSpan
            { From = DateTime.UtcNow.AddDays(-1.0)
              To = DateTime.UtcNow }
            [33345]

        // TODO: Assert results
