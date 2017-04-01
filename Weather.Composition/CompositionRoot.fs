module Weather.Composition.CompositionRoot

open Weather.Utils
open Weather.Persistence
open Weather.DataProvider
open Weather.Logic

let fillNewData connectionString minTimeSpan stationNumber interval =
    DbService.getLastObservationTime connectionString stationNumber interval
        |> Tuple.mapSecondOption (Weather.Logic.Observations.getMissingInterval minTimeSpan interval)
        |> Option.map ObservationsProvider.fetchObservationsByInterval
        |> Option.map (DbService.insertParseObservationsResultList connectionString)
        |> ignore

let fillNewDataForStationList connectionString minTimeSpan stationList interval =
    DbService.getLastObservationTimeList connectionString stationList interval
        |> List.choose (Tuple.mapSecondOption (Weather.Logic.Observations.getMissingInterval minTimeSpan interval))
        |> List.map (ObservationsProvider.fetchObservationsByInterval)
        |> List.map (DbService.insertParseObservationsResultList connectionString)
        |> ignore
