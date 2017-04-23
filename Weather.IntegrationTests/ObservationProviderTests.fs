namespace Weather.IntegraionTests

open Xunit
open Weather.DataProvider
open System
open Swensen.Unquote
open System.Net
open Weather.IntegrationTests
open Weather.Model


type ObservationsProviderTests() =
    inherit DbTests()

    [<Fact>]
    let ``FetchObservations for empty interval should throw WebException`` () =
        raisesWith<WebException> 
            <@ ObservationsProvider.fetchObservations (StationNumber 33345)
                (Some DateTime.Now) (Some (DateTime.Now.AddDays(-1.0))) @>
