module Weather.Utils.ResultList

open Weather.Utils

let getFailures results = 
    results 
    |> List.choose 
        (function
        | Success () -> None
        | Failure failure -> Some failure)