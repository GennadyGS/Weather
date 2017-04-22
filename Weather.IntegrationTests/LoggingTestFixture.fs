namespace Weather.IntegrationTests

open System
open Weather.Diagnostic

type LoggingTestFixture() =
    do
        Logger.configureLogger ()

    interface IDisposable with
        member __.Dispose() = 
            ()        