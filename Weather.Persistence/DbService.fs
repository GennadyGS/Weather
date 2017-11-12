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
    
    static member Create (connectionString: string) = 
        DataContext(SqlProvider.GetDataContext(connectionString))
    
    static member SaveChanges (dataContext : DataContext) = 
        dataContext.InnerDataContext.SubmitUpdates()

// Stations

let private getOrderedStationEntitiesQuery (dataContext : DataContext) =
    dataContext.InnerDataContext.Dbo.Stations
        .OrderBy(fun station -> station.Number)

let private getStationEntitiesFromObservationTasksQuery (dataContext : DataContext) =
    query {
        for stationEntity in getOrderedStationEntitiesQuery dataContext do
        join taskEntity in dataContext.InnerDataContext.Dbo.CollectObservationTasks on (string stationEntity.Number = taskEntity.StationNumberMask)
        select stationEntity
    }

let private getStationEntitiesByNumbersQuery (dataContext : DataContext) stationNumbers =
    let intStationNumbers = 
        stationNumbers 
        |> List.map (fun (StationNumber stationNumber) -> stationNumber)
    query {
        for station in getOrderedStationEntitiesQuery dataContext do
        where (station.Number |=| intStationNumbers)
    }

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

[<ReflectedDefinition>]
let entityToObservation (entity : SqlProvider.dataContext.``dbo.ObservationsEntity``) = 
    { Header = 
        { StationNumber = StationNumber entity.StationNumber
          ObservationTime = 
            { Date = entity.Date 
              Hour = entity.Hour }
          RequestTime = entity.RequestTime }
      Temperature = entity.Temperature }

let private getOrderedObservationsQuery (dataContext : DataContext) = 
    dataContext.InnerDataContext.Dbo.Observations
        .OrderBy(fun o -> o.StationNumber)
        .ThenBy(fun o -> o.Date)
        .ThenBy(fun o -> o.Hour)

// TODO: Add optional station number and interval parameters
let getObservations (dataContext : DataContext) = 
    query {
        for observationEntity in (getOrderedObservationsQuery dataContext) do
        select (entityToObservation observationEntity)
    }
    |> runQuerySafe

// Stations and observations

let private getStationNumbersAndObservationsQuery 
        (stationEntitiesQuery : IQueryable<SqlProvider.dataContext.``dbo.StationsEntity``>) = 
    query {
        for stationEntity in stationEntitiesQuery do
        for observationEntity in (!!) stationEntity.``dbo.Observations by Number`` do
        select 
            (if stationEntity.Number = observationEntity.StationNumber then
                (StationNumber stationEntity.Number, Some (entityToObservation observationEntity))
            else
                (StationNumber stationEntity.Number, None))
    } 

let private getObservationsByStationNumbersQuery 
        (stationEntitiesQuery : IQueryable<SqlProvider.dataContext.``dbo.StationsEntity``>) = 
    let stationNumberAndObservationQuery = query {
        for stationEntity in stationEntitiesQuery do
        for observationEntity in (!!) stationEntity.``dbo.Observations by Number`` do
        select (stationEntity.Number, observationEntity)
    } 
    query {
        for (number, o) in stationNumberAndObservationQuery do
        groupBy number into group
        let maxObservationTime = query {
            for (observation, _) in group do
            maxBy (observation.Date.AddHours(float(observation.Hour)))
        }
        select (StationNumber group.Key, maxObservationTime)
    } 

let getObservationsByStationNumbers (dataContext : DataContext) stationNumbers =
    getObservationsByStationNumbersQuery (getStationEntitiesByNumbersQuery dataContext stationNumbers)
    |> runQuerySafe

let getStationNumbersAndObservationsByStationNumbers (dataContext : DataContext) stationNumbers = 
    let stationEntitiesQuery = getStationEntitiesByNumbersQuery dataContext stationNumbers
    getStationNumbersAndObservationsQuery stationEntitiesQuery
    |> runQuerySafe

let getStationNumbersAndObservationsFromObservationTasks (dataContext : DataContext) = 
    let stationEntitiesQuery = getStationEntitiesFromObservationTasksQuery dataContext
    getStationNumbersAndObservationsQuery stationEntitiesQuery
    |> runQuerySafe

// Last observation times

let getLastObservationTimeListForStations
        (dataContext : DataContext) 
        (interval : DateTimeInterval, stationNumbers : StationNumber list) =
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
        for stationNumber in stationNumbers.AsQueryable() do
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
