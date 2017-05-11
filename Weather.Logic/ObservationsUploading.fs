module Weather.Logic.ObservationsUploading

open Weather.Utils
open Weather.Model
open Weather.Logic

// TODO: To utils?
let private tee sideEffect =
    fun x ->
        do sideEffect x
        x

let private tryGetMissingTrailingStationInterval minTimeSpan interval =
    Tuple.mapSecondOption 
        (Intervals.tryGetMissingTrailingInterval minTimeSpan interval)

let fetchObservationsForLastObservationTimeList fetchObservationsByIntervalFunc minTimeSpan interval = 
    List.choose (tryGetMissingTrailingStationInterval minTimeSpan interval)
    >> List.collect fetchObservationsByIntervalFunc

let private handleFailure insertObservationParsingErrorFunc dataContext = function
    | InvalidObservationFormat (header, message) -> 
        insertObservationParsingErrorFunc dataContext (header, message)
    | _ -> ()

let saveObservationsAndHandleErrors 
        insertObservationFunc 
        insertObservationParsingErrorFunc
        logErrorFunc
        dataContext = 
    List.map 
        (Result.map (insertObservationFunc dataContext))
    >> List.map 
        (Result.mapFailure 
            (tee (Weather.Logic.FailureHandling.logFailure logErrorFunc) 
            >> (handleFailure insertObservationParsingErrorFunc dataContext)))
    >> Result.ignoreAll
