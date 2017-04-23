module Weather.Logic.ObservationsUploading

open Weather.Utils
open Weather.Model
open Weather.Diagnostic

let private tryGetMissingTrailingStationInterval minTimeSpan interval =
    Tuple.mapSecondOption 
        (Intervals.tryGetMissingTrailingInterval minTimeSpan interval)

let fetchObservationsForLastObservationTimeList fetchObservationsByIntervalFunc minTimeSpan interval = 
    List.choose (tryGetMissingTrailingStationInterval minTimeSpan interval)
    >> List.collect fetchObservationsByIntervalFunc

let private handleInvalidObservationFormats insertObservationParsingErrorListFunc connectionString = function
    | InvalidObservationFormat (header, message) -> 
        let errorMessage = sprintf "Invalid observation header format (header: %A): %s" header message
        Logger.logError errorMessage
        insertObservationParsingErrorListFunc connectionString [(header, message)]
        |> Result.mapBoth (fun _ -> None) Some
    | value -> Some value

let saveObservationsAndHandleErrors 
        insertObservationListFunc 
        insertObservationParsingErrorListFunc
        connectionString = 
    ResultList.combineSuccesses 
    >> List.map 
        (Result.bind (insertObservationListFunc connectionString))
    >> FailureHandling.handleFailures 
        (handleInvalidObservationFormats insertObservationParsingErrorListFunc connectionString)


