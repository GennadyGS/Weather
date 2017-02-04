namespace Weather.IntegraionTests

open Xunit
open Weather.DataProvider
open System
open Swensen.Unquote
open System.Net
open Weather.IntegrationTests


type ObservationsProviderTests() =
    inherit DbTests()

    [<Fact>]
    let ``FetchObservations for empty interval should return`` () =
        raisesWith<WebException> 
            <@ ObservationsProvider.fetchObservations "33345" 
                (Some DateTime.Now) (Some (DateTime.Now.AddDays(-1.0))) @>
