// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

// Define your library scripting code here

#load "Scripts/load-project-debug.fsx"

open Weather.Model
open System


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