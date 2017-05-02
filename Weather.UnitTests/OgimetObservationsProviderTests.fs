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
open Weather.Utils.Result

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
        let observationString = 
            sprintf "%05d,%04d,%02d,%02d,%02d,%02d,AAXX %02d%02d1 %s" 
                stationNumber.Get date.Year date.Month date.Day date.Hour date.Minute 
                roundedDate.Day roundedDate.Hour synopStr.Get

        let synopParser _ =
            Success { StationNumber = stationNumber.Get
                      Temperature = temperature }
        let httpGetFunc _ _ = 
            (HttpStatusCode.OK, observationString)

        let result = 
            OgimetObservationsProvider.fetchObservationsByInterval 
                synopParser httpGetFunc (StationNumber stationNumber.Get, interval)        
        result =! 
            [Success 
                { Header = 
                    { StationNumber = StationNumber stationNumber.Get
                      ObservationTime = 
                        { Date = roundedDate.Date
                          Hour = byte roundedDate.Hour }}
                  Temperature = temperature}]

    [<Property>]
    member this. ``FetchObservationsByInterval returns empty list for non SYNOP input string``
            (date : DateTime)
            (stationNumber : PositiveInt)
            interval
            (synopStr : SingleLineString) =

        let roundedDate = DateTime.roundToHours date
        let observationString = 
            sprintf "%05d,%04d,%02d,%02d,%02d,%02d,AAXY %02d%02d1 %s" 
                stationNumber.Get date.Year date.Month date.Day date.Hour date.Minute 
                roundedDate.Day roundedDate.Hour synopStr.Get

        let synopParser _ =
            raise (Exception("SYNOP parser should not be called"))
        let httpGetFunc _ _ = 
            (HttpStatusCode.OK, observationString)

        let result = 
            OgimetObservationsProvider.fetchObservationsByInterval 
                synopParser httpGetFunc (StationNumber stationNumber.Get, interval)        
        result =! []
