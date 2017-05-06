module Weather.Composition.ObservationsUploading

open Weather
open Weather.Utils
open Weather.Logic.Database
open Weather.Persistence
open Weather.CompositionRoot

let private saveObservationsAndHandleErrors = 
    Logic.ObservationsUploading.saveObservationsAndHandleErrors
        (writeDataContext DbService.insertObservationList)
        (writeDataContext DbService.insertObservationParsingErrorList)

let private fetchObservationsForLastObservationTimeList = 
    Logic.ObservationsUploading.fetchObservationsForLastObservationTimeList 
        ObservationProviders.Ogimet.fetchObservationsByInterval

let fillNewDataForStations minTimeSpan connectionString interval stationList =
    readDataContext3 DbService.getLastObservationTimeListForStations connectionString interval stationList
    |> Result.bindToList (fetchObservationsForLastObservationTimeList minTimeSpan interval)
    |> saveObservationsAndHandleErrors connectionString
