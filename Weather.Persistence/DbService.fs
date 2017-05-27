module Weather.Persistence.DbService

open FSharp.Data.Sql
open Weather.Utils
open Weather.Model
open System
open System.Linq
open Weather.Utils.Nullable
open Weather.Utils.Database

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
    row.RequestTime <- observation.Header.RequestTime
    let (StationNumber stationNumber) = observation.Header.StationNumber
    row.StationNumber <- stationNumber
    row.Date <- observation.Header.ObservationTime.Date
    row.Hour <- observation.Header.ObservationTime.Hour
    row.Temperature <- observation.Temperature

let insertObservationParsingError (dataContext : DataContext) (observationHeader, errorText) =
    let row = dataContext.InnerDataContext.Dbo.ObservationParsingErrors.Create()
    row.RequestTime <- observationHeader.RequestTime
    let (StationNumber stationNumber) = observationHeader.StationNumber
    row.StationNumber <- stationNumber
    row.Date <- observationHeader.ObservationTime.Date
    row.Hour <- observationHeader.ObservationTime.Hour
    row.ErrorText <- errorText

let private getObservationTaskStations (dataContext : DataContext) =
    query {
        for station in dataContext.InnerDataContext.Dbo.Stations do
        join task in dataContext.InnerDataContext.Dbo.CollectObservationTasks on (string station.Number = task.StationNumberMask)
        select station
    }

let private getStationsByNumbers (dataContext : DataContext) stationNumbers =
    query {
        for station in dataContext.InnerDataContext.Dbo.Stations do
        where (station.Number |=| stationNumbers)
    }

let private getStationNumbersAndObservations 
        (dataContext : DataContext) 
        (stations : IQueryable<SqlProvider.dataContext.``dbo.StationsEntity``>) = 
    query {
        for station in stations do
        for observation in (!!) station.``dbo.Observations by Number`` do
        select 
            (if station.Number = observation.StationNumber then
                (station.Number, Some observation)
            else
                (station.Number, None))
    } 
    |> runQuerySafe

let getStationNumbersAndObservationsByStationNumbers (dataContext : DataContext) stationNumbers = 
    getStationNumbersAndObservations dataContext (getStationsByNumbers dataContext stationNumbers)

let getStationNumbersAndObservationsByTasks (dataContext : DataContext) = 
    getStationNumbersAndObservations dataContext (getObservationTaskStations dataContext)

// TODO: Add optional station number and interval parameters
let getObservations (dataContext : DataContext) = 
    let observationsTable = dataContext.InnerDataContext.Dbo.Observations
    query {
        for o in observationsTable do
        select {
            Header = 
                { StationNumber = StationNumber o.StationNumber
                  ObservationTime = 
                    { Date = o.Date 
                      Hour = o.Hour }
                  RequestTime = o.RequestTime }
            Temperature = o.Temperature
        }
    }
    |> runQuerySafe

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
    |> runQuerySafe

// Collect observation tasks

let insertCollectObservationTask (dataContext : DataContext) 
        (stationNumberMask,  collectStartDate, collectIntervalHours) =
    let row = dataContext.InnerDataContext.Dbo.CollectObservationTasks.Create()
    row.StationNumberMask <- stationNumberMask
    row.CollectStartDate <- collectStartDate
    row.CollectIntervalHours <- collectIntervalHours
