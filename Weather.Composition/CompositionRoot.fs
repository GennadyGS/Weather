module Weather.Composition.CompositionRoot

open Weather.Utils
open Weather.Persistence
open Weather.DataProvider
open Weather.Logic

let private processResults connectionString results =
    let partitionedResults = Weather.Logic.Results.partitionResults results
    DbService.insertParseObservationsResultList connectionString (partitionedResults.Success, partitionedResults.InvalidObservationFormatFailures)
    // TODO: Log errors

let fillNewData connectionString minTimeSpan stationList interval =
    DbService.getLastObservationTimeList connectionString (stationList, interval)
        |> List.choose (Tuple.mapSecondOption (Weather.Logic.Observations.getMissingInterval minTimeSpan interval))
        |> List.collect ObservationsProvider.fetchObservationsByInterval
        |> processResults connectionString
