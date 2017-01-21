module Weather.Filler

open System
open Weather.Model

type DateTimeInterval = {
    From : DateTime;
    To: DateTime
}

let inline (|??) (a: 'a option) b = 
    if a.IsSome then a.Value else b  

let fillNewData 
        (getLastObservationTime : string -> DateTimeInterval -> DateTime option)
        (saveObservations : Observation list -> unit)
        (fetchObservations : string -> DateTimeInterval -> Observation list)
        (stationNumber : string)
        (interval: DateTimeInterval)
        : unit =
    let lastObservationTime = getLastObservationTime stationNumber interval
    let actualInterval = {interval with From = (lastObservationTime |?? interval.From)}
    if (actualInterval.From <= actualInterval.To) then
        let observations = fetchObservations stationNumber actualInterval
        saveObservations observations
    else
        ()