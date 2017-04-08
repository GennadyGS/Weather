module Weather.Composition.CompositionRoot

open Weather.Utils
open Weather.Persistence
open Weather.DataProvider
open Weather.Logic

let private processResults connectionString results =
    let (observations, invalidObservarions, failures) = Weather.Logic.Results.partitionResults results
    DbService.insertParseObservationsResultList connectionString (observations, invalidObservarions)
    |> ignore
    // TODO: Log failures

// TODO: Move to Results module
let mapToList func result = 
    match result with
        | Success success -> func success
        | Failure failure -> [Failure failure]

let fillNewDataForStations connectionString minTimeSpan interval stationList =
    DbService.getLastObservationTimeList connectionString (stationList, interval)
        |> List.map
                (Result.map
                        (Tuple.mapSecondOption 
                            (Weather.Logic.Observations.getMissingInterval minTimeSpan interval)))
        |> List.collect (mapToList (ObservationsProvider.fetchObservationsByIntervalOption))
        |> processResults connectionString
