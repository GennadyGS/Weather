module Weather.Composition.CompositionRoot

open Weather.Model
open Weather.Utils
open Weather.Persistence
open Weather.Diagnostic
open Weather.DataProvider

let private handleInvalidObservationFormats connectionString = function
    | InvalidObservationFormat (header, message) -> 
        let errorMessage = sprintf "Invalid observation header format (header: %A): %s" header message
        Logger.logError errorMessage
        DbService.insertObservationParsingErrorList connectionString [(header, message)]
        |> Result.mapBoth (fun _ -> None) Some
    | value -> Some value

let private logFailure failure = 
    let errorMessage = 
        match failure with
        | InvalidHeaderFormat message -> sprintf "Invalid header format: %s" message
        | DatabaseError message -> sprintf "Database error: %s" message
        | InvalidObservationFormat _ -> sprintf "Unexpected error: %A" failure
    Logger.logError errorMessage
    None

let private combineSuccesses results =
    let (successes, falures) = List.partition results
    let successList = if not (List.isEmpty successes) then [Success successes] else []
    successList @ (falures |> List.map Failure)
    
let private handleFailures handler results = 
    results
    |> List.choose (Result.mapFailureToOption handler)

let private processResults connectionString results =
    results
    |> combineSuccesses 
    |> List.map (Result.bind (DbService.insertObservationList connectionString))
    |> handleFailures (handleInvalidObservationFormats connectionString)
    |> handleFailures logFailure
    |> ignore

let fillNewDataForStations connectionString minTimeSpan interval stationList =
    DbService.getLastObservationTimeList connectionString (stationList, interval)
    |> List.choose 
        (Result.mapToOption
            (Tuple.mapSecondOption
                (Weather.Logic.Observations.getMissingInterval minTimeSpan interval)))
    |> List.collect (Result.bindToList (ObservationsProvider.fetchObservationsByInterval))
    |> processResults connectionString
