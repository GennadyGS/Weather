module Weather.Logic.Database

open System
open System.Data.SqlClient
open Weather.Model
open Weather.Utils

let (|DatabaseFailure|_|) (ex : Exception) = 
    match ex with
    | :? SqlException as sqlEx -> 
        sqlEx.ToString() 
        |> DatabaseError 
        |> Failure 
        |> Some
    | _ -> None
    

