module Weather.Loader.Loader

open FSharp.Data.Sql
open Weather.Model

let [<Literal>] SchemaConnectionString =
    "Data Source=gennadygs.database.windows.net;Initial Catalog=Weather;Integrated Security=False;User ID=gennadygs;Password=zl0zYH`};Connect Timeout=60;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"

type SqlProvider = SqlDataProvider<
    ConnectionString = SchemaConnectionString,
    UseOptionTypes = true>

let dataContext = SqlProvider.GetDataContext()

let observationsTable = dataContext.Dbo.Observations

let insertObservation (observation: Observation) =
    let row = observationsTable.Create()
    row.StationNumber <- observation.StationNumber
    row.Date <- observation.Date
    row.Hour <- observation.Hour

let saveObservations (observations: Observation seq) =
    observations |> Seq.map insertObservation |> ignore
    dataContext.SubmitUpdates()

let getObservations () = 
    query {
        for o in observationsTable do
        select {
            Date = o.Date; 
            Hour = o.Hour;
            StationNumber = o.StationNumber;
            Temperature = o.Temperature
        }
    } 


