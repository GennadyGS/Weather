namespace Weather.TestUtils

open FsCheck
open Weather.Utils
open System

type SingleLineString = SingleLineString of string with
    member x.Get = match x with SingleLineString r -> r
    override x.ToString() = x.Get

type Generators =
    static member Int64() =
        { new Arbitrary<int64>() with
            override x.Generator = Arb.generate<int> |> Gen.map int64 }

    static member Version() =
        Arb.generate<byte>
        |> Gen.map int
        |> Gen.four
        |> Gen.map (fun (ma, mi, bu, re) -> Version(ma, mi, bu, re))
        |> Arb.fromGen

    static member SingleLineString() =
        Arb.Default.String()
        |> Arb.filter (fun s -> (s <> null))
        |> Arb.filter (fun s -> (String.length s > 10))
        |> Arb.filter (fun s -> Array.length (String.split [|'\r'; '\n'|] s) <= 1)
        |> Arb.convert SingleLineString string
