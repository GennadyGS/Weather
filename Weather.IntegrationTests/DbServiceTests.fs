namespace Weather.IntegraionTests

open Xunit
open Swensen.Unquote
open Weather.Model
open Weather.Persistence
open Weather.IntegrationTests

type DbServiceTests() =
    inherit DbTests()
    
    [<Fact>]
    let ``SaveObservations should save empty list of observation correctly``() = 
        DbService.saveObservations Settings.ConnectionStrings.Weather ([])

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
        DbService.saveObservations Settings.ConnectionStrings.Weather [observation]
        DbService.getObservations Settings.ConnectionStrings.Weather =! [observation]