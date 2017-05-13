module Weather.Composition.ObservationsUploading

open Weather
open Weather.Utils
open Weather.Logic
open Weather.Persistence
open Weather.Diagnostic
open Weather.Composition

let private saveObservationsToDataContextAndHandleErrors = 
    Logic.ObservationsUploading.saveObservationsToDataContextAndHandleErrors
        DbService.insertObservation
        DbService.insertObservationParsingError
        Logger.logError

let private saveObservationsAndHandleErrors connectionString =
    Database.writeDataContext 
        saveObservationsToDataContextAndHandleErrors connectionString
    >> Result.mapFailure (Logic.FailureHandling.logFailure Logger.logError)
    >> Result.ignore

let private fetchObservationsForLastObservationTimeList = 
    Logic.ObservationsUploading.fetchObservationsForLastObservationTimeList 
        ObservationProviders.Ogimet.fetchObservationsByInterval

let fillNewDataForStations minTimeSpan connectionString interval stationList =
    Database.readDataContext 
        DbService.getLastObservationTimeListForStations connectionString (interval, stationList)
    |> Result.bindToList (fetchObservationsForLastObservationTimeList minTimeSpan interval)
    |> saveObservationsAndHandleErrors connectionString