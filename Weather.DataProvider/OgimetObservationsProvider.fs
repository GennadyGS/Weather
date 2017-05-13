module Weather.DataProvider.OgimetObservationsProvider

open System
open Weather
open Weather.Utils
open Weather.Utils.TryParser
open Weather.Utils.String
open Weather.Utils.Result
open Weather.Utils.RegEx
open Weather.Model
open Weather.Synop
open Weather.Utils.DateTime

let private formatDateTime (dateTime : DateTime) : string = 
    dateTime.ToString("yyyyMMddHHmm")

let private formatStationNumber =
    sprintf "%05d"

let private buildOptionalParam name optionalValue = 
    optionalValue |> Option.map (fun value -> (name, value))

let private getUrlQueryParams (StationNumber stationNumber) dateFrom dateTo = 
    ("block", stationNumber |> formatStationNumber) ::
    List.concat [
        buildOptionalParam "begin" (dateFrom |> Option.map formatDateTime) 
            |> Option.toList
        buildOptionalParam "end" (dateTo |> Option.map formatDateTime) 
            |> Option.toList]
    
let private tryParseHeader requestTime string =
    match string with
    | Regex @"^(\d{5}),(\d{4}),(\d{2}),(\d{2}),(\d{2}),(\d{2}),AAXX (\d{2})(\d{2})\d (.*)$" 
        [Int(stationNumber); Int(year); Int(month); Int(day); Int(hour); Int(minute); 
         Int(registerDay); Int(registerHour); synopString] -> 
            let roundedObservationTime = DateTime(year, month, day, hour, minute, 0) |> DateTime.roundToHours
            if registerDay <> roundedObservationTime.Day then
                Some (Failure <| InvalidObservationHeaderFormat (sprintf "Registration day does not match the header: '%s'" string))
            elif registerHour <> int roundedObservationTime.Hour then
                Some (Failure <| InvalidObservationHeaderFormat (sprintf "Registration hour does not match the header: '%s'" string))
            else 
                let header =
                    { StationNumber = StationNumber stationNumber
                      ObservationTime = 
                        { Date = roundedObservationTime.Date
                          Hour = byte roundedObservationTime.Hour }
                      RequestTime = roundToSeconds requestTime }
                Some <| Success (header, synopString)
    | Regex @"^(\d{5}),(\d{4}),(\d{2}),(\d{2}),(\d{2}),(\d{2}),[A-Z]{4}(.*)$" _ ->
        None
    | _ -> Some (Failure (InvalidObservationHeaderFormat (sprintf "Observation header does not match the template: '%s'" string)))

let private safeCreateObservation (header : ObservationHeader) synop synopStr =
    let (StationNumber headerStationNumber) = header.StationNumber
    if synop.StationNumber <> headerStationNumber then
        Failure <| InvalidObservationFormat (header, sprintf "Expected station number '%05d' in SYNOP: '%s'" headerStationNumber synopStr)
    else
        Success { Header = header
                  Temperature = synop.Temperature }

let private parseSynop parser (header, synopStr) =  
    synopStr
    |> parser
    |> Result.bindBoth
        (fun synop -> safeCreateObservation header synop synopStr)
        (fun message -> Failure <| InvalidObservationFormat (header, message))

let private tryParseObservation synopParser requestTime = 
    tryParseHeader requestTime
    >> Option.map (Result.bind (parseSynop synopParser))

let private checkHttpStatusInResponseString string = 
    match string with
    | Regex @"^Status: (\d{3}) (.*)" 
        [Int(status); message] -> 
            let statusCode = LanguagePrimitives.EnumOfValue(status)
            HttpError (statusCode, message) |> Failure
    | _ -> Success string

let private splitResponseIntoLines = 
    String.split [|'\r'; '\n'|] 
    >> List.ofArray 
    >> List.filter (fun line -> line <> String.Empty)

let fetchObservations synopParser httpGetFunc baseUrl requestTime stationNumber dateFrom dateTo = 
    Logic.HttpClient.safeHttpGet
             httpGetFunc baseUrl (getUrlQueryParams stationNumber dateFrom dateTo)
        |> Result.bind checkHttpStatusInResponseString
        |> Result.mapToList splitResponseIntoLines
        |> List.choose (Result.bindToOption (tryParseObservation synopParser requestTime))

let fetchObservationsByInterval synopParser httpGetFunc baseUrl requestTime (stationNumber, interval) = 
    fetchObservations synopParser httpGetFunc baseUrl requestTime stationNumber 
        (Some interval.From) (Some interval.To)
