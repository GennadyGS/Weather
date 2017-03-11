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

let private mapContextReadFunc func = 
    SqlProvider.GetDataContext >> func

let private mapContextUpdateFunc func = 
    fun connectionString arg ->
        let dataContext = SqlProvider.GetDataContext connectionString 
        let result = func dataContext arg
        dataContext.SubmitUpdates()
        result

let private insertObservation 
        (observationsTable : SqlProvider.dataContext.dboSchema.``dbo.Observations``) 
        observation =
    let row = observationsTable.Create()
    // TODO: Pass RequestTime with observation
    row.RequestTime <- DateTime.UtcNow
    row.StationNumber <- observation.Header.StationNumber
    row.Date <- observation.Header.ObservationTime.Date
    row.Hour <- observation.Header.ObservationTime.Hour
    row.Temperature <- observation.Temperature

let private saveObservationsInternal (dataContext : DataContext) observations =
    observations 
    |> List.map (insertObservation dataContext.Dbo.Observations) 
    |> ignore

let saveObservations = mapContextUpdateFunc saveObservationsInternal

let private saveParseObservationsResultsInternal 
        (dataContext : DataContext) observationResults =
    observationResults.Success
    |> saveObservationsInternal dataContext
    // TODO: save failures

let saveParseObservationsResults = mapContextUpdateFunc saveParseObservationsResultsInternal

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

let getLastObservationTime = mapContextReadFunc getLastObservationTimeInternal