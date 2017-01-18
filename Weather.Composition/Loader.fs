module Weather.Loader

open FSharp.Data
open System
open Weather.Utils.TryParser
open Weather.Utils.String
open Weather.Model
open Weather.Synop.Parser

let private formatDate (date : DateTime) : string = 
    date.ToString("yyyyMMddHHmm")

let private getRequestUrl (stationNumber : int) (dateFrom : DateTime) (dateTo : DateTime) = 
    sprintf "http://www.ogimet.com/cgi-bin/getsynop?block=%d&begin=%s&end=%s" stationNumber (formatDate dateFrom) (formatDate dateTo)

let private parseObservation (string : string) : Observation = 
    string
        |> splitString ','
        |> function
            | [|Int(station); Int(year); Int(month); Int(day); Byte(hour); Byte(0uy); Synop(synop)|] -> 
                {
                    Time = 
                        {
                            Date = System.DateTime(year, month, day);
                            Hour = hour
                        };
                    StationNumber = station;
                    Temperature = synop.Temperature
                }
            | _ -> raise (sprintf "Invalid observation string format: %s" string |> FormatException)

let loadObservations (stationNumber : int) (dateFrom : DateTime) (dateTo : DateTime) : Observation list = 
    getRequestUrl stationNumber dateFrom dateTo
        |> Http.RequestString 
        |> splitString '\n' 
        |> List.ofArray 
        |> List.filter (fun line -> line <> String.Empty)
        |> List.map parseObservation
