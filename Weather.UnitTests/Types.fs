namespace Weather.UnitTests

open FsCheck
open Weather.Utils
open System

type SingleLineString = SingleLineString of string with
    member x.Get = match x with SingleLineString r -> r
    override x.ToString() = x.Get

type Generators =
    static member SingleLineString() =
        Arb.Default.String()
        |> Arb.filter (fun s -> not (String.IsNullOrEmpty s))
        |> Arb.filter (fun s -> s |> String.split [|'\r'; '\n'|] |> Array.length = 1)
        |> Arb.convert SingleLineString string
