module Weather.Loader.Loader

open FSharp.Data.Sql

let [<Literal>] SchemaConnectionString =
    "Data Source=gennadygs.database.windows.net;Initial Catalog=Weather;Integrated Security=False;User ID=gennadygs;Password=zl0zYH`};Connect Timeout=60;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"

type SqlProvider = SqlDataProvider<ConnectionString = SchemaConnectionString>

let dataContext = SqlProvider.GetDataContext()

let observationsTable = dataContext.Dbo.Observations

let insertRow () =
    let now = System.DateTime.Now
    let row = observationsTable.Create()
    row.StationNumber <- 33345
    row.Date <- now
    row.Hour <- byte (now.Hour)
    dataContext.SubmitUpdates()

let getObservations () = 
    query {
        for o in observationsTable do
        select (o.Date, o.Hour, o.StationNumber) 
    } 
    |> Seq.toList

