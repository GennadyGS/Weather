namespace Weather.IntegraionTests

open Xunit
open Weather.Persistence
open Weather.IntegrationTests
open System.Diagnostics
open System

type LoaderTests() =
    inherit DbTests()
    
    [<Fact>]
    let ``SaveObservations should save empty list of observation correctly``() = 
        DbService.saveObservations Settings.ConnectionStrings.Weather ([])

    [<Fact>]
    let ``SaveObservations should save single observation correctly``() = 
        let now = System.DateTime.UtcNow
        DbService.saveObservations Settings.ConnectionStrings.Weather 
            ([{ Header = 
                    { ObservationTime = 
                        { Date = now
                          Hour = byte(now.Hour) }
                      StationNumber = 0 }
                Temperature = -1.3m } ])
