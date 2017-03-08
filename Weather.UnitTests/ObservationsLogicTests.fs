module Weather.UnitTests.ObservationsLogicTests

open FsCheck
open FsCheck.Xunit
open Swensen.Unquote
open Weather.Logic
open Weather.Utils
open System

[<Property>]
let ``GetMissingInterval returns None when interval is empty`` 
        interval lastObservationTime = 
    
    // Arrange
    let minTimeSpan = TimeSpan.FromMinutes(1.0)
    (interval.From + minTimeSpan > interval.To) ==> lazy
    (minTimeSpan.Ticks > 0L) ==> lazy
    
    // Act
    let result = Observations.getMissingInterval minTimeSpan interval (Some lastObservationTime)

    // Assert
    result =! None 

[<Property>]
let ``GetMissingInterval returns None when lastObservationTime is greater then interval.To`` 
        interval lastObservationTime = 
    
    // Arrange
    let minTimeSpan = TimeSpan.FromMinutes(1.0)
    (interval.From + minTimeSpan < interval.To) ==> lazy
    (lastObservationTime > interval.To + minTimeSpan) ==> lazy
    
    // Act
    let result = Observations.getMissingInterval minTimeSpan interval (Some lastObservationTime)

    // Assert
    result =! None 

[<Property>]
let ``GetMissingInterval returns interval from lastObservationTime when it is inside interval`` 
        interval lastObservationTime  = 
    
    // Arrange
    let minTimeSpan = TimeSpan.FromMinutes(1.0)
    (interval.From + minTimeSpan < interval.To) ==> lazy
    (lastObservationTime > interval.From + minTimeSpan) ==> lazy
    (lastObservationTime < interval.To + minTimeSpan) ==> lazy
    
    // Act
    let result = Observations.getMissingInterval minTimeSpan interval (Some lastObservationTime)

    // Assert
    result =! Some { interval with From = lastObservationTime + minTimeSpan }

[<Property>]
let ``GetMissingInterval returns input interval when lastObservationTime is less then interval.From`` 
        interval lastObservationTime  = 
    
    // Arrange
    let minTimeSpan = TimeSpan.FromMinutes(1.0)
    (interval.From + minTimeSpan < interval.To) ==> lazy
    (lastObservationTime < interval.From + minTimeSpan) ==> lazy
    
    // Act
    let result = Observations.getMissingInterval minTimeSpan interval (Some lastObservationTime)

    // Assert
    result =! Some interval

[<Property>]
let ``GetMissingInterval returns interval when lastObservationTime is None`` 
        interval  = 
    
    // Arrange
    let minTimeSpan = TimeSpan.FromMinutes(1.0)
    (interval.From + minTimeSpan < interval.To) ==> lazy

    // Act
    let result = Observations.getMissingInterval minTimeSpan interval None

    // Assert
    result =! Some interval
        