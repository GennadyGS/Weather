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

let private mapContextReadFunc func = 
    SqlProvider.GetDataContext >> func

let private mapContextUpdateFunc func = 
    fun connectionString arg ->
        let dataContext = SqlProvider.GetDataContext connectionString 
        let result = func dataContext arg
        dataContext.SubmitUpdates()
        result

// Observations

let private insertObservation (dataContext : DataContext) observation =
    let row = dataContext.Dbo.Observations.Create()
    // TODO: insert request time 
    row.RequestTime <- DateTime.UtcNow
    row.StationNumber <- observation.Header.StationNumber
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
    row.StationNumber <- observationHeader.StationNumber
    row.Date <- observationHeader.ObservationTime.Date
    row.Hour <- observationHeader.ObservationTime.Hour
    row.ErrorText <- errorText

let private insertObservationHeaderParsingError (dataContext : DataContext) errorText =
    let row = dataContext.Dbo.ObservationHeaderParsingErrors.Create()
    // TODO: insert request time 
    row.RequestTime <- DateTime.UtcNow
    row.ErrorText <- errorText

let private insertParseObservationsResultListInternal (dataContext : DataContext) observationResults =
    observationResults.Success
    |> insertObservationListInternal dataContext

    observationResults.WithInvalidObservationFormat
    |> List.map (insertObservationParsingError dataContext) 
    |> ignore
    
    observationResults.WithInvalidHeaderFormat
    |> List.map (insertObservationHeaderParsingError dataContext) 
    |> ignore

let insertParseObservationsResultList = mapContextUpdateFunc insertParseObservationsResultListInternal

let private getObservationsInternal (dataContext : DataContext) : Observation list = 
    let observationsTable = dataContext.Dbo.Observations
    query {
        for o in observationsTable do
        select {
            Header = 
                { ObservationTime = 
                    { Date = o.Date 
                      Hour = o.Hour }
                  StationNumber = o.StationNumber }
            Temperature = o.Temperature
        }
    } |> List.ofSeq

let getObservations = mapContextReadFunc getObservationsInternal

// Last observation times

let private getLastObservationTimeInternal (dataContext : DataContext) stationNumber interval = 
    let observationsQuery = query {
        for o in dataContext.Dbo.Observations do
        select (o.StationNumber, o.Date.AddHours(float(o.Hour)))
    }
    query {
        for (stNumber, observationTime) in observationsQuery do
        where (stNumber = stationNumber && 
            observationTime >= interval.From 
            && observationTime <= interval.To)
        maxBy (Some (observationTime))
    }

let getLastObservationTime = mapContextReadFunc getLastObservationTimeInternal

let private getLastObservationTimeListInternal 
        (dataContext : DataContext) 
        (stationNumberList : int list) 
        (interval : DateTimeInterval) = 
    let observationsQuery = query {
        for o in dataContext.Dbo.Observations do
        select (o.StationNumber, o.Date.AddHours(float(o.Hour)))
    }
    let query2 = query {
        for stationNumber in stationNumberList.AsQueryable()  do
        leftOuterJoin (stNumber, observationTime) in observationsQuery
                on (stationNumber = stNumber) into result
        for (stNumber, observationTime) in result do
        let rr = match (stNumber, observationTime) with
            | Unchecked.defaultof<int * DateTime> -> (stNumber, Some observationTime)
            | _ -> (stNumber, None))
        select rr
    }

    query {
        for (stNumber, observationTime) in query2 do
        let maxObservationTime = 
            query { 
                for (_, observationTime) in result do 
                maxBy (Some observationTime) 
            }
        select (stNumber, maxObservationTime)
    } |> List.ofSeq

let getLastObservationTimeList = mapContextReadFunc getLastObservationTimeListInternal

// Collect observation tasks

let private insertCollectObservationTaskInternal (dataContext : DataContext) 
        (stationNumberMask,  collectStartDate, collectIntervalHours) =
    let row = dataContext.Dbo.CollectObservationTasks.Create()
    row.StationNumberMask <- stationNumberMask
    row.CollectStartDate <- collectStartDate
    row.CollectIntervalHours <- collectIntervalHours
    
let insertCollectObservationTask = mapContextUpdateFunc insertCollectObservationTaskInternal
