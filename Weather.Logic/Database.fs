module Weather.Logic.Database

open System.Linq
open Weather
open Weather.Utils
open Weather.Model

let runQuerySafe query = 
    query
    |> Utils.Database.runQuerySafe
    |> Result.mapFailure DatabaseError

let inline saveChangesSafe dataContext = 
    dataContext
    |> Utils.Database.saveChangesSafe
    |> Result.mapFailure DatabaseError

let inline readDataContext (func : 'dc -> IQueryable<'a>) = 
    Utils.Database.readDataContext func >> Result.mapFailure DatabaseError

let inline readDataContext2 (func : 'dc -> 'a -> IQueryable<'b>) = 
    fun connectionString a -> 
        readDataContext (fun dataContext -> func dataContext a) connectionString

let inline readDataContext3 (func : 'dc -> 'a -> 'b -> IQueryable<'c>) = 
    fun connectionString a b -> 
        readDataContext (fun dataContext -> func dataContext a b) connectionString

let inline writeDataContext (func : 'dc -> unit) = 
    Utils.Database.writeDataContext func >> Result.mapFailure DatabaseError

let inline writeDataContext2 (func : 'dc -> 'a -> unit) = 
    fun connectionString a ->
        writeDataContext (fun dataContext -> func dataContext a) connectionString

let inline writeDataContext3 (func : 'dc -> 'a -> 'b -> unit) = 
    fun connectionString a b ->
        writeDataContext (fun dataContext -> func dataContext a b) connectionString
