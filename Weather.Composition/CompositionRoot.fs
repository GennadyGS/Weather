module Weather.Composition.CompositionRoot

open Weather.Utils
open Weather.Persistence
open Weather.DataProvider
open Weather.Logic

let fillNewDataForStations connectionString minTimeSpan interval stationList =
    DbService.getLastObservationTimesForStations connectionString interval stationList
        |> List.choose (Tuple.mapSecondOption (Weather.Logic.Observations.getMissingInterval minTimeSpan interval))
        |> List.collect ObservationsProvider.fetchObservationsByInterval
        |> Weather.Logic.Observations.partitionResults 
        |> DbService.insertParseObservationsResultList connectionString
