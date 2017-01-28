module Weather.IntegraionTests.ObservationsProviderTests

open Xunit
open Weather.DataProvider
open System

[<Fact>]
let ``FetchObservations for empty interval should return`` () =
    let results = ObservationsProvider.fetchObservations "33345" 
                    (Some DateTime.Now) (Some (DateTime.Now.AddDays(-1.0)))
    ()