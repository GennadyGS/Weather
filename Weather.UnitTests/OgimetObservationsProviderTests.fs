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
        let headerString = 
            sprintf "%05d,%04d,%02d,%02d,%02d,%02d,AAXX %02d%02d1 %s" 
                stationNumber.Get date.Year date.Month date.Day date.Hour date.Minute 
                roundedDate.Day roundedDate.Hour synopStr.Get

        let synopParser _ =
            Success { StationNumber = stationNumber.Get
                      Temperature = temperature }
        let httpGetFunc _ _ = 
            (HttpStatusCode.OK, headerString)

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
        let headerString = 
            sprintf "%05d,%04d,%02d,%02d,%02d,%02d,AAXY %02d%02d1 %s" 
                stationNumber.Get date.Year date.Month date.Day date.Hour date.Minute 
                roundedDate.Day roundedDate.Hour synopStr.Get

        let synopParser _ =
            raise (Exception("SYNOP parser should not be called"))
        let httpGetFunc _ _ = 
            (HttpStatusCode.OK, headerString)

        let result = 
            OgimetObservationsProvider.fetchObservationsByInterval 
                synopParser httpGetFunc (StationNumber stationNumber.Get, interval)        
        result =! []

    [<Property>]
    member this. ``FetchObservationsByInterval returns Failure InvalidObservationHeaderFormat for invalid input string``
        (stationNumber : PositiveInt)
        interval
        (headerString : SingleLineString) = 

        let httpGetFunc _ _ = 
            (HttpStatusCode.OK, headerString.Get)
        let synopParser _ =
            raise (Exception("SYNOP parser should not be called"))

        let result = 
            OgimetObservationsProvider.fetchObservationsByInterval 
                synopParser httpGetFunc (StationNumber stationNumber.Get, interval)        
        test <@ match result with
                | [Failure (InvalidObservationHeaderFormat _)] -> true
                | _ -> false @>

    [<Property>]
    member this. ``FetchObservationsByInterval passes correct URL to httpGetFunc``
            (stationNumber : PositiveInt)
            interval
            (synopStr : SingleLineString) 
            synopParser =

        let mutable receivedBaseUrl = String.Empty
        let mutable receivedUrlParams = []
        let httpGetFunc baseUrl urlParams = 
            receivedBaseUrl <- baseUrl
            receivedUrlParams <- urlParams
            (HttpStatusCode.OK, String.Empty)

        OgimetObservationsProvider.fetchObservationsByInterval 
            synopParser httpGetFunc (StationNumber stationNumber.Get, interval) |> ignore      
        
        let formatDate (date : DateTime) = date.ToString("yyyyMMddHHmm")
        receivedBaseUrl =! "http://www.ogimet.com/cgi-bin/getsynop"
        receivedUrlParams =! [
            ("block", stationNumber.Get.ToString("D5"))
            ("begin", formatDate interval.From)
            ("end", formatDate interval.To)]