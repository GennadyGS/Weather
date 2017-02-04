module Weather.IntegraionTests.CompositionRootTests

open Xunit
open System
open Weather.IntegrationTests

[<Fact>]
let ``FillNewData for the last day do not throw exception``() =
    Weather.Composition.CompositionRoot.fillNewData 
        Settings.ConnectionStrings.Weather 
        "33345" 
        {From = DateTime.UtcNow.AddDays(-1.0); To = DateTime.UtcNow}
