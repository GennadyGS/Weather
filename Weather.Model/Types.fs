namespace Weather.Model

open System.Net
open System

type ObservationTime = 
    { Date: System.DateTime
      Hour: byte } 
    with
        member this.ToDateTime() = 
            this.Date.AddHours(float this.Hour)

type StationNumber = 
    | StationNumber of int

type ObservationHeader = 
    { StationNumber: StationNumber
      ObservationTime: ObservationTime 
      RequestTime: DateTime }

type Observation = 
    { Header : ObservationHeader
      Temperature: decimal }

type Failure =
    | DatabaseError of string
    | HttpError of (HttpStatusCode * string)
    | InvalidObservationHeaderFormat of string
    | InvalidObservationFormat of (ObservationHeader * string)
