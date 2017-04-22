module FailureHandling

open Weather.Model
open Weather.Diagnostic

let private logFailure failure = 
    let errorMessage = 
        match failure with
        | InvalidHeaderFormat message -> sprintf "Invalid header format: %s" message
        | DatabaseError message -> sprintf "Database error: %s" message
        | InvalidObservationFormat _ -> sprintf "Unexpected error: %A" failure
    Logger.logError errorMessage
    None

let logFailures results = 
    Weather.Utils.FailureHandling.handleFailures logFailure results
