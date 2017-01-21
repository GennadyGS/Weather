module Weather.Tests.LoaderTests

open Xunit
open Weather.Persistence.DbService
    
[<Fact>]
let ``SaveObservations should save observation correctly``() = 
    saveObservations([|{Time = {Date = System.DateTime.Now; Hour = 12uy}; StationNumber = 0; Temperature = -1.3m}|])
