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

let getMissingNewStationIntervals connectionString minTimeSpan interval stationList =
    DbService.getLastObservationTimeList connectionString (stationList, interval)
    |> List.choose 
        (Result.mapToOption
            (Tuple.mapSecondOption
                (Weather.Logic.Intervals.getMissingTrailingInterval minTimeSpan interval)))

let fillStationIntervals connectionString stationIntervals = 
    stationIntervals
    |> List.collect ObservationsProvider.fetchObservationsByInterval
    |> ResultList.combineSuccesses 
    |> List.map 
        (Result.bind 
            (DbService.insertObservationList connectionString))
    |> FailureHandling.handleFailures 
        (handleInvalidObservationFormats connectionString)

let fillNewDataForStations connectionString minTimeSpan interval stationList =
    getMissingNewStationIntervals connectionString minTimeSpan interval stationList
    |> ResultList.combineSuccesses 
    |> List.collect 
        (Result.bindToList 
            (fillStationIntervals connectionString))

