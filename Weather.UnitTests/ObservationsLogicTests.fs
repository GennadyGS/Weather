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
    let getLastObservationTime stationNumberArg intervalArg = 
        stationNumberArg =! stationNumber
        intervalArg =! interval
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
        lastObservationTime
        observations 
        saveObservations 
        stationNumber 
        interval = 
    
    // Restrictions
    (interval.From < interval.To) ==> lazy
    (lastObservationTime < interval.To.AddMinutes(1.0)) ==> lazy
    
    // Arrange
    let getLastObservationTime _ _ = 
        Some lastObservationTime

    let mutable fetchObservationsCalled = false
    let fetchObservations stationNumberArg intervalArg =
        stationNumberArg =! stationNumber
        intervalArg =! { interval with From = lastObservationTime.AddMinutes(1.0) }
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

[<Property>]
let ``FillNewData calls fetchObservations when last observation time is None`` 
        observations 
        saveObservations 
        stationNumber 
        interval = 
    
    // Restrictions
    (interval.From < interval.To) ==> lazy

    // Arrange
    let getLastObservationTime _ _ = 
        None

    let mutable fetchObservationsCalled = false
    let fetchObservations stationNumberArg intervalArg =
        stationNumberArg =! stationNumber
        intervalArg =! interval
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
