module Weather.Filler

open System
open Weather.Utils
open Weather.Utils.Result
open Weather.Model

let inline (|??) (a: 'a option) b = 
    if a.IsSome then a.Value else b  

let fillNewData 
        (getLastObservationTime : string -> DateTimeInterval -> DateTime option)
        (saveObservations : Observation list -> unit)
        (fetchObservations : string -> DateTimeInterval -> Result<Observation, string> list)
        (stationNumber : string)
        (interval: DateTimeInterval)
        : unit =
    let lastObservationTime = getLastObservationTime stationNumber interval
    let actualInterval = {interval with From = (lastObservationTime |?? interval.From)}
    if (actualInterval.From <= actualInterval.To) then
        let observations = fetchObservations stationNumber actualInterval
        observations 
            |> List.filter (function
                | Success _ -> true
                | Failure _ -> false)
            |> List.map (function
                | Success observation -> observation
                | Failure _ -> (raise (InvalidOperationException())))
            |> saveObservations
    else
        ()