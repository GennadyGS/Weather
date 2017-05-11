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

let inline readDataContext (func : 'dc -> 'a -> IQueryable<'b>) = 
    createDataContext >>
    fun dataContext a -> 
        func dataContext a
        |> runQuerySafe

let inline writeDataContext (func : 'dc -> 'a -> unit) = 
    createDataContext >>
    fun dataContext a -> 
        func dataContext a
        saveChangesSafe dataContext

let inline writeDataContextForList (func : 'dc -> 'a -> unit) = 
    writeDataContext (fun dataContext -> List.map (func dataContext) >> ignore)
