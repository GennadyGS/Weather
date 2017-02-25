module Weather.UnitTests.ObservationsLogicTests

open FsCheck
open FsCheck.Xunit
open Swensen.Unquote
open Weather.Logic
open System
open Weather.Utils
open Weather.Utils.DateTime
open Weather.Model
open Weather.Utils.Result

let private filterObservationsByInterval interval observations
    = observations 
    |> List.filter (fun observation -> inside interval (observation.Time.ToDateTime())) 

[<Property>]
let ``GetNewData throws exception interval is empty`` 
        stationNumber 
        interval = 
    
    // Restrictions
    (interval.From > interval.To) ==> lazy

    let getLastObservationTime (_ : int) (_ : DateTimeInterval) : DateTime option = 
        failwith "Should not be called"
        None
    
    let fetchObservations (_ : int) (_ : DateTimeInterval) : Result<Observation, string> list = 
        failwith "Should not be called"
        []
    
    // Act & Assert
    raises<ArgumentException> 
        <@ Observations.getNewData 
            getLastObservationTime 
            fetchObservations
            stationNumber
            interval @>

[<Property>]
let ``GetNewData returns empty list when last observation time is after interval.To`` 
        lastObservationTime
        observations 
        stationNumber 
        interval = 
    
    // Restrictions
    (interval.From < interval.To) ==> lazy
    (lastObservationTime > interval.To.AddMinutes(1.0)) ==> lazy
    
    // Arrange
    let getLastObservationTime _ _ = 
        Some lastObservationTime

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
let ``GetNewData returns fetched observations from last observation time to interval.To when it is inside interval`` 
        lastObservationTime
        observations 
        stationNumber 
        interval = 
    
    // Restrictions
    (interval.From < interval.To) ==> lazy
    (lastObservationTime < interval.To.AddMinutes(1.0)) ==> lazy
    (lastObservationTime > interval.From.AddMinutes(1.0)) ==> lazy
    
    // Arrange
    let getLastObservationTime _ _ = 
        Some lastObservationTime

    let expectedActualInterval = { interval with From = lastObservationTime.AddMinutes(1.0) }

    let fetchObservations stationNumberArg intervalArg =
        stationNumberArg =! stationNumber
        intervalArg =! expectedActualInterval
        observations |> List.map Success

    // Act
    let result = 
        Observations.getNewData 
            getLastObservationTime 
            fetchObservations
            stationNumber
            interval

    // Assert
    result =! (observations |> filterObservationsByInterval expectedActualInterval)

[<Property>]
let ``GetNewData returns fetched observations from interval when last observation time is before interval.From`` 
        lastObservationTime
        observations 
        stationNumber 
        interval = 
    
    // Restrictions
    (interval.From < interval.To) ==> lazy
    (lastObservationTime < interval.From.AddMinutes(1.0)) ==> lazy
    
    // Arrange
    let getLastObservationTime _ _ = 
        Some lastObservationTime

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
    result =! (observations |> filterObservationsByInterval interval)

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
    result =! (observations |> (filterObservationsByInterval interval))

[<Property>]
let ``GetNewData returns only success fetched observations`` 
        observations 
        stationNumber 
        interval = 
    
    // Restrictions
    (interval.From < interval.To) ==> lazy

    let getLastObservationTime _ _ = 
        None

    let fetchObservations _ _ =
        observations

    // Act
    let result = 
        Observations.getNewData 
            getLastObservationTime 
            fetchObservations
            stationNumber
            interval

    // Assert
    result =! (observations 
        |> filterSuccess
        |> filterObservationsByInterval interval)
        