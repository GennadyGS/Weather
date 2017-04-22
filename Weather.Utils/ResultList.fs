module Weather.Utils.ResultList

open Weather.Utils

let combineSuccesses results =
    let (successes, falures) = ListPartition.partition results
    let successList = if not (List.isEmpty successes) then [Success successes] else []
    successList @ (falures |> List.map Failure)
    


