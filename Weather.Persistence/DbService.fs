module Weather.Persistence.DbService

open FSharp.Data.Sql
open Weather.Utils
open Weather.Model
open System

type private SqlProvider = 
    SqlDataProvider<
        ConnectionStringName = "Weather",
        UseOptionTypes = true>

let private insertObservation 
        (observationsTable : SqlProvider.dataContext.dboSchema.``dbo.Observations``) 
        (observation: Observation) =
    let row = observationsTable.Create()
    row.StationNumber <- observation.StationNumber
    row.Date <- observation.Time.Date
    row.Hour <- observation.Time.Hour
    row.Temperature <- observation.Temperature

let saveObservations (connectionString : string) (observations: Observation seq) : unit =
    let dataContext = SqlProvider.GetDataContext connectionString
    let observationsTable = dataContext.Dbo.Observations
    observations |> Seq.map (insertObservation observationsTable) |> Seq.toArray |> ignore
    dataContext.SubmitUpdates()

let getObservations (connectionString : string) : Observation list = 
    let dataContext = SqlProvider.GetDataContext connectionString
    let observationsTable = dataContext.Dbo.Observations
    query {
        for o in observationsTable do
        select {
            Time = 
                {
                    Date = o.Date; 
                    Hour = o.Hour;
                };
            StationNumber = o.StationNumber;
            Temperature = o.Temperature
        }
    } |> List.ofSeq

let getLastObservationTime 
        (connectionString : string) 
        (stationNumber : int) 
        (interval : DateTimeInterval) 
        : DateTime option = 
    let dataContext = SqlProvider.GetDataContext connectionString
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
