module Weather.HttpClient.HttpClient

open System
open FSharp.Data

let private getHttpResponseBodyText = 
    function
    | Text text -> text
    | Binary _ -> String.Empty

let httpGet (baseUrl : Uri) queryParams =
    let response : FSharp.Data.HttpResponse = Http.Request (baseUrl.ToString(), query = queryParams, silentHttpErrors = true)
    let statusCode = LanguagePrimitives.EnumOfValue response.StatusCode
    let bodyText = getHttpResponseBodyText response.Body
    (statusCode, bodyText)

