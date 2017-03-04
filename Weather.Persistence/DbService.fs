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
        observation =
    let row = observationsTable.Create()
    row.StationNumber <- observation.Header.StationNumber
    row.Date <- observation.Header.Time.Date
    row.Hour <- observation.Header.Time.Hour
    row.Temperature <- observation.Temperature

let saveObservations connectionString observations =
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
            Header = 
                {
                    Time = 
                        {
                            Date = o.Date; 
                            Hour = o.Hour;
                        };
                    StationNumber = o.StationNumber;
                }
            Temperature = o.Temperature
        }
    } |> List.ofSeq

let getLastObservationTime connectionString stationNumber interval = 
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
