module Weather.Composition.ObservationsUploading

open Weather
open Weather.Utils
open Weather.Logic
open Weather.Persistence
open Weather.CompositionRoot

let private saveObservationsAndHandleErrors = 
    Logic.ObservationsUploading.saveObservationsAndHandleErrors
        DbService.insertObservation
        DbService.insertObservationParsingError

let private fetchObservationsForLastObservationTimeList = 
    Logic.ObservationsUploading.fetchObservationsForLastObservationTimeList 
        ObservationProviders.Ogimet.fetchObservationsByInterval

let fillNewDataForStations minTimeSpan connectionString interval stationList =
    Database.readDataContext 
        DbService.getLastObservationTimeListForStations connectionString (interval, stationList)
    |> Result.bindToList (fetchObservationsForLastObservationTimeList minTimeSpan interval)
    |> Database.writeDataContext 
        saveObservationsAndHandleErrors connectionString
