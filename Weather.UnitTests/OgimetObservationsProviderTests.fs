module OgimetObservationsProviderTests

open Swensen.Unquote
open FsCheck.Xunit
open Weather.Utils
open Weather.Synop
open Weather.Model
open Weather.DataProvider
open System.Net

[<Property>]
let ``FetchObservationsByInterval returns correct result`` 
        (date : System.DateTime)
        stationNumber
        interval
        temperature
        () =
    
    let correctedDate = date.AddMinutes(30.0)
    let synopParser _ =
        Success { Day = byte correctedDate.Day
                  Hour = byte correctedDate.Hour
                  StationNumber = stationNumber
                  Temperature = temperature }

    let observationString = 
        sprintf "%06d,%04d,%02d,%02d,%02d,%02d,dummySYNOP" 
            stationNumber date.Year date.Month date.Day 
            date.Hour date.Minute

    let httpGetFunc _ _ = 
        (HttpStatusCode.OK, observationString)

    let expectedResult = 
        [Success 
            { Header = 
                { StationNumber = StationNumber stationNumber
                  ObservationTime = 
                    { Date = correctedDate.Date
                      Hour = byte <| correctedDate.Hour }}
              Temperature = temperature}]

    expectedResult =! OgimetObservationsProvider.fetchObservationsByInterval 
        synopParser 
        httpGetFunc 
        (StationNumber stationNumber, interval)
