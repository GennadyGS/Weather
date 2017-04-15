module Weather.Logic.Results

open Weather.Model
open Weather.Utils

let mapToList func result = 
    match result with
        | Success success -> func success
        | Failure failure -> [Failure failure]

let private partitionFailureResults = 
    Weather.Utils.List.mapAndPartition (function
        | InvalidObservationFormat value -> True value
        | InvalidHeaderFormat value -> False value
        | DatabaseError value -> False value)

let partitionResults results = 
    let (successResults, failureResults) = 
        Weather.Utils.List.partition results
    let (invalidObservationFormatFailures, failures) = 
        partitionFailureResults failureResults
    (successResults, invalidObservationFormatFailures, failures)
