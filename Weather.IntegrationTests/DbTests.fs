namespace Weather.IntegrationTests

open FSharp.Data.Sql.Providers

type DbTests () =
    let executeSqlCommand (sql : string) : int = 
        using 
            (MSSqlServer.createConnection Settings.ConnectionStrings.Weather)
            (fun connection -> 
                connection.Open()
                let command = MSSqlServer.createCommand sql connection
                command.ExecuteNonQuery())
    
    do
        ["Observations"]
        |> List.map ((sprintf "TRUNCATE TABLE %s") >> executeSqlCommand)
        |> ignore
    
