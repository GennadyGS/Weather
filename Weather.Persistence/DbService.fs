module Weather.Persistence.DbService

open FSharp.Data.Sql
open Weather.Utils
open Weather.Model
open System
open System.Linq

type private SqlProvider = 
    SqlDataProvider<
        ConnectionStringName = "Weather",
        UseOptionTypes = true>

type DataContext private (innerDataContext : SqlProvider.dataContext) = 
    member internal this.InnerDataContext = innerDataContext
    
    static member Create connectionString = 
        DataContext(SqlProvider.GetDataContext(connectionString))
    
    static member SaveChanges (dataContext : DataContext) = 
        dataContext.InnerDataContext.SubmitUpdates()

// Observations

let insertObservation (dataContext : DataContext) observation =
    let row = dataContext.InnerDataContext.Dbo.Observations.Create()
    // TODO: insert request time 
    row.RequestTime <- DateTime.UtcNow
    let (StationNumber stationNumber) = observation.Header.StationNumber
    row.StationNumber <- stationNumber
    row.Date <- observation.Header.ObservationTime.Date
    row.Hour <- observation.Header.ObservationTime.Hour
    row.Temperature <- observation.Temperature

let insertObservationParsingError (dataContext : DataContext) (observationHeader, errorText) =
    let row = dataContext.InnerDataContext.Dbo.ObservationParsingErrors.Create()
    // TODO: insert request time 
    row.RequestTime <- DateTime.UtcNow
    let (StationNumber stationNumber) = observationHeader.StationNumber
    row.StationNumber <- stationNumber
    row.Date <- observationHeader.ObservationTime.Date
    row.Hour <- observationHeader.ObservationTime.Hour
    row.ErrorText <- errorText

// TODO: Add optional station number and interval parameters
let getObservations (dataContext : DataContext) () = 
    let observationsTable = dataContext.InnerDataContext.Dbo.Observations
    query {
        for o in observationsTable do
        select {
            Header = 
                { ObservationTime = 
                    { Date = o.Date 
                      Hour = o.Hour }
                  StationNumber = StationNumber o.StationNumber }
            Temperature = o.Temperature
        }
    }

// Last observation times

let getLastObservationTimeListForStations
        (dataContext : DataContext) 
        (interval : DateTimeInterval, stationNumberList : StationNumber list) =
    // TODO: decompose and reuse queries
    let observationsQuery = query {
        for o in dataContext.InnerDataContext.Dbo.Observations do
        select (StationNumber o.StationNumber, o.Date.AddHours(float(o.Hour)))
    }
    
    let filteredObservationsQuery = query {
        for (stNumber, observationTime) in observationsQuery do
        where (observationTime >= interval.From 
            && observationTime <= interval.To)
    }
    
    let observationTimesByStation = query {
        for stationNumber in stationNumberList.AsQueryable() do
        leftOuterJoin (stNumber, observationTime) in filteredObservationsQuery
            on (stationNumber = stNumber) into result
        for item in result do 
        let optionalItem = Nullable.toOption item
        select (stationNumber, Option.map snd optionalItem)
    }

    query {
        for (stNumber, observationTime) in observationTimesByStation do
        groupBy stNumber into group
        let maxObservationTime = query { for (_, observationTime) in group do maxBy observationTime }
        select (group.Key, maxObservationTime)
    }

// Collect observation tasks

let insertCollectObservationTask (dataContext : DataContext) 
        (stationNumberMask,  collectStartDate, collectIntervalHours) =
    let row = dataContext.InnerDataContext.Dbo.CollectObservationTasks.Create()
    row.StationNumberMask <- stationNumberMask
    row.CollectStartDate <- collectStartDate
    row.CollectIntervalHours <- collectIntervalHours
