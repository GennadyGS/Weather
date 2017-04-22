module Weather.Utils.FailureHandling

let handleFailures handler results = 
    results
    |> List.choose (Result.mapFailureToOption handler)
