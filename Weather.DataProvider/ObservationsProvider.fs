module Weather.DataProvider.ObservationsProvider

open FSharp.Data
open System
open Weather.Utils
open Weather.Utils.TryParser
open Weather.Utils.String
open Weather.Utils.Result
open Weather.Utils.RegEx
open Weather.Model
open Weather.Synop.Parser
open System.Net

let [<Literal>] private Url = "http://www.ogimet.com/cgi-bin/getsynop"

let private formatDate (date : DateTime) : string = 
    date.ToString("yyyyMMddHHmm")

let private formatStationNumber : (int -> string) =
    sprintf "%05d"

let private mapTupleOption f (a, b) = 
    b |> Option.map (fun item -> a, f item)

let private getUrlQueryParams (stationNumber : int) (dateFrom : DateTime option) (dateTo : DateTime option) = 
    ("block", stationNumber |> formatStationNumber) ::
    List.concat [
        ("begin", dateFrom) |> (mapTupleOption formatDate) |> Option.toList;
        ("end", dateTo) |> (mapTupleOption formatDate) |> Option.toList]
    
let private parseObservation (string : string) : Result<Observation, string> = 
    result {
        let! (time, stationNumber, synop) = 
            string
            |> split [|','|]
            |> function
                | [|Int(stationNumber); Int(year); Int(month); Int(day); Byte(hour); Byte(0uy); synop|] -> 
                    let time = { Date = DateTime(year, month, day); Hour = hour };
                    Success (time, stationNumber, synop)
                | _ -> Failure (sprintf "Invalid observation string format: %s" string)
        return! 
            match synop with
            | Synop(synop) -> 
                Success {
                    Time = time;
                    StationNumber = stationNumber;
                    Temperature = synop.Temperature
                }
            | _ -> Failure (sprintf "Invalid SYNOP format: %s" string)
    }
let private checkHttpStatusInResponseString (string : string) : string = 
    match string with
    | Regex @"^Status: (\d{3}) (.*)$" 
        [Int(status); message] -> 
            let errorText = sprintf "The remote server returned an error: (%d) %s" status message
            let statusCode : WebExceptionStatus = LanguagePrimitives.EnumOfValue(status)
            raise (WebException(errorText, statusCode))
    | _ -> string

let fetchObservations 
        (stationNumber : int) 
        (dateFrom : DateTime option) 
        (dateTo : DateTime option) 
        : Result<Observation, string> list = 
    Http.RequestString (Url, query = getUrlQueryParams stationNumber dateFrom dateTo)
        |> split [|'\r'; '\n'|] 
        |> List.ofArray 
        |> List.filter (fun line -> line <> String.Empty)
        |> List.map (checkHttpStatusInResponseString >> parseObservation)

let fetchObservationsByInterval (stationNumber : int) (interval : DateTimeInterval) : Result<Observation, string> list = 
    fetchObservations stationNumber (Some interval.From) (Some interval.To)
