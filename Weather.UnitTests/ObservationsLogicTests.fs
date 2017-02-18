module Weather.UnitTests.ObservationsLogicTests

open FsCheck
open FsCheck.Xunit
open Swensen.Unquote
open Weather.Logic
open System
open Weather.Utils

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

[<Property>]
let ``FillNewData does not call fetchObservations when last observation time plus 1 munute is greater then interval.To`` 
        maxObservationTime
        observations 
        saveObservations 
        stationNumber 
        interval = 
    
    // Restrictions
    (maxObservationTime > interval.To.AddMinutes(1.0)) ==> lazy
    
    // Arrange
    let getLastObservationTime _ _ = 
        Some maxObservationTime

    let mutable fetchObservationsCalled = false
    let fetchObservations _ _ =
        fetchObservationsCalled <- true
        observations

    // Act
    Observations.fillNewData 
        getLastObservationTime 
        fetchObservations
        saveObservations 
        stationNumber
        interval

    // Assert
    fetchObservationsCalled =! false

[<Property>]
let ``FillNewData calls fetchObservations when last observation time plus 1 munute is lower then interval.To`` 
        maxObservationTime
        observations 
        saveObservations 
        stationNumber 
        interval = 
    
    // Restrictions
    (maxObservationTime < interval.To.AddMinutes(1.0)) ==> lazy
    
    // Arrange
    let getLastObservationTime _ _ = 
        Some maxObservationTime

    let mutable fetchObservationsCalled = false
    let fetchObservations _ _ =
        fetchObservationsCalled <- true
        observations

    // Act
    Observations.fillNewData 
        getLastObservationTime 
        fetchObservations
        saveObservations 
        stationNumber
        interval

    // Assert
    fetchObservationsCalled =! true
