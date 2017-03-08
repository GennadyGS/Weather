module Weather.Persistence.DbService

open FSharp.Data.Sql
open Weather.Utils
open Weather.Model
open System

type private SqlProvider = 
    SqlDataProvider<
        ConnectionStringName = "Weather",
        UseOptionTypes = true>

type private DataContext = 
    SqlProvider.dataContext

let private insertObservation 
        (observationsTable : SqlProvider.dataContext.dboSchema.``dbo.Observations``) 
        observation =
    let row = observationsTable.Create()
    row.StationNumber <- observation.Header.StationNumber
    row.Date <- observation.Header.ObservationTime.Date
    row.Hour <- observation.Header.ObservationTime.Hour
    row.Temperature <- observation.Temperature

let private saveObservationsInternal (dataContext : DataContext) observations =
    observations 
    |> List.map (insertObservation dataContext.Dbo.Observations) 
    |> ignore

let saveObservations connectionString observations = 
    let dataContext = SqlProvider.GetDataContext connectionString 
    saveObservationsInternal dataContext observations
    dataContext.SubmitUpdates()

let private saveParseObservationsResultsInternal 
        (dataContext : DataContext) 
        observationResults =
    observationResults.Success
    |> saveObservationsInternal dataContext
    // TODO: save failures

let saveParseObservationsResults connectionString observations = 
    let dataContext = SqlProvider.GetDataContext connectionString 
    saveParseObservationsResultsInternal dataContext observations
    dataContext.SubmitUpdates()

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

let getObservations connectionString = 
    connectionString 
    |> SqlProvider.GetDataContext 
    |> getObservationsInternal

let private getLastObservationTimeInternal (dataContext : DataContext) stationNumber interval = 
    let observationsTable = dataContext.Dbo.Observations
    let observationsQuery = query {
        for o in observationsTable do
        select (o.StationNumber, o.Date.AddHours(float(o.Hour)))
    }
    query {
        for (stNumber, observationTime) in observationsQuery do
        where (stNumber = stationNumber && 
            observationTime >= interval.From 
            && observationTime <= interval.To)
        maxBy (Some (observationTime))
    }

let getLastObservationTime connectionString = 
    connectionString 
    |> SqlProvider.GetDataContext 
    |> getLastObservationTimeInternal
