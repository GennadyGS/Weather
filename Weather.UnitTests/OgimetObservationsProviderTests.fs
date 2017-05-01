module OgimetObservationsProviderTests

open System
open Swensen.Unquote
open FsCheck
open FsCheck.Xunit
open Weather.Utils
open Weather.Synop
open Weather.Model
open Weather.DataProvider
open System.Net

let private roundToHours (dateTime : DateTime) = 
    let updated = dateTime.AddMinutes(30.0)
    DateTime(updated.Year, updated.Month, updated.Day, updated.Hour, 0, 0, dateTime.Kind);

[<Property>]
let ``FetchObservationsByInterval returns correct result`` 
        (date : DateTime)
        (stationNumber : PositiveInt)
        interval
        temperature
        (synopStr : NonEmptyString) =

    (synopStr.Get 
    |> String.split [|'\r'; '\n'|] 
    |> Array.length <= 1) ==> lazy

    let roundedDate =  roundToHours date
    let synopParser _ =
        Success { Day = byte roundedDate.Day
                  Hour = byte roundedDate.Hour
                  StationNumber = stationNumber.Get
                  Temperature = temperature }

    let observationString = 
        sprintf "%05d,%04d,%02d,%02d,%02d,%02d,%s" 
            stationNumber.Get date.Year date.Month date.Day date.Hour date.Minute synopStr.Get

    let httpGetFunc _ _ = 
        (HttpStatusCode.OK, observationString)

    let expectedResult = 
        [Success 
            { Header = 
                { StationNumber = StationNumber stationNumber.Get
                  ObservationTime = 
                    { Date = roundedDate.Date
                      Hour = byte roundedDate.Hour }}
              Temperature = temperature}]

    expectedResult =! OgimetObservationsProvider.fetchObservationsByInterval 
        synopParser 
        httpGetFunc 
        (StationNumber stationNumber.Get, interval)
