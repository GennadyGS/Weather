module Weather.Logic.Observations

open System
open Weather.Utils
open Weather.Utils.Result
open Weather.Model

let inline (|??) (a: 'a option) b = 
    if a.IsSome then a.Value else b  

let getNewData 
        (getLastObservationTime : int -> DateTimeInterval -> DateTime option)
        (fetchObservations : int -> DateTimeInterval -> Result<Observation, string> list)
        (stationNumber : int)
        (interval: DateTimeInterval)
        : Observation list =
    let lastObservationTime = getLastObservationTime stationNumber interval
    let actualFromTime = lastObservationTime |> Option.map (fun d -> d.AddMinutes(1.0))
    let actualInterval = {interval with From = (actualFromTime |?? interval.From)}
    let observations = 
        if actualInterval.From <= actualInterval.To then
            fetchObservations stationNumber actualInterval
        else
            []
    observations
        |> List.choose (function
            | Success observation -> Some observation
            | Failure _ -> None)
