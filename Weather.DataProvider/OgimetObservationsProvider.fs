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

let [<Literal>] private Url = "http://www.ogimet.com/cgi-bin/getsynop"

let private formatDateTime (dateTime : DateTime) : string = 
    dateTime.ToString("yyyyMMddHHmm")

let private roundToMinutes (dateTime : DateTime) = 
    let updated = dateTime.AddMinutes(30.0)
    DateTime(updated.Year, updated.Month, updated.Day, updated.Hour, 0, 0, dateTime.Kind);

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
    
let private parseHeader string =
    match string with
    | Regex @"^(\d{5}),(\d{4}),(\d{2}),(\d{2}),(\d{2}),(\d{2}),(.*)$" 
        [Int(stationNumber); Int(year); Int(month); Int(day); Int(hour); Int(minute); synopString] -> 
            let roundedObservationTime = DateTime(year, month, day, hour, minute, 0) |> roundToMinutes
            let header =
                { StationNumber = StationNumber stationNumber
                  ObservationTime = 
                    { Date = roundedObservationTime.Date
                      Hour = byte roundedObservationTime.Hour }}
            Success (header, synopString)
    | _ -> Failure <| InvalidObservationHeaderFormat string

let private safeToObservation (header : ObservationHeader) synop synopStr =
    let (StationNumber headerStationNumber) = header.StationNumber
    if synop.StationNumber <> headerStationNumber then
        Failure <| InvalidObservationFormat (header, sprintf "Expected station number %d in SYNOP: '%s'" headerStationNumber synopStr)
    elif synop.Day <> byte header.ObservationTime.Date.Day then
        Failure <| InvalidObservationFormat (header, sprintf "Expected day %d in SYNOP: '%s'" header.ObservationTime.Date.Day synopStr)
    elif Math.Abs(int synop.Hour - int header.ObservationTime.Hour) > 1 then
        Failure <| InvalidObservationFormat (header, sprintf "Expected hour %d in SYNOP: '%s'" header.ObservationTime.Hour synopStr)
    else
        Success { Header = header
                  Temperature = synop.Temperature }

let private parseSynop parser header synopStr =  
    synopStr
    |> parser
    |> Result.bindBoth
        (fun synop -> safeToObservation header synop synopStr)
        (fun message -> Failure <| InvalidObservationFormat (header, message))

let private parseObservation synopParser string = 
    result {
        let! (header, synopString) = parseHeader string
        return! parseSynop synopParser header synopString
    }

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

let private fetchObservations synopParser httpGetFunc stationNumber dateFrom dateTo = 
    Logic.HttpClient.safeHttpGet
             httpGetFunc Url (getUrlQueryParams stationNumber dateFrom dateTo)
        |> Result.bind checkHttpStatusInResponseString
        |> Result.mapToList splitResponseIntoLines
        |> List.map (Result.bind (parseObservation synopParser))

let fetchObservationsByInterval synopParser httpGetFunc (stationNumber, interval) = 
    fetchObservations synopParser httpGetFunc stationNumber 
        (Some interval.From) (Some interval.To)
