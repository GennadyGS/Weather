module Weather.Composition.CompositionRoot

open Weather.Utils
open Weather.Persistence
open Weather.DataProvider
open Weather.Logic

let fillNewDataForStations connectionString minTimeSpan stationList interval =
    DbService.getLastObservationTimesForStations connectionString stationList interval
        |> List.choose (Tuple.mapSecondOption (Weather.Logic.Observations.getMissingInterval minTimeSpan interval))
        |> List.map (ObservationsProvider.fetchObservationsByInterval)
        |> List.map (DbService.insertParseObservationsResultList connectionString)
        |> ignore
