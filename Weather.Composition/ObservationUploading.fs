module Weather.Composition.ObservationsUploading

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

let private combineSuccesses results =
    let (successes, falures) = ListPartition.partition results
    let successList = if not (List.isEmpty successes) then [Success successes] else []
    successList @ (falures |> List.map Failure)
    
let fillNewDataForStations connectionString minTimeSpan interval stationList =
    DbService.getLastObservationTimeList connectionString (stationList, interval)
    |> List.choose 
        (Result.mapToOption
            (Tuple.mapSecondOption
                (Weather.Logic.Observations.getMissingTrailingInterval minTimeSpan interval)))
    |> List.collect (Result.bindToList (ObservationsProvider.fetchObservationsByInterval))
    |> combineSuccesses 
    |> List.map (Result.bind (DbService.insertObservationList connectionString))
    |> FailureHandling.handleFailures (handleInvalidObservationFormats connectionString)