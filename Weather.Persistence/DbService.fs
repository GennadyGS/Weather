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

type private DataContext = 
    SqlProvider.dataContext

// Utilities

let private (|DatabaseFailure|_|) (ex : Exception) = 
    match ex with
    | :? System.Data.SqlClient.SqlException as sqlEx -> 
        sqlEx.ToString() 
        |> DatabaseError 
        |> Failure 
        |> Some
    | _ -> None
    
let private mapContextReadFunc (func : DataContext -> IQueryable<'a>) = 
    let compositeFunc = SqlProvider.GetDataContext >> func
    fun connectionString -> 
        try
            compositeFunc connectionString 
            |> Seq.toList
            |> Success
        with
          | DatabaseFailure failure -> failure

let private mapContextReadFunc2 (func : DataContext -> 'a -> IQueryable<'b>) = 
    fun connectionString a -> 
        mapContextReadFunc (fun dataContext -> func dataContext a) connectionString

let private mapContextReadFunc3 (func : DataContext -> 'a -> 'b -> IQueryable<'c>) = 
    fun connectionString a b -> 
        mapContextReadFunc (fun dataContext -> func dataContext a b) connectionString

let private mapContextUpdateFunc (func : DataContext -> 'a -> unit) = 
    fun connectionString arg ->
        try
            let dataContext = SqlProvider.GetDataContext connectionString 
            let result = func dataContext arg
            dataContext.SubmitUpdates()
            Success result
        with
          | DatabaseFailure failure -> failure

// Observations

let private insertObservation (dataContext : DataContext) observation =
    let row = dataContext.Dbo.Observations.Create()
    // TODO: insert request time 
    row.RequestTime <- DateTime.UtcNow
    let (StationNumber stationNumber) = observation.Header.StationNumber
    row.StationNumber <- stationNumber
    row.Date <- observation.Header.ObservationTime.Date
    row.Hour <- observation.Header.ObservationTime.Hour
    row.Temperature <- observation.Temperature

let private insertObservationListInternal (dataContext : DataContext) observations =
    observations 
    |> List.map (insertObservation dataContext) 
    |> ignore

let insertObservationList = mapContextUpdateFunc insertObservationListInternal

let private insertObservationParsingError (dataContext : DataContext) (observationHeader, errorText) =
    let row = dataContext.Dbo.ObservationParsingErrors.Create()
    // TODO: insert request time 
    row.RequestTime <- DateTime.UtcNow
    let (StationNumber stationNumber) = observationHeader.StationNumber
    row.StationNumber <- stationNumber
    row.Date <- observationHeader.ObservationTime.Date
    row.Hour <- observationHeader.ObservationTime.Hour
    row.ErrorText <- errorText

let private insertObservationParsingErrorListInternal (dataContext : DataContext) observationParsingErrorList =
    observationParsingErrorList
    |> List.map (insertObservationParsingError dataContext) 
    |> ignore

let insertObservationParsingErrorList = mapContextUpdateFunc insertObservationParsingErrorListInternal

// TODO: Add optional station number and interval parameters
let private getObservationsInternal (dataContext : DataContext) = 
    let observationsTable = dataContext.Dbo.Observations
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

let getObservations = mapContextReadFunc getObservationsInternal

// Last observation times

let private getLastObservationTimeListForStationsInternal 
        (dataContext : DataContext) 
        (interval : DateTimeInterval)
        (stationNumberList : StationNumber list) =
    // TODO: decompose and reuse queries
    let observationsQuery = query {
        for o in dataContext.Dbo.Observations do
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

let getLastObservationTimeListForStations = mapContextReadFunc3 getLastObservationTimeListForStationsInternal

// Collect observation tasks

let private insertCollectObservationTaskInternal (dataContext : DataContext) 
        (stationNumberMask,  collectStartDate, collectIntervalHours) =
    let row = dataContext.Dbo.CollectObservationTasks.Create()
    row.StationNumberMask <- stationNumberMask
    row.CollectStartDate <- collectStartDate
    row.CollectIntervalHours <- collectIntervalHours
    
let insertCollectObservationTask = mapContextUpdateFunc insertCollectObservationTaskInternal
