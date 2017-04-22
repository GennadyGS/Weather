module Weather.Composition.ObservationsUploading

open Weather.Model
open Weather.Utils
open Weather.Persistence
open Weather.Diagnostic
open Weather.DataProvider
open Weather.Logic

let private handleInvalidObservationFormats connectionString = function
    | InvalidObservationFormat (header, message) -> 
        let errorMessage = sprintf "Invalid observation header format (header: %A): %s" header message
        Logger.logError errorMessage
        DbService.insertObservationParsingErrorList connectionString [(header, message)]
        |> Result.mapBoth (fun _ -> None) Some
    | value -> Some value

let saveObservationsAndHandleErrors connectionString = 
    ResultList.combineSuccesses 
    >> List.map 
        (Result.bind 
            (DbService.insertObservationList connectionString))
    >> FailureHandling.handleFailures 
        (handleInvalidObservationFormats connectionString)

let private tryGetMissingTrailingStationInterval minTimeSpan interval =
    Tuple.mapSecondOption 
        (Intervals.tryGetMissingTrailingInterval minTimeSpan interval)

let fetchObservationsForLastObservationTime minTimeSpan interval = 
    tryGetMissingTrailingStationInterval minTimeSpan interval
    >> Option.toList
    >> List.collect (ObservationsProvider.fetchObservationsByInterval)

let fillNewDataForStations minTimeSpan connectionString interval stationList =
    DbService.getLastObservationTimeListForStations connectionString interval stationList
    |> List.collect 
        (Result.bindToList
            (fetchObservationsForLastObservationTime minTimeSpan interval))
    |> saveObservationsAndHandleErrors connectionString
