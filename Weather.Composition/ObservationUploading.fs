﻿module Weather.Composition.ObservationsUploading

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

let fillStationIntervals connectionString = 
    List.collect 
        (Result.bindToList ObservationsProvider.fetchObservationsByInterval)
    >> ResultList.combineSuccesses 
    >> List.map 
        (Result.bind 
            (DbService.insertObservationList connectionString))
    >> FailureHandling.handleFailures 
        (handleInvalidObservationFormats connectionString)

let private tryGetMissingTrailingStationInterval minTimeSpan interval =
    Tuple.mapSecondOption 
        (Intervals.tryGetMissingTrailingInterval minTimeSpan interval)

let fillNewDataFromLastObservationTimeList minTimeSpan connectionString interval =
    List.choose 
        (Result.mapToOption 
            (tryGetMissingTrailingStationInterval minTimeSpan interval))
    >> fillStationIntervals connectionString

let fillNewDataForStations minTimeSpan connectionString interval stationList =
    DbService.getLastObservationTimeList connectionString interval stationList
    |> fillNewDataFromLastObservationTimeList minTimeSpan connectionString interval
