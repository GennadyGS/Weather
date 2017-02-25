module Weather.Logic.Observations

open System
open Weather.Utils
open Weather.Utils.Result
open Weather.Model

let inline (|??) (a: 'a option) b = 
    if a.IsSome then a.Value else b  

let private maxDate (date1: DateTime) (date2 : DateTime) = 
    DateTime(Math.Max(date1.Ticks, date2.Ticks))

let getNewData 
        (getLastObservationTime : int -> DateTimeInterval -> DateTime option)
        (fetchObservations : int -> DateTimeInterval -> Result<Observation, string> list)
        (stationNumber : int)
        (interval: DateTimeInterval)
        : Observation list =
    if interval.To < interval.From then raise (ArgumentException("interval"))
    
    let getActualInterval interval = 
        let lastObservationTime = getLastObservationTime stationNumber interval
        let lastObservationTimePlus1Minute = lastObservationTime |> Option.map (fun d -> d.AddMinutes(1.0))
        let actualIntervalFrom = maxDate interval.From (lastObservationTimePlus1Minute |?? interval.From)
        {interval with From = actualIntervalFrom}
    
    let fetchObservationsSafe interval = 
        if interval.From <= interval.To then 
            fetchObservations stationNumber interval
        else []
    
    interval
        |> getActualInterval
        |> fetchObservationsSafe
        |> List.choose (function
            | Success observation -> Some observation
            | Failure _ -> None)
