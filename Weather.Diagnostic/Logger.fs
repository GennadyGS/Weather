module Weather.Diagnostic.Logger

open log4net

let private loggerName = "Logger"

let private logger = 
    LogManager.GetLogger(loggerName);

let configureLogger () =
    log4net.Config.XmlConfigurator.Configure() |> ignore

let logFatal (message : string) =
    logger.Fatal(message)
    ()

let logError (message : string) =
    logger.Error(message)
    ()

let logWarning (message : string) =
    logger.Warn(message)
    ()

let logDebug (message : string) =
    logger.Debug(message)
    ()

let logInfo (message : string) =
    logger.Info(message)
    ()
