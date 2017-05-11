module Weather.Composition.FailureHandling

open Weather
open Weather.Diagnostic

let logFailure = 
    Logic.FailureHandling.logFailure Logger.logError
