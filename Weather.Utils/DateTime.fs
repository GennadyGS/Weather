module Weather.Utils.DateTime

open System

let roundToHours (dateTime : DateTime) = 
    let updated = dateTime.AddMinutes(30.0)
    DateTime(updated.Year, updated.Month, updated.Day, updated.Hour, 0, 0, dateTime.Kind);

let max (dateTime1: DateTime) (dateTime2 : DateTime) = 
    DateTime(Math.Max(dateTime1.Ticks, dateTime2.Ticks))

let inside interval dateTime =
    dateTime >= interval.From && dateTime <= interval.To
