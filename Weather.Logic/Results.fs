module Weather.Logic.Results

open Weather.Model
open Weather.Utils

let private partitionFailureResults = 
    Weather.Utils.List.mapAndPartition (function
        | InvalidObservationFormat value -> True value
        | InvalidHeaderFormat value -> False value)

let partitionResults results = 
    let (successResults, failureResults) = 
        Weather.Utils.List.partition results
    let (invalidObservationFormatResults, invalidHeaderFormatResults) = 
        partitionFailureResults failureResults
    { Success = successResults
      WithInvalidObservationFormat = invalidObservationFormatResults
      WithInvalidHeaderFormat = invalidHeaderFormatResults }


