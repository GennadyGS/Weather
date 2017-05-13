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

let private fetchObservationsForLastObservationTimeList baseProviderUrl requestTime = 
    Logic.ObservationsUploading.fetchObservationsForLastObservationTimeList 
        (ObservationProviders.Ogimet.fetchObservationsByInterval baseProviderUrl requestTime)

let fillNewDataForStations minTimeSpan connectionString baseProviderUrl interval stationList =
    let requestTime = System.DateTime.UtcNow
    Database.readDataContext 
        DbService.getLastObservationTimeListForStations connectionString (interval, stationList)
    |> Result.bindToList 
        (fetchObservationsForLastObservationTimeList baseProviderUrl requestTime minTimeSpan interval)
    |> saveObservationsAndHandleErrors connectionString