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
open Weather.Utils.DateTime

type OgimetObservationsProviderTests() =
    inherit GeneratorTests()

    [<Literal>]
    let synopFormatCode = "AAXX"

    let currentTime = DateTime.UtcNow

    let generateHeaderAndHeaderString observationFormatCode synopStr stationNumber date =
        let roundedDate = DateTime.roundToHours date
        let headerString = 
            sprintf "%05d,%04d,%02d,%02d,%02d,%02d,%s %02d%02d1 %s" 
                stationNumber date.Year date.Month date.Day date.Hour date.Minute 
                observationFormatCode roundedDate.Day roundedDate.Hour synopStr
        let header =
            { StationNumber = StationNumber stationNumber
              ObservationTime = 
                { Date = roundedDate.Date
                  Hour = byte roundedDate.Hour }
              RequestTime = roundToMilliseconds currentTime }
        (header, headerString)

    let toTupleOfLists listOfTuples =
        let rec f listOfTuples (list1, list2) = 
            match listOfTuples with
            | (item1, item2)::tail -> f tail (item1::list1, item2::list2)
            | [] -> (list1, list2)
        f (List.rev listOfTuples) ([], [])

    [<Property>]
    member this. ``FetchObservationsByInterval returns observation for correct input string``
            (date : DateTime)
            (stationNumber : PositiveInt)
            interval
            temperature
            (synopStr : SingleLineString) =

        let (header, headerString) = 
            generateHeaderAndHeaderString synopFormatCode synopStr.Get stationNumber.Get date 
        let synopParser _ =
            Success { StationNumber = stationNumber.Get
                      Temperature = temperature }
        let httpGetFunc _ _ = 
            (HttpStatusCode.OK, headerString)

        let result = 
            OgimetObservationsProvider.fetchObservationsByInterval 
                synopParser httpGetFunc currentTime 
                (StationNumber stationNumber.Get, interval)        

        result =! 
            [Success
                { Header = header
                  Temperature = temperature}]

    [<Property>]
    member this. ``FetchObservationsByInterval returns observation for each of correct input string``
            (dates : DateTime list)
            (stationNumber : PositiveInt)
            interval
            temperature
            (synopStr : SingleLineString) =

        let (headers, headerStrings) =
            dates
            |> List.map (generateHeaderAndHeaderString synopFormatCode synopStr.Get stationNumber.Get)
            |> toTupleOfLists
            
        let synopParser _ =
            Success { StationNumber = stationNumber.Get
                      Temperature = temperature }
        let httpGetFunc _ _ = 
            (HttpStatusCode.OK, String.Join("\r\n", headerStrings))

        let result = 
            OgimetObservationsProvider.fetchObservationsByInterval 
                synopParser httpGetFunc currentTime 
                (StationNumber stationNumber.Get, interval)        

        result =! 
            (headers
            |> List.map 
                (fun header ->
                    Success { Header = header
                              Temperature = temperature}))

    [<Property>]
    member this. ``FetchObservationsByInterval returns empty list for non SYNOP input string``
            (date : DateTime)
            (stationNumber : PositiveInt)
            interval
            (synopStr : SingleLineString) =

        let (_, headerString) = 
            generateHeaderAndHeaderString "AAXY" synopStr.Get stationNumber.Get date 
        let synopParser _ =
            raise (Exception("SYNOP parser should not be called"))
        let httpGetFunc _ _ = 
            (HttpStatusCode.OK, headerString)

        let result = 
            OgimetObservationsProvider.fetchObservationsByInterval 
                synopParser httpGetFunc currentTime 
                (StationNumber stationNumber.Get, interval)        
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
                synopParser httpGetFunc currentTime 
                (StationNumber stationNumber.Get, interval)        
        test <@ match result with
                | [Failure (InvalidObservationHeaderFormat _)] -> true
                | _ -> false @>

    [<Property>]
    member this. ``FetchObservationsByInterval returns Failure InvalidObservationFormat when synopParser returns failure``
            (date : DateTime)
            (stationNumber : PositiveInt)
            interval
            (synopStr : SingleLineString) 
            synopParserErrorMessage =

        let (header, headerString) = 
            generateHeaderAndHeaderString synopFormatCode synopStr.Get stationNumber.Get date 
        let synopParser _ =
            Failure synopParserErrorMessage
        let httpGetFunc _ _ = 
            (HttpStatusCode.OK, headerString)

        let result = 
            OgimetObservationsProvider.fetchObservationsByInterval 
                synopParser httpGetFunc currentTime 
                (StationNumber stationNumber.Get, interval)        

        result =! [Failure (InvalidObservationFormat (header, synopParserErrorMessage))]

    [<Property>]
    member this. ``FetchObservationsByInterval returns Failure InvalidObservationFormat when synopParser returns Synop with invalid station number``
            (date : DateTime)
            (stationNumber : PositiveInt)
            interval
            temperature
            (synopStr : SingleLineString) =

        let (header, headerString) = 
            generateHeaderAndHeaderString synopFormatCode synopStr.Get stationNumber.Get date 
        let synopParser _ =
            Success { StationNumber = stationNumber.Get + 1
                      Temperature = temperature }
        let httpGetFunc _ _ = 
            (HttpStatusCode.OK, headerString)

        let result = 
            OgimetObservationsProvider.fetchObservationsByInterval 
                synopParser httpGetFunc currentTime
                (StationNumber stationNumber.Get, interval)        

        test <@ match result with
                | [Failure (InvalidObservationFormat (header, _))] -> true
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
            synopParser httpGetFunc currentTime
            (StationNumber stationNumber.Get, interval) |> ignore      
        
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
                synopParser httpGetFunc currentTime
                (StationNumber stationNumber.Get, interval)

        result =! [Failure (HttpError (httpStatusCode, httpErrorMessage))]