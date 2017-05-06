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

let inline mapContextReadFunc (func : 'dc -> IQueryable<'a>) = 
    let compositeFunc = createDataContext >> func
    fun connectionString -> 
        try
            compositeFunc connectionString 
            |> Seq.toList
            |> Success
        with
          | DatabaseFailure failure -> failure

let inline mapContextReadFunc2 (func : 'dc -> 'a -> IQueryable<'b>) = 
    fun connectionString a -> 
        mapContextReadFunc (fun dataContext -> func dataContext a) connectionString

let inline mapContextReadFunc3 (func : 'dc -> 'a -> 'b -> IQueryable<'c>) = 
    fun connectionString a b -> 
        mapContextReadFunc (fun dataContext -> func dataContext a b) connectionString

let inline mapContextUpdateFunc (func : 'dc -> 'a -> unit) = 
    fun connectionString arg ->
        try
            let dataContext = createDataContext connectionString 
            let result = func dataContext arg
            saveChangesToDataContext dataContext
            Success result
        with
          | DatabaseFailure failure -> failure
