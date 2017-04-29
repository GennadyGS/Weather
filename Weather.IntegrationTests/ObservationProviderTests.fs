namespace Weather.IntegraionTests

open Xunit
open Weather.DataProvider
open System
open Swensen.Unquote
open System.Net
open Weather.IntegrationTests
open Weather.Model
open Weather.HttpClient

type ObservationsProviderTests() =
    inherit DbTests()

    [<Fact>]
    let ``FetchObservations for empty interval should throw WebException`` () =
        raisesWith<WebException> 
            <@ OgimetObservationsProvider.fetchObservations 
                HttpClient.httpGet 
                (StationNumber 33345)
                (Some DateTime.Now) 
                (Some (DateTime.Now.AddDays(-1.0))) @>
