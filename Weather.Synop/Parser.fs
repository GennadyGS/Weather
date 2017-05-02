module Weather.Synop.Parser

open Weather.Utils
open Weather.Utils.RegEx
open Weather.Utils.TryParser

let private tryParseSign = function
| Regex @"^([01])$" [Byte(sign)] ->
    Some (if sign = 0uy then 1m else -1m)
| _ -> None

let private (|Sign|_|) = tryParseSign

let private tryParseSignedDecimal = function
| Regex @"^([01])(\d{3})$" [Sign(sign); Int(digits)] ->
    Some (decimal digits / 10m * sign)
| _ -> None

let private (|SignedDecimal|_|) = tryParseSignedDecimal

let tryParseSynop = function
| Regex @"^(\d{5}) [\d\/]{5} [\d\/]{5} 1([01]\d{3})" 
    [Int(stationNumber); SignedDecimal(temperature)] -> 
        Some { StationNumber = stationNumber
               Temperature = temperature }
| _ -> None

let safeParseSynop str = 
    str
    |> tryParseSynop
    |> function
       | Some synop -> Success synop
       | None -> Failure <| sprintf "Invalid SYNOP string: %s" str
    