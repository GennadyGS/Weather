module Weather.Tests.LoaderTests

open Xunit
open Weather.Persistence.DbService

[<Fact>]
let ``SaveObservations should save empty array of observation correctly``() = saveObservations ([||])

[<Fact>]
let ``SaveObservations should save single observation correctly``() = 
    let now = System.DateTime.UtcNow
    saveObservations ([| { Time = 
                               { Date = now
                                 Hour = byte(now.Hour) }
                           StationNumber = 0
                           Temperature = -1.3m } |])
