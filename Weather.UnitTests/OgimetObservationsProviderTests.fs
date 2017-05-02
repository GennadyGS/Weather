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

    [<Literal>]
    let synopFormatCode = "AAXX"

    let generateHeaderAndHeaderString date stationNumber observationFormatCode synopStr =
        let roundedDate = DateTime.roundToHours date
        let headerString = 
            sprintf "%05d,%04d,%02d,%02d,%02d,%02d,%s %02d%02d1 %s" 
                stationNumber date.Year date.Month date.Day date.Hour date.Minute 
                observationFormatCode roundedDate.Day roundedDate.Hour synopStr
        let header =
            { StationNumber = StationNumber stationNumber
              ObservationTime = 
                { Date = roundedDate.Date
                  Hour = byte roundedDate.Hour }}
        (header, headerString)

    [<Property>]
    member this. ``FetchObservationsByInterval returns observation for correct input string``
            (date : DateTime)
            (stationNumber : PositiveInt)
            interval
            temperature
            (synopStr : SingleLineString) =

        let (header, headerString) = 
            generateHeaderAndHeaderString date stationNumber.Get synopFormatCode synopStr.Get
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
                { Header = header
                  Temperature = temperature}]

    [<Property>]
    member this. ``FetchObservationsByInterval returns empty list for non SYNOP input string``
            (date : DateTime)
            (stationNumber : PositiveInt)
            interval
            (synopStr : SingleLineString) =

        let (_, headerString) = 
            generateHeaderAndHeaderString date stationNumber.Get "AAXY" synopStr.Get
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

    [<Property>]
    member this. ``FetchObservationsByInterval returns Failure HttpError when httpGetFunc returns HttpError``
            (stationNumber : PositiveInt)
            interval
            httpStatusCode
            httpErrorMessage
            synopParser =

        httpStatusCode <> HttpStatusCode.OK ==> lazy

        let httpGetFunc _ _ = 
            (httpStatusCode, httpErrorMessage)

        let result = 
            OgimetObservationsProvider.fetchObservationsByInterval 
                synopParser httpGetFunc (StationNumber stationNumber.Get, interval)

        result =! [Failure (HttpError (httpStatusCode, httpErrorMessage))]