module Weather.DataProvider.ObservationsProvider

open FSharp.Data
open System
open Weather.Utils
open Weather.Utils.TryParser
open Weather.Utils.String
open Weather.Utils.Result
open Weather.Utils.RegEx
open Weather.Model
open Weather.Synop
open Weather.Synop.Parser
open System.Net

let [<Literal>] private Url = "http://www.ogimet.com/cgi-bin/getsynop"

let private formatDateTime (dateTime : DateTime) : string = 
    dateTime.ToString("yyyyMMddHHmm")

let private roundToMinutes (dateTime : DateTime) = 
    let updated = dateTime.AddMinutes(30.0)
    DateTime(updated.Year, updated.Month, updated.Day, updated.Hour, 0, 0, dateTime.Kind);

let private formatStationNumber =
    sprintf "%05d"

let private mapTupleOption f (a, b) = 
    b |> Option.map (fun item -> a, f item)

let private getUrlQueryParams (stationNumber : int) (dateFrom : DateTime option) (dateTo : DateTime option) = 
    ("block", stationNumber |> formatStationNumber) ::
    List.concat [
        ("begin", dateFrom) |> (mapTupleOption formatDateTime) |> Option.toList;
        ("end", dateTo) |> (mapTupleOption formatDateTime) |> Option.toList]
    
let private toObservation header synop =
    { Header = header
      Temperature = synop.Temperature }

let private parseHeader string =
    string
    |> split [|','|]
    |> function
        | [|Int(stationNumber); Int(year); Int(month); Int(day); Int(hour); Int(minute); synopString|] -> 
            let roundedObservationTime = DateTime(year, month, day, hour, minute, 0) |> roundToMinutes
            let header = 
                { StationNumber = stationNumber
                  ObservationTime = 
                    { Date = roundedObservationTime.Date
                      Hour = byte roundedObservationTime.Hour }}
            Success (header, synopString)
        | _ -> Failure (InvalidHeaderFormat (sprintf "Invalid observation string format: %s" string))

let private parseSynop header string =  
    match string with
    | Synop(synop) -> Success (toObservation header synop)
    | _ -> Failure (InvalidObservationFormat (header, sprintf "Invalid SYNOP format: %s" string))

let private parseObservation string = 
    result {
        let! (header, synopString) = parseHeader string
        return! parseSynop header synopString
    }
let private checkHttpStatusInResponseString (string : string) : string = 
    match string with
    | Regex @"^Status: (\d{3}) (.*)$" 
        [Int(status); message] -> 
            let errorText = sprintf "The remote server returned an error: (%d) %s" status message
            let statusCode : WebExceptionStatus = LanguagePrimitives.EnumOfValue(status)
            raise (WebException(errorText, statusCode))
    | _ -> string

let fetchObservations stationNumber dateFrom dateTo = 
    Http.RequestString (Url, query = getUrlQueryParams stationNumber dateFrom dateTo)
        |> split [|'\r'; '\n'|] 
        |> List.ofArray 
        |> List.filter (fun line -> line <> String.Empty)
        |> List.map (checkHttpStatusInResponseString >> parseObservation)

let fetchObservationsByInterval stationNumber interval = 
    fetchObservations stationNumber (Some interval.From) (Some interval.To)
