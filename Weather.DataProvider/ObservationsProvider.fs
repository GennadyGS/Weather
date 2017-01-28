module Weather.DataProvider.ObservationsProvider

open FSharp.Data
open System
open Weather.Utils
open Weather.Utils.TryParser
open Weather.Utils.String
open Weather.Utils.Result
open Weather.Model
open Weather.Synop.Parser

let [<Literal>] private Url = "http://www.ogimet.com/cgi-bin/getsynop"

let private formatDate (date : DateTime) : string = 
    date.ToString("yyyyMMddHHmm")

let private mapTupleOption f (a, b) = 
    b |> Option.map (fun item -> a, f item)

let private getUrlQueryParams (stationNumber : string) (dateFrom : DateTime option) (dateTo : DateTime option) = 
    ("block", stationNumber) ::
    List.concat [
        ("begin", dateFrom) |> (mapTupleOption formatDate) |> Option.toList;
        ("end", dateTo) |> (mapTupleOption formatDate) |> Option.toList]
    
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

let fetchObservations 
        (stationNumber : string) 
        (dateFrom : DateTime option) 
        (dateTo : DateTime option) 
        : Result<Observation, string> list = 
    Http.RequestString (Url, query = getUrlQueryParams stationNumber dateFrom dateTo)
        |> splitString '\n' 
        |> List.ofArray 
        |> List.filter (fun line -> line <> String.Empty)
        |> List.map parseObservation

let fetchObservationsByInterval (stationNumber : string) (interval : DateTimeInterval) : Result<Observation, string> list = 
    fetchObservations stationNumber (Some interval.From) (Some interval.To)
