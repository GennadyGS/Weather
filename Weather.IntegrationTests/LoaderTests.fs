module Weather.IntegraionTests.LoaderTests

open Xunit
open Weather.Persistence
open Weather.IntegrationTests

[<Fact>]
let ``SaveObservations should save empty array of observation correctly``() = 
    DbService.saveObservations Settings.ConnectionStrings.Weather ([||])

[<Fact>]
let ``SaveObservations should save single observation correctly``() = 
    let now = System.DateTime.UtcNow
    DbService.saveObservations Settings.ConnectionStrings.Weather 
                            ([| { Time = { Date = now; Hour = byte(now.Hour) };
                                StationNumber = "0";
                                Temperature = -1.3m } |])
