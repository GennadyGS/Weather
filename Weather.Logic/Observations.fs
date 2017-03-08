﻿module Weather.Logic.Observations

open System
open Weather.Utils
open Weather.Utils.DateTime
open Weather.Utils.Option
open Weather.Utils.Result
open Weather.Model

let getMissingInterval interval lastObservationTime =
    let actualIntervalFrom = 
        lastObservationTime 
        |> Option.map (fun (d : DateTime) -> d.AddMinutes(1.0))
        |> Option.map (max interval.From)
        |?? interval.From
    if actualIntervalFrom <= interval.To then
        Some { interval with From = actualIntervalFrom }
    else
        None
