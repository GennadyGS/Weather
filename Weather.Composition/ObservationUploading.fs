module Weather.Composition.ObservationsUploading

open Weather
open Weather.Model
open Weather.Utils
open Weather.Persistence
open Weather.Diagnostic
open Weather.DataProvider
open Weather.Logic

let private saveObservationsAndHandleErrors = 
    Logic.ObservationsUploading.saveObservationsAndHandleErrors
        DbService.insertObservationList
        DbService.insertObservationParsingErrorList

let private fetchObservationsForLastObservationTimeList = 
    Logic.ObservationsUploading.fetchObservationsForLastObservationTimeList 
        (ObservationsProvider.fetchObservationsByInterval HttpClient.HttpClient.httpGet)

let fillNewDataForStations minTimeSpan connectionString interval stationList =
    DbService.getLastObservationTimeListForStations connectionString interval stationList
    |> Result.bindToList (fetchObservationsForLastObservationTimeList minTimeSpan interval)
    |> saveObservationsAndHandleErrors connectionString
