module Weather.Utils.List

let mapAndPartition mapper list = 
  let rec partitionAux list outTrue outFalse =
    match list with 
    | [] -> List.rev outTrue, List.rev outFalse
    | x::xs -> 
        match mapper x with
        | True value -> partitionAux xs (value::outTrue) outFalse
        | False value -> partitionAux xs outTrue (value::outFalse)
  partitionAux list [] []   

let partition<'a, 'b> = 
    mapAndPartition (function
        | Success (successValue : 'a) -> True successValue
        | Failure (failureValue : 'b) -> False failureValue)

