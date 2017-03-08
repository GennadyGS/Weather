module Weather.Composition.CompositionRoot

open Weather.Utils
open Weather.Persistence
open Weather.DataProvider
open Weather.Logic

let fillNewData connectionString stationNumber interval =
    DbService.getLastObservationTime connectionString stationNumber interval
        |> Weather.Logic.Observations.getMissingInterval interval
        |> Option.map (ObservationsProvider.fetchObservationsByInterval stationNumber)
        |> Option.map (DbService.saveParseObservationsResults connectionString)
