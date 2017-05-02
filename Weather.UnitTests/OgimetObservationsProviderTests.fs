namespace Weather.UnitTests

open System
open Swensen.Unquote
open FsCheck
open FsCheck.Xunit
open Weather.Utils
open Weather.Synop
open Weather.Model
open Weather.DataProvider
open System.Net

type OgimetObservationsProviderTests() =
    inherit GeneratorTests()

    [<Property>]
    member this. ``FetchObservationsByInterval returns observation for correct input string``
            (date : DateTime)
            (stationNumber : PositiveInt)
            interval
            temperature
            (synopStr : SingleLineString) =

        let roundedDate = DateTime.roundToHours date
        let synopParser _ =
            Success { StationNumber = stationNumber.Get
                      Temperature = temperature }

        let observationString = 
            sprintf "%05d,%04d,%02d,%02d,%02d,%02d,AAXX %02d%02d1 %s" 
                stationNumber.Get date.Year date.Month date.Day date.Hour date.Minute 
                roundedDate.Day roundedDate.Hour synopStr.Get

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
