module Weather.Composition.ObservationProvider

open FSharp.Data
open System
open Weather.Utils.TryParser
open Weather.Utils.String
open Weather.Utils.Result
open Weather.Model
open Weather.Synop.Parser

let [<Literal>] private Url = "http://www.ogimet.com/cgi-bin/getsynop"

let private formatDate (date : DateTime) : string = 
    date.ToString("yyyyMMddHHmm")

let private getUrlParams (stationNumber : string) (dateFrom : DateTime option) (dateTo : DateTime option) = 
    List.concat [
        ["block", stationNumber]; 
        (dateFrom |> Option.map(fun item -> "begin", formatDate(item)) |> Option.toList); 
        (dateTo |> Option.map(fun item -> "end", formatDate(item)) |> Option.toList)]
    

let private parseObservation (string : string) : Result<Observation, string> = 
    string
        |> splitString ','
        |> function
            | [|stationNumber; Int(year); Int(month); Int(day); Byte(hour); Byte(0uy); Synop(synop)|] -> 
                Success {
                    Time = 
                        {
                            Date = System.DateTime(year, month, day);
                            Hour = hour
                        };
                    StationNumber = stationNumber;
                    Temperature = synop.Temperature
                }
            | _ -> Failure (sprintf "Invalid observation string format: %s" string)

let loadObservations (stationNumber : string) (dateFrom : DateTime option) (dateTo : DateTime option) : Result<Observation, string> list = 
    Http.RequestString (Url, query = getUrlParams stationNumber dateFrom dateTo)
        |> splitString '\n' 
        |> List.ofArray 
        |> List.filter (fun line -> line <> String.Empty)
        |> List.map parseObservation
