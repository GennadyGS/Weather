module Weather.Utils.Database

open System.Data.SqlClient

let inline createDataContext connectionString : ^dc = 
    (^dc: (static member Create: string -> ^dc) connectionString)

let inline saveChangesToDataContext (dataContext : ^dc) = 
    (^dc: (static member SaveChanges: ^dc -> unit) dataContext)

let handleSqlException func =
    fun arg -> 
        try
            Success (func arg)
        with
            | :? SqlException as e -> Failure (e.ToString())
