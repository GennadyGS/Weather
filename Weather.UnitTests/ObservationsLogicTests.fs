module Weather.UnitTests.ObservationsLogicTests

open FsCheck.Xunit
open Swensen.Unquote
open Weather.Logic

[<Property>]
let ``FillNewData calls getMaxObservationTime`` 
        lastObservationTime
        fetchObservations
        saveObservations 
        stationNumber 
        interval = 
    
    // Arrange
    let mutable getLastObservationTimeCalled = false
    let getLastObservationTime _ _ = 
        getLastObservationTimeCalled <- true
        lastObservationTime

    // Act
    Observations.fillNewData 
        getLastObservationTime 
        fetchObservations
        saveObservations 
        stationNumber
        interval

    // Assert
    getLastObservationTimeCalled =! true
