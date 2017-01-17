module Weather.Synop.Parser

open System
open Weather.Utils.RegEx
open Weather.Utils.TryParser

type Synop = 
    {
        Day : byte;
        Hour : byte;
        StationNumber : int;
        Temperature : decimal
    }

let tryParseSign = function
| Regex @"^([01])$" [Byte(sign)] ->
    Some (if sign = 0uy then 1m else -1m)
| _ -> None

let (|Sign|_|) = tryParseSign

let tryParseSignedDecimal = function
| Regex @"^([01])(\d{3})$" [Sign(sign); Int(digits)] ->
    Some (decimal digits / 10m * sign)
| _ -> None

let (|SignedDecimal|_|) = tryParseSignedDecimal

let tryParseSynop = function
| Regex @"^AAXX (\d{2})(\d{2})1 (\d{5}) [\d\/]{5} [\d\/]{5} 1([01]\d{3})" 
    [Byte(day); Byte(hour); Int(stationNumber); SignedDecimal(temperature)] -> 
        Some { 
            Day = day; 
            Hour = hour; 
            StationNumber = stationNumber; 
            Temperature = temperature
        }
| _ -> None

let (|Synop|_|) = tryParseSynop
    