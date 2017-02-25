module Weather.Composition.CompositionRoot

open Weather.Utils
open Weather.Persistence
open Weather.DataProvider
open Weather.Logic

let fillNewData 
        (connectionString : string) 
        (stationNumber : int) 
        (interval: DateTimeInterval) 
        : unit =
    Weather.Logic.Observations.getNewData 
        (DbService.getLastObservationTime connectionString)
        ObservationsProvider.fetchObservationsByInterval
        stationNumber
        interval
    |> DbService.saveObservations connectionString
