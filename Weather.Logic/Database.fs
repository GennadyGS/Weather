﻿module Weather.Logic.Database

open System.Linq
open Weather
open Weather.Utils
open Weather.Model

let inline readDataContext (func : 'dc -> 'a -> IQueryable<'b>) = 
    fun connectionString a ->
        Utils.Database.readDataContext func connectionString a 
        |> Result.mapFailure DatabaseError

let inline writeDataContext (func : 'dc -> 'a -> unit) = 
    fun connectionString a ->
        Utils.Database.writeDataContext func connectionString a 
        |> Result.mapFailure DatabaseError
