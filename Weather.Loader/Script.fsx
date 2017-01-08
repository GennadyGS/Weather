// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

// Define your library scripting code here

#load "Scripts/load-project-debug.fsx"

open Weather.Model
open System

let private getMissingObservationTimes 
        (observationTimes : ObservationTime seq)
        (dateFrom : DateTime, dateTo : DateTime) =
    Seq.empty

let run getSavedObservations 
        requestObservations 
        (stationNumber : int) 
        (dateFrom : DateTime, dateTo : DateTime) : unit =
    let (savedObservationTimes : ObservationTime seq, saveObservations : (Observation seq -> unit)) = 
        getSavedObservations stationNumber
    let missingObservationTimes = getMissingObservationTimes savedObservationTimes (dateFrom, dateTo)
    let observations = requestObservations missingObservationTimes
    saveObservations observations