module Weather.Logic.Database

open System
open System.Data.SqlClient
open Weather.Model
open Weather.Utils
open System.Linq

let (|DatabaseFailure|_|) (ex : Exception) = 
    match ex with
    | :? SqlException as sqlEx -> 
        sqlEx.ToString() 
        |> DatabaseError 
        |> Failure 
        |> Some
    | _ -> None
    
let inline private createDataContext connectionString : ^dc = 
    (^dc: (static member Create: string -> ^dc) connectionString)

let inline private saveChangesToDataContext (dataContext : ^dc) = 
    (^dc: (static member SaveChanges: ^dc -> unit) dataContext)

let runQuerySafe (query : IQueryable<'a>) = 
    try
        query
        |> Seq.toList
        |> Success
    with
        | DatabaseFailure failure -> failure

let inline saveChangesSafe dataContext = 
    try
        saveChangesToDataContext dataContext
        Success ()
    with
        | DatabaseFailure failure -> failure

let inline readDataContext (func : 'dc -> IQueryable<'a>) = 
    createDataContext >> func >> runQuerySafe

let inline readDataContext2 (func : 'dc -> 'a -> IQueryable<'b>) = 
    fun connectionString a -> 
        readDataContext (fun dataContext -> func dataContext a) connectionString

let inline readDataContext3 (func : 'dc -> 'a -> 'b -> IQueryable<'c>) = 
    fun connectionString a b -> 
        readDataContext (fun dataContext -> func dataContext a b) connectionString

let inline writeDataContext (func : 'dc -> unit) = 
    createDataContext >>
    fun dataContext -> 
        func dataContext
        saveChangesSafe dataContext

let inline writeDataContext2 (func : 'dc -> 'a -> unit) = 
    fun connectionString a ->
        writeDataContext (fun dataContext -> func dataContext a) connectionString
