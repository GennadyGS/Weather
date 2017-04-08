module Weather.Logic.Observations

open System
open Weather.Utils
open Weather.Utils.DateTime
open Weather.Utils.Option
open Weather.Utils.Result
open Weather.Model

let getMissingInterval minTimeSpan interval lastObservationTime =
    let actualIntervalFrom = 
        lastObservationTime 
        |> Option.map (fun (d : DateTime) -> d + minTimeSpan)
        |> Option.map (max interval.From)
        |?? interval.From
    if actualIntervalFrom <= interval.To then
        Some { interval with From = actualIntervalFrom }
    else
        None

let private partitionFailureResults = 
    Weather.Utils.List.mapAndPartition (function
        | InvalidObservationFormat value -> True value
        | InvalidHeaderFormat value -> False value)

let partitionResults results = 
    let (successResults, failureResults) = 
        Weather.Utils.List.partition results
    let (invalidObservationFormatResults, invalidHeaderFormatResults) = 
        partitionFailureResults failureResults
    { Success = successResults
      WithInvalidObservationFormat = invalidObservationFormatResults
      WithInvalidHeaderFormat = invalidHeaderFormatResults }
