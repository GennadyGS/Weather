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

let fillNewDataForStations connectionString minTimeSpan interval stationList =
    DbService.getLastObservationTimeList connectionString (stationList, interval)
        |> List.choose 
            (Result.mapToOption
                (Tuple.mapSecondOption
                    (Weather.Logic.Observations.getMissingInterval minTimeSpan interval)))
        |> List.collect (Result.bindToList (ObservationsProvider.fetchObservationsByInterval))
        |> processResults connectionString
