module Weather.Utils.Database

open System.Data.SqlClient
open System.Linq

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

let runQuerySafe query = 
    query
    |> handleSqlException Seq.toList

let inline saveChangesSafe dataContext = 
    dataContext
    |> handleSqlException saveChangesToDataContext

let inline readDataContext (func : 'dc -> IQueryable<'a>) = 
    createDataContext >> func >> runQuerySafe

let inline readDataContext2 (func : 'dc -> 'a -> IQueryable<'b>) = 
    fun connectionString a -> 
        readDataContext (fun dataContext -> func dataContext a) connectionString

let inline writeDataContext (func : 'dc -> unit) = 
    createDataContext >>
    fun dataContext -> 
        func dataContext
        saveChangesSafe dataContext

let inline writeDataContext2 (func : 'dc -> 'a -> unit) = 
    fun connectionString a ->
        writeDataContext (fun dataContext -> func dataContext a) connectionString
