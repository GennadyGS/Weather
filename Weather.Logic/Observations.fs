﻿module Weather.Logic.Observations

open System
open Weather.Utils
open Weather.Utils.DateTime
open Weather.Utils.Option
open Weather.Utils.Result
open Weather.Model

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
        let actualIntervalFrom = max interval.From (lastObservationTimePlus1Minute |?? interval.From)
        {interval with From = actualIntervalFrom}
    
    let fetchObservationsSafe interval = 
        if interval.From <= interval.To then 
            fetchObservations stationNumber interval
        else []
    
    let actualInterval = getActualInterval interval
    actualInterval
        |> fetchObservationsSafe
        |> filterSuccess
        |> List.filter (fun observation -> 
            inside actualInterval (observation.Time.ToDateTime()))
