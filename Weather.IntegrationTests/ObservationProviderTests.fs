namespace Weather.IntegraionTests

open Xunit
open System
open Swensen.Unquote
open System.Net
open Weather.Utils
open Weather.Model
open Weather.IntegrationTests
open Weather.Composition

type OgimetObservationProviderTests() =
    inherit DbTests()

    [<Fact>]
    let ``FetchObservations for empty interval should throw WebException`` () =
        let result = 
            ObservationProviders.Ogimet.fetchObservationsByInterval
                (Settings.OgimetBaseUrl.ToString())
                DateTime.UtcNow
                ((StationNumber 33345),
                { From = DateTime.Now
                  To = DateTime.Now.AddDays(-1.0) })
        
        true =! match result with
                | [Result.Failure (HttpError (HttpStatusCode.InternalServerError, _))] -> true 
                | _ -> false
