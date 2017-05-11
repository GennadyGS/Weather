module Weather.Logic.FailureHandling

open Weather.Model

let logFailure logFunc failure = 
    let errorMessage = 
        match failure with
        | DatabaseError message -> sprintf "Database error: %s" message
        | HttpError (statusCode, message) -> sprintf "Http error %d: %s" (int statusCode) message
        | InvalidObservationHeaderFormat message -> sprintf "Invalid observation header format: %s" message
        | InvalidObservationFormat (header, message) -> sprintf "Invalid observation format (header: %A): %s" header message
    logFunc errorMessage