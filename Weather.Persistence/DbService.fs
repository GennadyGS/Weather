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

let private insertObservation (dataContext : DataContext) observation =
    let row = dataContext.Dbo.Observations.Create()
    // TODO: insert request time 
    row.RequestTime <- DateTime.UtcNow
    row.StationNumber <- observation.Header.StationNumber
    row.Date <- observation.Header.ObservationTime.Date
    row.Hour <- observation.Header.ObservationTime.Hour
    row.Temperature <- observation.Temperature

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

// TODO: Rename to save... for consistency
let private saveObservationsInternal (dataContext : DataContext) observations =
    observations 
    |> List.map (insertObservation dataContext) 
    |> ignore

let saveObservations = mapContextUpdateFunc saveObservationsInternal

let private saveObservationParsingErrorsInternal (dataContext : DataContext) observationParsingErrors =
    observationParsingErrors 
    |> List.map (insertObservationParsingError dataContext) 
    |> ignore

let private saveObservationHeaderParsingErrorsInternal (dataContext : DataContext) observationHeaderParsingErrors =
    observationHeaderParsingErrors
    |> List.map (insertObservationHeaderParsingError dataContext) 
    |> ignore

let private saveParseObservationsResultsInternal (dataContext : DataContext) observationResults =
    observationResults.Success
    |> saveObservationsInternal dataContext

    observationResults.WithInvalidObservationFormat
    |> saveObservationParsingErrorsInternal dataContext
    
    observationResults.WithInvalidHeaderFormat
    |> saveObservationHeaderParsingErrorsInternal dataContext

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

let private insertCollectObservationTaskInternal (dataContext : DataContext) 
        (stationNumberMask,  collectStartDate, collectIntervalHours) =
    let row = dataContext.Dbo.CollectObservationTasks.Create()
    row.StationNumberMask <- stationNumberMask
    row.CollectStartDate <- collectStartDate
    row.CollectIntervalHours <- collectIntervalHours
    
let insertCollectObservationTask = mapContextUpdateFunc insertCollectObservationTaskInternal
