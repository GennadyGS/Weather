module Weather.Loader

open FSharp.Data
open System
open Weather.Model

let private splitString (char : char) (string : string) : string[] = 
    string.Split([|char|])

let private formatDate (date : DateTime) : string = 
    date.ToString("yyyyMMddHHmm")

let private getRequestUrl (stationNumber : int) (dateFrom : DateTime) (dateTo : DateTime) = 
    sprintf "http://www.ogimet.com/cgi-bin/getsynop?block=%d&begin=%s&end=%s" stationNumber (dateFrom |> formatDate) (dateTo |> formatDate)


let private parseObservation (string : string) : Observation = 
    raise (NotImplementedException ())

let loadObservations (stationNumber : int) (dateFrom : DateTime) (dateTo : DateTime) : Observation list = 
    getRequestUrl stationNumber dateFrom dateTo
        |> Http.RequestString 
        |> splitString '\n' 
        |> List.ofArray 
        |> List.map parseObservation
