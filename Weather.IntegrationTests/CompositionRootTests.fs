module Weather.IntegraionTests.CompositionRootTests

open Xunit
open System

[<Fact>]
let ``FillNewData for the last day do not throw exception``() =
    Weather.Composition.CompositionRoot.fillNewData "33345" {From = DateTime.UtcNow.AddDays(-1.0); To = DateTime.UtcNow}
