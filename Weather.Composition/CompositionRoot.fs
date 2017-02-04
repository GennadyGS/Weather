module Weather.Composition.CompositionRoot

open Weather.Utils
open Weather.Persistence
open Weather.DataProvider

let fillNewData 
        (connectionString : string) 
        (stationNumber : string) 
        (interval: DateTimeInterval) 
        : unit =
    Weather.Filler.fillNewData 
        (DbService.getLastObservationTime connectionString)
        (DbService.saveObservations connectionString)
        ObservationsProvider.fetchObservationsByInterval
        stationNumber
        interval
