module Weather.Filler

open System
open Weather.Model

type DateTimeInterval = {
    From : DateTime;
    To: DateTime
}

let private getMissingObservationTimes 
        (observationTimes : ObservationTime seq)
        (interval: DateTimeInterval) 
        : ObservationTime seq =
    Seq.empty

let run (getSavedObservations : int -> DateTimeInterval -> ObservationTime seq * (Observation seq -> unit))
        (requestObservations : ObservationTime seq -> Observation seq)
        (stationNumber : int) 
        (interval: DateTimeInterval) : unit =
    let (savedObservationTimes : ObservationTime seq, saveObservations : (Observation seq -> unit)) = 
        getSavedObservations stationNumber interval
    let missingObservationTimes = getMissingObservationTimes savedObservationTimes interval
    let observations = requestObservations missingObservationTimes
    saveObservations observations