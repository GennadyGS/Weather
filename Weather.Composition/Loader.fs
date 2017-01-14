module Weather.Loader

open FSharp.Data
open System
open Weather.Utils.TryParser
open Weather.Utils.String
open Weather.Model

let private formatDate (date : DateTime) : string = 
    date.ToString("yyyyMMddHHmm")

let private getRequestUrl (stationNumber : int) (dateFrom : DateTime) (dateTo : DateTime) = 
    sprintf "http://www.ogimet.com/cgi-bin/getsynop?block=%d&begin=%s&end=%s" stationNumber (formatDate dateFrom) (formatDate dateTo)

let parseObservation (string : string) : Observation = 
    string
        |> splitString ','
        |> function
            | [|Int(station); Int(year); Int(month); Int(day); Byte(hour); "00"; synopCode|] -> 
                {
                    Time = 
                        {
                            Date = System.DateTime(year, month, day);
                            Hour = hour
                        };
                    StationNumber = station;
                    Temperature = None
                }
            | _ -> raise (sprintf "Invalid observation string format: %s" string |> FormatException)

let loadObservations (stationNumber : int) (dateFrom : DateTime) (dateTo : DateTime) : Observation list = 
    getRequestUrl stationNumber dateFrom dateTo
        |> Http.RequestString 
        |> splitString '\n' 
        |> List.ofArray 
        |> List.map parseObservation
