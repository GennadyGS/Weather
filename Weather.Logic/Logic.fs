﻿module Weather.Logic

open System
open Weather.Utils
open Weather.Utils.Result
open Weather.Model

let inline (|??) (a: 'a option) b = 
    if a.IsSome then a.Value else b  

let fillNewData 
        (getLastObservationTime : int -> DateTimeInterval -> DateTime option)
        (saveObservations : Observation list -> unit)
        (fetchObservations : int -> DateTimeInterval -> Result<Observation, string> list)
        (stationNumber : int)
        (interval: DateTimeInterval)
        : unit =
    let lastObservationTime = getLastObservationTime stationNumber interval
    let actualFromInterval = lastObservationTime |> Option.map (fun d -> d.AddMinutes(1.0))
    let actualInterval = {interval with From = (actualFromInterval |?? interval.From)}
    if (actualInterval.From <= actualInterval.To) then
        let observations = fetchObservations stationNumber actualInterval
        observations 
            |> List.choose (function
                | Success observation -> Some observation
                | Failure _ -> None)
            |> saveObservations
    else
        ()