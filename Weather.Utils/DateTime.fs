module Weather.Utils.DateTime

open System

let max (dateTime1: DateTime) (dateTime2 : DateTime) = 
    DateTime(Math.Max(dateTime1.Ticks, dateTime2.Ticks))

let inside interval dateTime =
    dateTime >= interval.From && dateTime <= interval.To
