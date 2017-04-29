module Weather.Logic.HttpClient

open System.Net
open Weather.Utils
open Weather.Model

let safeHttpGet httpGet baseUrl queryParams = 
    let (statusCode, bodyText) = httpGet baseUrl queryParams
    match statusCode with
    | HttpStatusCode.OK -> Success bodyText
    | _ -> HttpError (statusCode, bodyText) |> Failure

