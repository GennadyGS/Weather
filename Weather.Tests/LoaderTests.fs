module Weather.Tests.LoaderTests

open Xunit
    
[<Fact>]
let ``Test loading``() = 
    Weather.Loader.Loader.saveObservations([|{Time = {Date = System.DateTime.Now; Hour = 12uy}; StationNumber = 0; Temperature = -1.3m}|])
