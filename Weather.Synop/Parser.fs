module Weather.Synop.Parser

open System

type Synop = 
    {
        StationNumber : int;
        Temperature : decimal
    }

let tryParseSynop (string : string) : Synop option =
    raise (NotImplementedException())

let (|Synop|_|) = tryParseSynop
    