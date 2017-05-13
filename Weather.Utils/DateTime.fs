module Weather.Utils.DateTime

open System

let roundToTicks (dateTime : DateTime) (resulutionTicks : int64) = 
    DateTime(int64 (Math.Round(float(dateTime.Ticks) / float(resulutionTicks), MidpointRounding.AwayFromZero)) * resulutionTicks, dateTime.Kind)

let roundToHours (dateTime : DateTime) = 
    roundToTicks dateTime TimeSpan.TicksPerHour

let roundToMinutes (dateTime : DateTime) = 
    roundToTicks dateTime TimeSpan.TicksPerMinute

let roundToSeconds (dateTime : DateTime) = 
    roundToTicks dateTime TimeSpan.TicksPerSecond

let roundToMilliseconds (dateTime : DateTime) = 
    roundToTicks dateTime TimeSpan.TicksPerMillisecond

let max (dateTime1: DateTime) (dateTime2 : DateTime) = 
    DateTime(Math.Max(dateTime1.Ticks, dateTime2.Ticks))

let inside interval dateTime =
    dateTime >= interval.From && dateTime <= interval.To
