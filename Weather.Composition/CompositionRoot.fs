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
    Weather.Logic.Observations.fillNewData 
        (DbService.getLastObservationTime connectionString)
        ObservationsProvider.fetchObservationsByInterval
        (DbService.saveObservations connectionString)
        stationNumber
        interval
