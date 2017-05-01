namespace Weather.IntegrationTests

open Weather.Diagnostic

type LoggingTestFixture() =
    do Logger.configureLogger ()
