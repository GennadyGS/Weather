module Weather.UnitTests.ObservationsLogicTests

open FsCheck
open FsCheck.Xunit
open Swensen.Unquote
open Weather.Logic
open Weather.Utils

[<Property>]
let ``GetMissingInterval returns None when interval is empty`` 
        interval lastObservationTime = 
    
    (interval.From > interval.To) ==> lazy
    
    // Act
    let result = Observations.getMissingInterval interval (Some lastObservationTime)

    // Assert
    result =! None 

[<Property>]
let ``GetMissingInterval returns None when lastObservationTime is greater then interval.To`` 
        interval lastObservationTime = 
    
    // Restrictions
    (interval.From < interval.To) ==> lazy
    (lastObservationTime > interval.To.AddMinutes(1.0)) ==> lazy
    
    // Act
    let result = Observations.getMissingInterval interval (Some lastObservationTime)

    // Assert
    result =! None 

[<Property>]
let ``GetMissingInterval returns interval from lastObservationTime when it is inside interval`` 
        interval lastObservationTime  = 
    
    // Restrictions
    (interval.From < interval.To) ==> lazy
    (lastObservationTime < interval.To.AddMinutes(1.0)) ==> lazy
    (lastObservationTime > interval.From.AddMinutes(1.0)) ==> lazy
    
    // Act
    let result = Observations.getMissingInterval interval (Some lastObservationTime)

    // Assert
    result =! Some { interval with From = lastObservationTime.AddMinutes(1.0) }

[<Property>]
let ``GetMissingInterval returns input interval when lastObservationTime is less then interval.From`` 
        interval lastObservationTime  = 
    
    // Restrictions
    (interval.From < interval.To) ==> lazy
    (lastObservationTime < interval.From.AddMinutes(1.0)) ==> lazy
    
    // Act
    let result = Observations.getMissingInterval interval (Some lastObservationTime)

    // Assert
    result =! Some interval

[<Property>]
let ``GetMissingInterval returns interval when lastObservationTime is None`` 
        interval  = 
    
    // Restrictions
    (interval.From < interval.To) ==> lazy

    // Act
    let result = Observations.getMissingInterval interval None

    // Assert
    result =! Some interval
        