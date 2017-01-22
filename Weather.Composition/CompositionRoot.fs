module Weather.Composition.CompositionRoot

open Weather.Utils
open Weather.Persistence

let fillNewData (stationNumber : string) (interval: DateTimeInterval) : unit =
    Weather.Filler.fillNewData 
        DbService.getLastObservationTime 
        DbService.saveObservations
        ObservationProvider.fetchObservationsByInterval
        stationNumber
        interval
