namespace Weather.IntegraionTests

open Xunit
open Swensen.Unquote
open Weather.Model
open Weather.Persistence
open Weather.IntegrationTests

type DbServiceTests() =
    inherit DbTests()
    
    let sortObservations observations = 
        observations |> List.sortBy (fun o -> o.Header)

    let testSaveObservations observations = 
        DbService.insertObservationList Settings.ConnectionStrings.Weather observations
        let results = DbService.getObservations Settings.ConnectionStrings.Weather
        (results |> sortObservations) =! (observations |> sortObservations)

    [<Fact>]
    let ``SaveObservations should save empty list of observation correctly``() = 
        testSaveObservations []

    [<Fact>]
    let ``SaveObservations should save single observation correctly``() = 
        let now = System.DateTime.UtcNow
        let observation = 
            { Header = 
                { ObservationTime = 
                    { Date = now.Date
                      Hour = byte(now.Hour) }
                  StationNumber = 0 }
              Temperature = -1.3m }
        testSaveObservations []
