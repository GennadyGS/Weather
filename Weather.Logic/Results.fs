module Weather.Logic.Results

open Weather.Model
open Weather.Utils

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
    { Success = successResults
      InvalidObservationFormatFailures = invalidObservationFormatFailures
      Failures = failures }


