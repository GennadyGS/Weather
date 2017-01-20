module Weather.Loader.Loader

open FSharp.Data.Sql
open Weather.Model

let [<Literal>] SchemaConnectionString =
    "Data Source=gennadygs.database.windows.net;Initial Catalog=Weather;Integrated Security=False;User ID=gennadygs;Password=zl0zYH`};Connect Timeout=60;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"

type SqlProvider = 
    SqlDataProvider<
        ConnectionString = SchemaConnectionString,
        UseOptionTypes = true>

let private insertObservation 
        (observationsTable : SqlProvider.dataContext.dboSchema.``dbo.Observations``) 
        (observation: Observation) =
    let row = observationsTable.Create()
    row.StationNumber <- observation.StationNumber
    row.Date <- observation.Time.Date
    row.Hour <- observation.Time.Hour
    row.Temperature <- observation.Temperature

let saveObservations (observations: Observation seq) =
    let dataContext = SqlProvider.GetDataContext()
    let observationsTable = dataContext.Dbo.Observations
    observations |> Seq.map (insertObservation observationsTable) |> Seq.toArray |> ignore
    dataContext.SubmitUpdates()

let getObservations () = 
    let dataContext = SqlProvider.GetDataContext()
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
    } 


