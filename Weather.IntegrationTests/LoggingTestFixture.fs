namespace Weather.IntegrationTests

open System
open log4net

type LoggingTestFixture() =
    do
        log4net.Config.XmlConfigurator.Configure() |> ignore

    interface IDisposable with
        member __.Dispose() = 
            ()        