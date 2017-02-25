module Weather.UnitTests.ObservationsLogicTests

open FsCheck
open FsCheck.Xunit
open Swensen.Unquote
open Weather.Logic
open System
open Weather.Utils

[<Property>]
let ``GetNewData returns empty list when last observation time plus 1 munute is greater then interval.To`` 
        maxObservationTime
        observations 
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
    let result = 
        Observations.getNewData 
            getLastObservationTime 
            fetchObservations
            stationNumber
            interval

    // Assert
    result =! [] 
    fetchObservationsCalled =! false

[<Property>]
let ``GetNewData returns fetched observations when last observation time plus 1 munute is lower then interval.To`` 
        lastObservationTime
        observations 
        stationNumber 
        interval = 
    
    // Restrictions
    (interval.From < interval.To) ==> lazy
    (lastObservationTime < interval.To.AddMinutes(1.0)) ==> lazy
    
    // Arrange
    let getLastObservationTime _ _ = 
        Some lastObservationTime

    let fetchObservations stationNumberArg intervalArg =
        stationNumberArg =! stationNumber
        intervalArg =! { interval with From = lastObservationTime.AddMinutes(1.0) }
        observations |> List.map Success

    // Act
    let result = 
        Observations.getNewData 
            getLastObservationTime 
            fetchObservations
            stationNumber
            interval

    // Assert
    result =! observations

[<Property>]
let ``GetNewData returns fetched observation when last observation time is None`` 
        observations 
        stationNumber 
        interval = 
    
    // Restrictions
    (interval.From < interval.To) ==> lazy

    // Arrange
    let getLastObservationTime _ _ = 
        None

    let fetchObservations stationNumberArg intervalArg =
        stationNumberArg =! stationNumber
        intervalArg =! interval
        observations |> List.map Success

    // Act
    let result = 
        Observations.getNewData 
            getLastObservationTime 
            fetchObservations
            stationNumber
            interval

    // Assert
    result =! observations
