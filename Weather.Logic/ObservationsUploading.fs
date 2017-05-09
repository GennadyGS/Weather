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

let private handleInvalidObservationFormat insertObservationParsingErrorFunc dataContext = function
    | InvalidObservationFormat (header, message) -> 
        let errorMessage = sprintf "Invalid observation format (header: %A): %s" header message
        Logger.logError errorMessage
        insertObservationParsingErrorFunc dataContext (header, message)
        None
    | value -> Some value

let saveObservationsAndHandleErrors 
        insertObservationFunc 
        insertObservationParsingErrorFunc
        dataContext = 
    List.map 
        (Result.map (insertObservationFunc dataContext))
    >> FailureHandling.handleFailures 
        (handleInvalidObservationFormat insertObservationParsingErrorFunc dataContext)
